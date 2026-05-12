using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class OrigamiLoader : MonoBehaviour
{
    [Header("模型文件")]
    public string jsonPath = "Models/miura0.json";
    public Material defaultMaterial;

    [Header("可视化设置")]
    public bool showCreases = true;
    public bool useCustomColors = true;

    [Header("折痕材质")]
    public Material creaseMaterial; // ✅ 新增字段（在 Inspector 中设置）

    [Header("UI 控制")]
    public Slider foldSlider;
    public Text modelNameText;
    public Text modelDescriptionText;

    private OrigamiModel model;
    private List<Vector3> vertices = new();
    private Dictionary<int, GameObject> faceObjects = new();
    private List<HingeJoint> hinges = new();
    private GameObject creaseParent;
    private List<GameObject> faceListObjects = new();

    // ✅ 三种预生成材质（减少内存消耗）
    private Material mountainMat;
    private Material valleyMat;
    private Material boundaryMat;
    private List<GameObject> creaseLineObjects = new();


    void Start()
    {
        if (foldSlider != null)
            foldSlider.onValueChanged.AddListener(OnFoldSliderChanged);

        LoadModel();
    }

    private void OnFoldSliderChanged(float value)
    {
        var controller = GetComponent<OrigamiController>();
        if (controller != null)
            controller.foldProgress = value;
    }

    public void LoadModel(string customPath = null)
    {
        CleanupCurrentModel();

        // 传入的路径可能只包含文件名或者相对路径
        string path = customPath ?? jsonPath;

        string jsonFullPath;
        if (Path.IsPathRooted(path))
        {
            jsonFullPath = path;
        }
        else
        {
            bool hasSeparator = path.IndexOfAny(new[] { '/', '\\' }) >= 0;
            jsonFullPath = hasSeparator
                ? Path.Combine(Application.dataPath, path)
                : Path.Combine(Application.dataPath, "Models", path);
        }

        if (!File.Exists(jsonFullPath))
        {
            Debug.LogError($"❌ JSON 文件不存在: {jsonFullPath}");
            return;
        }

        string json = File.ReadAllText(jsonFullPath);
        model = JsonUtility.FromJson<OrigamiModel>(json);

        if (model == null)
        {
            Debug.LogError("❌ JSON 解析失败");
            return;
        }

        UpdateUIInfo();

        vertices.Clear();
        if (model.vertices != null)
        {
            for (int i = 0; i < model.vertices.Count; i++)
            {
                var v = model.vertices[i];
                vertices.Add(new Vector3(v.x, v.y, v.z));
            }
        }

        if (model.faces != null && model.faces.Count > 0)
            CreateFaces();

        if (showCreases && model.creases != null && model.creases.Count > 0)
            DrawCreases();


        CreateCreasesAndConnections();
    }


    private void UpdateUIInfo()
    {
        if (modelNameText != null)
            modelNameText.text = model.name;

        if (modelDescriptionText != null)
            modelDescriptionText.text = model.description;
    }

    private void CleanupCurrentModel()
    {
        Debug.Log("[OrigamiLoader] 清理当前模型");

        foreach (var obj in faceObjects.Values)
        {
            if (obj != null)
                Destroy(obj);
        }

        if (creaseParent != null)
        {
            Destroy(creaseParent);
            creaseParent = null;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child != null)
                Destroy(child.gameObject);
        }

        if (mountainMat != null) { Destroy(mountainMat); mountainMat = null; }
        if (valleyMat != null) { Destroy(valleyMat); valleyMat = null; }
        if (boundaryMat != null) { Destroy(boundaryMat); boundaryMat = null; }

        faceObjects.Clear();
        hinges.Clear();
        vertices.Clear();
        faceListObjects.Clear();
        for (int i = 0; i < creaseLineObjects.Count; i++)
        {
            var go = creaseLineObjects[i];
            if (go != null) Destroy(go);
        }
        creaseLineObjects.Clear();
    }

    private void CreateFaces()
    {
        foreach (var face in model.faces)
        {
            GameObject obj = new GameObject($"Face_{face.id}");
            obj.transform.parent = transform;

            Mesh mesh = new Mesh();
            Vector3[] fVerts = new Vector3[face.vertices.Count];
            int[] tris = new int[(face.vertices.Count - 2) * 3];

            for (int i = 0; i < face.vertices.Count; i++)
                fVerts[i] = vertices[face.vertices[i] - 1];

            for (int i = 0; i < face.vertices.Count - 2; i++)
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }

            mesh.vertices = fVerts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            var renderer = obj.AddComponent<MeshRenderer>();

            // ✅ 仅使用 Inspector 中的 defaultMaterial
            // 移除所有 JSON 和自定义颜色逻辑
            Material faceMat = defaultMaterial != null
                ? new Material(defaultMaterial)  // 复制 Inspector 中的材质
                : new Material(Shader.Find("Standard"));

            renderer.material = faceMat;  // 直接使用材质

            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass = face.rigid ? 0.2f : 0.1f;
            rb.drag = face.rigid ? 0.8f : 0.5f;
            rb.angularDrag = face.rigid ? 0.8f : 0.5f;

            // 在CreateFaces()方法中，添加完Rigidbody后
            MeshCollider collider = obj.AddComponent<MeshCollider>();
            collider.isTrigger = false; // 通常不需要触发器

            faceObjects[face.id] = obj;
            faceListObjects.Add(obj);
        }
    }

    private void CreateCreasesAndConnections()
    {
        Debug.Log($"[OrigamiLoader] CreateCreasesAndConnections: 使用折痕创建铰链，折痕数={(model.creases != null ? model.creases.Count : 0)}");
        if (model.creases == null || model.creases.Count == 0 || model.faces == null || model.faces.Count == 0)
        {
            Debug.Log("[OrigamiLoader] 无折痕或无面，跳过铰链创建");
            return;
        }

        for (int i = 0; i < model.creases.Count; i++)
        {
            var crease = model.creases[i];
            if (crease.type == OrigamiCrease.Type.Boundary)
            {
                Debug.Log($"[OrigamiLoader] 跳过边界折痕 crease#{crease.id}");
                continue;
            }

            List<int> touchingFaces = new List<int>();
            for (int fIdx = 0; fIdx < model.faces.Count; fIdx++)
            {
                var f = model.faces[fIdx];
                if (f.vertices != null && f.vertices.Contains(crease.v1) && f.vertices.Contains(crease.v2))
                    touchingFaces.Add(fIdx);
            }

            if (touchingFaces.Count < 2)
            {
                Debug.LogWarning($"[OrigamiLoader] 折痕#{crease.id} 未找到两侧面，找到={touchingFaces.Count}，跳过铰链创建");
                continue;
            }

            // 获取两个相邻面
            var faceA = faceListObjects[touchingFaces[0]];
            var faceB = faceListObjects[touchingFaces[1]];
            Rigidbody rbA = faceA.GetComponent<Rigidbody>();
            Rigidbody rbB = faceB.GetComponent<Rigidbody>();

            // 计算折痕方向（从v1到v2）
            Vector3 v1 = vertices[crease.v1 - 1];
            Vector3 v2 = vertices[crease.v2 - 1];
            Vector3 creaseDir = (v2 - v1).normalized;
            Vector3 hingePoint = (v1 + v2) * 0.5f;

            // 获取 OrigamiFace 数据结构
            OrigamiFace faceAData = model.faces[touchingFaces[0]];
            OrigamiFace faceBData = model.faces[touchingFaces[1]];

            // 直接用顶点顺序判断左右
            bool isFaceAOnLeft = IsFaceOnLeft_ByVertexOrder(faceAData, crease.v1, crease.v2);
            bool isFaceBOnLeft = !isFaceAOnLeft; // 两侧必然相反


            // 根据折痕类型决定铰链挂载面
            GameObject hingeOwner;
            if (crease.type == OrigamiCrease.Type.Mountain)
            {
                // 山折：铰链挂载在左侧面
                hingeOwner = isFaceAOnLeft ? faceA : faceB;
            }
            else
            {
                // 谷折：铰链挂载在右侧面
                hingeOwner = isFaceAOnLeft ? faceB : faceA;
            }

            // 创建铰链
            var hinge = hingeOwner.AddComponent<HingeJoint>();
            Rigidbody connectedRb = (hingeOwner == faceA) ? rbB : rbA;
            hinge.connectedBody = connectedRb;
            hinge.anchor = hingeOwner.transform.InverseTransformPoint(hingePoint);
            hinge.axis = hingeOwner.transform.InverseTransformDirection(creaseDir);
            hinge.useLimits = true;
            hinge.useSpring = true;
            hinge.enableCollision = false;

            // 设置铰链限制
            JointLimits lim = new JointLimits
            {
                min = crease.minAngle,
                max = crease.maxAngle,
                bounciness = 0f,
                contactDistance = 0f
            };
            hinge.limits = lim;

            // 设置弹簧
            JointSpring spring = new JointSpring
            {
                spring = crease.stiffness * 50f,
                damper = 5f,
                targetPosition = crease.restAngle
            };
            hinge.spring = spring;

            /*Debug.Log($"[OrigamiLoader] 创建铰链: crease#{crease.id} 类型={crease.type}, " +
                     $"挂载面={hingeOwner.name}, 左侧面={isFaceAOnLeft ? faceA.name : faceB.name}, " +
                     $"右侧面={isFaceAOnLeft ? faceB.name : faceA.name}");
            */

            hinges.Add(hinge);

            var controller = GetComponent<OrigamiController>();
            if (controller != null)
            {
                controller.AddHinge(hinge);
                Debug.Log($"[OrigamiLoader] 已注册到控制器: hinge of crease#{crease.id}");
            }
        }
    }

    // 计算面的法向量（基于Mesh的顶点顺序）
    /*private Vector3 CalculateFaceNormal(GameObject face)
    {
        MeshFilter mf = face.GetComponent<MeshFilter>();
        if (mf == null || mf.mesh == null || mf.mesh.vertices.Length < 3)
            return Vector3.up; // 默认向上

        Vector3[] vertices = mf.mesh.vertices;

        // 使用前三个顶点计算法向量
        Vector3 v0 = vertices[0];
        Vector3 v1 = vertices[1];
        Vector3 v2 = vertices[2];

        // 计算法向量（注意：这里假设顶点顺序是逆时针）
        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;
        Vector3 normal = Vector3.Cross(edge1, edge2).normalized;

        // 确保法向量向上（Z轴为高度）
        if (normal.z < 0)
            normal = -normal;

        return normal;
    }
    */
    // 判断面是否在折痕左侧
    private bool IsFaceOnLeft_ByVertexOrder(OrigamiFace face, int v1, int v2)
    {
        var verts = face.vertices;
        int idx1 = verts.IndexOf(v1);
        int idx2 = verts.IndexOf(v2);
        int N = verts.Count;

        if (idx1 < 0 || idx2 < 0)
            return false;

        // 判断 idx2 是否是 idx1 顺时针的下一个
        bool ccw = ((idx2 - idx1 + N) % N) == 1;

        // 面顶点顺序为 CCW，所以顺序一致代表面在 creaseDir 左侧
        return ccw;
    }


    private void InitCreaseMaterials(Color mountainColor, Color valleyColor)
    {
        // ✅ 修复1：只声明一次着色器变量
        string unlitShader = "Universal Render Pipeline/Unlit";

        if (creaseMaterial != null)
        {
            // ✅ 修复2：使用 Inspector 材质创建副本
            if (mountainMat == null)
            {
                mountainMat = new Material(creaseMaterial);
                mountainMat.color = mountainColor;
            }
            if (valleyMat == null)
            {
                valleyMat = new Material(creaseMaterial);
                valleyMat.color = valleyColor;
            }
            if (boundaryMat == null)
            {
                boundaryMat = new Material(creaseMaterial);
                boundaryMat.color = Color.black;
            }
        }
        else
        {
            // ✅ 修复3：直接使用已声明的 unlitShader
            if (mountainMat == null)
            {
                mountainMat = new Material(Shader.Find(unlitShader));
                mountainMat.color = mountainColor;
            }
            if (valleyMat == null)
            {
                valleyMat = new Material(Shader.Find(unlitShader));
                valleyMat.color = valleyColor;
            }
            if (boundaryMat == null)
            {
                boundaryMat = new Material(Shader.Find(unlitShader));
                boundaryMat.color = Color.black;
            }
        }
    }

    private void DrawCreases()
    {
        if (!showCreases)
            return;

        Color mountainColor = Color.red;
        Color valleyColor = Color.blue;

        if (model != null)
        {
            if (!string.IsNullOrEmpty(model.mountainCreaseColor))
                ColorUtility.TryParseHtmlString(model.mountainCreaseColor, out mountainColor);
            if (!string.IsNullOrEmpty(model.valleyCreaseColor))
                ColorUtility.TryParseHtmlString(model.valleyCreaseColor, out valleyColor);
        }

        InitCreaseMaterials(mountainColor, valleyColor);

        foreach (var crease in model.creases)
        {
            Vector3 p1 = vertices[crease.v1 - 1];
            Vector3 p2 = vertices[crease.v2 - 1];

            GameObject attachFace = null;
            for (int fIdx = 0; fIdx < model.faces.Count; fIdx++)
            {
                var f = model.faces[fIdx];
                if (f.vertices != null && f.vertices.Contains(crease.v1) && f.vertices.Contains(crease.v2))
                {
                    attachFace = (fIdx < faceListObjects.Count) ? faceListObjects[fIdx] : null;
                    break;
                }
            }

            GameObject lineObj = new GameObject($"Crease_{crease.id}");
            lineObj.transform.parent = attachFace != null ? attachFace.transform : transform;

            var lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = lr.endWidth = crease.width > 0 ? crease.width : model.defaultCreaseWidth;
            lr.useWorldSpace = attachFace == null;
            if (lr.useWorldSpace)
            {
                lr.SetPosition(0, p1);
                lr.SetPosition(1, p2);
            }
            else
            {
                lr.SetPosition(0, attachFace.transform.InverseTransformPoint(p1));
                lr.SetPosition(1, attachFace.transform.InverseTransformPoint(p2));
            }

            Material m;
            switch (crease.type)
            {
                case OrigamiCrease.Type.Mountain:
                    m = mountainMat;
                    break;
                case OrigamiCrease.Type.Valley:
                    m = valleyMat;
                    break;
                case OrigamiCrease.Type.Boundary:
                    m = boundaryMat;
                    break;
                default:
                    m = valleyMat;
                    break;
            }
            lr.material = m;
            creaseLineObjects.Add(lineObj);
        }
    }

    public void SetShowCreases(bool on)
    {
        showCreases = on;
        for (int i = 0; i < creaseLineObjects.Count; i++)
        {
            var go = creaseLineObjects[i];
            if (go != null) Destroy(go);
        }
        creaseLineObjects.Clear();
        if (on)
            DrawCreases();
    }

    // 公共方法
    public void ReloadModel() => LoadModel();
    public void LoadModelByName(string modelName)
    {
        string name = modelName.EndsWith(".json") ? modelName : modelName + ".json";
        LoadModel(Path.Combine("Models", name));
    }
}
