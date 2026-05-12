using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 修复版：只基于 点 + 折痕(边) 的折痕图编辑器（不含面操作）。
/// 修复：1. 折痕类型统一用枚举 2. 顶点删除后索引自动修正 3. 补全属性编辑逻辑
/// </summary>
public class CreasePatternEditor : MonoBehaviour
{
    #region Inspector Fields (UI & Visual)
    [Header("编辑器设置")]
    public Camera editorCamera;
    public float gridSize = 1f;
    public float snapDistance = 0.25f;
    public Material vertexMaterial;
    public Material creaseMaterial; // 用于默认折痕（会以类型材质覆盖）
    public float vertexSize = 0.12f;
    public float creaseWidth = 0.04f;

    [Header("UI 元素")]
    public Button addVertexButton;
    public Button addCreaseButton;
    public Button moveVertexButton;
    public Button deleteVertexButton;
    public Button deleteCreaseButton;
    public Button editCreaseAttrButton;
    public Button saveButton;
    public Button loadButton;
    public TMP_InputField fileNameInput;
    public TMP_InputField loadFileInput;

    [Header("折痕类型选择")]
    public Toggle mountainToggle;
    public Toggle valleyToggle;
    public Toggle boundaryToggle;
    public Toggle flatToggle;

    [Header("折痕属性面板（可选）")]
    public CreaseAttributePanel creaseAttributePanel;

    [SerializeField] private OrigamiLoader loader;   // ← 引用你的 OrigamiLoader

    #endregion

    #region Internal Data Models (self-contained for JSON)
    [System.Serializable]
    public class OrigamiVertex
    {
        public int id;
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    public class OrigamiCrease
    {
        public int id;
        public int v1; // 1-based indices
        public int v2;
        public CreaseType creaseType; // FIX: 替换布尔字段为枚举
        public float angle; // 目标折叠角度（度数）
        public float width;
        // 👇 新增以下两行
        public float minAngle = -180f; // 默认最小角度
        public float maxAngle = 180f;  // 默认最大角度
    }

    [System.Serializable]
    public class OrigamiMaterial
    {
        public string name;
        public float metallic;
        public float smoothness;
        public bool doubleSided;
    }

    [System.Serializable]
    public class OrigamiConnection
    {
        public int crease_id;
        public int faceA;
        public int faceB;
    }

    [System.Serializable]
    public class OrigamiModel
    {
        public string name;
        public string description;
        public List<OrigamiVertex> vertices;
        public List<OrigamiCrease> creases;
        public List<OrigamiConnection> connections;
        public OrigamiMaterial material;
        public float defaultCreaseWidth;
        // FIX: 删除冗余颜色字段（材质已实时计算）
    }

    // FIX: 折痕类型枚举（替代原布尔字段）
    public enum CreaseType { Mountain, Valley, Boundary, Flat }
    #endregion

    #region Runtime State
    private enum EditorMode { None, AddVertex, AddCrease, MoveVertex, DeleteVertex, DeleteCrease, EditCreaseAttributes }
    private EditorMode currentMode = EditorMode.None;

    private List<OrigamiVertex> vertices = new List<OrigamiVertex>();
    private List<OrigamiCrease> creases = new List<OrigamiCrease>();

    private List<GameObject> vertexObjects = new List<GameObject>();
    private List<GameObject> creaseObjects = new List<GameObject>();

    private int selectedVertexIndex = -1;
    private int startVertexIndex = -1;
    private int selectedCreaseIndex = -1;

    private Material mountainMat;
    private Material valleyMat;
    private Material boundaryMat;
    private Material flatMat;


    public List<Vector3> currentVertices;
    public List<OrigamiCrease> currentCreases;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (editorCamera == null) editorCamera = Camera.main;
    }
    void Start()
    {
        // UI 挂钩
        addVertexButton?.onClick.AddListener(() => SetMode(EditorMode.AddVertex));
        addCreaseButton?.onClick.AddListener(() => SetMode(EditorMode.AddCrease));
        moveVertexButton?.onClick.AddListener(() => SetMode(EditorMode.MoveVertex));
        deleteVertexButton?.onClick.AddListener(() => SetMode(EditorMode.DeleteVertex));
        deleteCreaseButton?.onClick.AddListener(() => SetMode(EditorMode.DeleteCrease));
        editCreaseAttrButton?.onClick.AddListener(() => SetMode(EditorMode.EditCreaseAttributes));
        saveButton?.onClick.AddListener(SaveToJson);
        loadButton?.onClick.AddListener(LoadFromJson);

        // 使用统一材质初始化逻辑
        InitCreaseMaterials(Color.red, Color.blue);
    }
    // ✅ 替换原 Start() 中的材质初始化部分（新增方法 + 修复细节）
    private void InitCreaseMaterials(Color mountainColor, Color valleyColor)
    {
        string unlitShader = "Universal Render Pipeline/Unlit";

        Shader shader = null;

        // 优先使用用户在 Inspector 中的 creaseMaterial
        if (creaseMaterial != null)
        {
            shader = creaseMaterial.shader;
        }
        else
        {
            shader = Shader.Find(unlitShader);
            if (shader == null)
            {
                Debug.LogError("[CreaseEditor] 找不到 URP Unlit Shader！");
                return;
            }
        }

        // 创建材质副本（避免污染 inspector 材质）
        mountainMat = new Material(shader);
        mountainMat.color = mountainColor;

        valleyMat = new Material(shader);
        valleyMat.color = valleyColor;

        boundaryMat = new Material(shader);
        boundaryMat.color = new Color32(51, 51, 51, 255); // 333333 对应的 255 通道

        flatMat = new Material(shader);
        flatMat.color = Color.gray;
    }

    void Update()
    {
        if (currentMode == EditorMode.None) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = GetMouseWorldPosition();
            HandleClick(worldPos);
        }

        if (currentMode == EditorMode.MoveVertex && selectedVertexIndex >= 0 && Input.GetMouseButton(0))
        {
            MoveSelectedVertex(GetMouseWorldPosition());
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (currentMode != EditorMode.MoveVertex)
                selectedVertexIndex = -1;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCurrentOperation();
        }
    }

    void OnDisable()
    {
        DestroySafe(mountainMat); DestroySafe(valleyMat); DestroySafe(boundaryMat); DestroySafe(flatMat);
    }

    void DestroySafe(Object o) { if (o != null) Destroy(o); }
    #endregion

    #region Mode & UI
    private void SetMode(EditorMode mode)
    {
        currentMode = mode;
        CancelCurrentOperation();
        if (creaseAttributePanel != null)
            creaseAttributePanel.Hide();
        Debug.Log($"[CreaseEditor] 切换模式: {mode}");
    }

    private void CancelCurrentOperation()
    {
        if (startVertexIndex >= 0 && startVertexIndex < vertexObjects.Count)
        {
            var r = vertexObjects[startVertexIndex]?.GetComponent<Renderer>();
            if (r != null && vertexMaterial != null) r.material = vertexMaterial;
        }
        startVertexIndex = -1;
        selectedVertexIndex = -1;
        selectedCreaseIndex = -1;
        if (creaseAttributePanel != null)
            creaseAttributePanel.Hide();
    }
    #endregion

    #region Click Handling
    private void HandleClick(Vector3 worldPos)
    {
        switch (currentMode)
        {
            case EditorMode.AddVertex:
                AddVertex(worldPos);
                break;
            case EditorMode.AddCrease:
                HandleCreaseCreation(worldPos);
                break;
            case EditorMode.MoveVertex:
                SelectVertex(worldPos);
                break;
            case EditorMode.DeleteVertex:
                TryDeleteVertex(worldPos);
                break;
            case EditorMode.DeleteCrease:
                TryDeleteCrease(worldPos);
                break;
            case EditorMode.EditCreaseAttributes:
                OpenCreaseAttributePanel(worldPos);
                break;
        }
    }
    #endregion

    #region Vertex Ops
    private void AddVertex(Vector3 position)
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        position.y = 0;
        int existing = FindNearestVertex(position);
        if (existing >= 0 && Vector3.Distance(position, GetVertexWorldPosition(existing)) < snapDistance)
        {
            Debug.Log("[CreaseEditor] 已存在相近顶点，跳过创建");
            return;
        }

        OrigamiVertex v = new OrigamiVertex
        {
            id = vertices.Count + 1,
            x = position.x,
            y = position.y,
            z = position.z
        };
        vertices.Add(v);

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = $"Vertex_{v.id}";
        go.transform.parent = transform;
        go.transform.position = position;
        go.transform.localScale = Vector3.one * vertexSize;
        if (vertexMaterial != null) go.GetComponent<Renderer>().material = vertexMaterial;
        vertexObjects.Add(go);

        Debug.Log($"[CreaseEditor] 添加顶点 #{v.id}");
    }

    private void SelectVertex(Vector3 position)
    {
        selectedVertexIndex = FindNearestVertex(position);
        if (selectedVertexIndex >= 0)
        {
            Debug.Log($"[CreaseEditor] 选中顶点 #{selectedVertexIndex + 1}");
            var r = vertexObjects[selectedVertexIndex].GetComponent<Renderer>();
            if (r != null) r.material.color = Color.yellow;
        }
    }

    private void MoveSelectedVertex(Vector3 position)
    {
        if (selectedVertexIndex < 0) return;
        position.y = 0;
        var v = vertices[selectedVertexIndex];
        v.x = position.x; v.y = position.y; v.z = position.z;
        vertexObjects[selectedVertexIndex].transform.position = new Vector3(v.x, v.y, v.z);
        UpdateConnectedCreases(selectedVertexIndex);
    }

    private void TryDeleteVertex(Vector3 position)
    {
        int idx = FindNearestVertex(position);
        if (idx < 0)
        {
            Debug.Log("[CreaseEditor] 未找到要删除的顶点");
            return;
        }

        // 删除所有包含该顶点的折痕
        for (int i = creases.Count - 1; i >= 0; i--)
        {
            if (creases[i].v1 - 1 == idx || creases[i].v2 - 1 == idx)
            {
                Destroy(creaseObjects[i]);
                creaseObjects.RemoveAt(i);
                creases.RemoveAt(i);
            }
        }

        Destroy(vertexObjects[idx]);
        vertexObjects.RemoveAt(idx);
        vertices.RemoveAt(idx);

        // FIX: 顶点删除后索引自动修正（关键修复！）
        ReindexAfterVertexRemoval(idx);
        Debug.Log($"[CreaseEditor] 删除顶点 #{idx + 1} 并移除关联折痕");
    }

    // FIX: 修复顶点删除后的索引问题（原代码用Clamp是错的！）
    private void ReindexAfterVertexRemoval(int deletedIndex)
    {
        // 1. 重置顶点ID
        for (int i = 0; i < vertices.Count; i++)
            vertices[i].id = i + 1;

        // 2. 修正折痕中的顶点索引（被删除的顶点 index = deletedIndex）
        for (int i = creases.Count - 1; i >= 0; i--)
        {
            // 如果折痕引用了已删除的顶点，则该折痕已被移除（TryDeleteVertex 已经删除相关折痕）
            // 对于仍然存在的折痕，若引用顶点索引大于 deletedIndex，则 -1
            if (creases[i].v1 - 1 > deletedIndex) creases[i].v1--;
            if (creases[i].v2 - 1 > deletedIndex) creases[i].v2--;
        }

        // 3. 重置折痕ID
        for (int i = 0; i < creases.Count; i++)
            creases[i].id = i + 1;

        // 4. 更新折痕对象名称（避免越界）
        for (int i = 0; i < creaseObjects.Count && i < creases.Count; i++)
            if (creaseObjects[i] != null)
                creaseObjects[i].name = $"Crease_{creases[i].id}";
    }


    private int FindNearestVertex(Vector3 position)
    {
        float minDistSq = snapDistance * snapDistance;
        int nearest = -1;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vPos = GetVertexWorldPosition(i);
            float d2 = (position.x - vPos.x) * (position.x - vPos.x) + (position.z - vPos.z) * (position.z - vPos.z);
            if (d2 < minDistSq)
            {
                minDistSq = d2;
                nearest = i;
            }
        }
        return nearest;
    }

    private Vector3 GetVertexWorldPosition(int index)
    {
        var v = vertices[index];
        return new Vector3(v.x, v.y, v.z);
    }
    #endregion

    #region Crease(Edge) Ops
    private void HandleCreaseCreation(Vector3 position)
    {
        int vIndex = FindNearestVertex(position);
        if (vIndex < 0)
        {
            Debug.Log("[CreaseEditor] 未找到顶点以创建折痕");
            return;
        }

        if (startVertexIndex < 0)
        {
            startVertexIndex = vIndex;
            var r = vertexObjects[startVertexIndex].GetComponent<Renderer>();
            if (r != null) r.material.color = Color.yellow;
            Debug.Log($"[CreaseEditor] 选择起点顶点 #{startVertexIndex + 1}");
            return;
        }

        if (startVertexIndex != vIndex)
        {
            CreateCrease(startVertexIndex, vIndex);
        }

        var rr = vertexObjects[startVertexIndex].GetComponent<Renderer>();
        if (rr != null && vertexMaterial != null) rr.material = vertexMaterial;
        startVertexIndex = -1;
    }

    private void CreateCrease(int v1Index, int v2Index)
    {
        if (creases.Any(c => (c.v1 - 1 == v1Index && c.v2 - 1 == v2Index) || (c.v1 - 1 == v2Index && c.v2 - 1 == v1Index)))
        {
            Debug.Log("[CreaseEditor] 折痕已存在，跳过创建");
            return;
        }

        // FIX: 用枚举设置类型（替代布尔字段）
        CreaseType type = CreaseType.Flat;
        if (mountainToggle != null && mountainToggle.isOn) type = CreaseType.Mountain;
        else if (valleyToggle != null && valleyToggle.isOn) type = CreaseType.Valley;
        else if (boundaryToggle != null && boundaryToggle.isOn) type = CreaseType.Boundary;

        OrigamiCrease crease = new OrigamiCrease
        {
            id = creases.Count + 1,
            v1 = v1Index + 1,
            v2 = v2Index + 1,
            creaseType = type,
            angle = 0f,
            width = creaseWidth
        };
        creases.Add(crease);

        GameObject go = new GameObject($"Crease_{crease.id}");
        go.transform.parent = transform;
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, GetVertexWorldPosition(v1Index));
        lr.SetPosition(1, GetVertexWorldPosition(v2Index));
        lr.startWidth = lr.endWidth = (type == CreaseType.Boundary ? creaseWidth * 1.5f : creaseWidth);
        lr.material = ChooseCreaseMaterial(crease);
        lr.useWorldSpace = true;
        creaseObjects.Add(go);

        Debug.Log($"[CreaseEditor] 创建折痕 #{crease.id} 类型: {type}");
    }

    private Material ChooseCreaseMaterial(OrigamiCrease c)
    {
        switch (c.creaseType)
        {
            case CreaseType.Mountain: return mountainMat;
            case CreaseType.Valley: return valleyMat;
            case CreaseType.Boundary: return boundaryMat;
            default: return flatMat;
        }
    }

    private void UpdateConnectedCreases(int vertexIndex)
    {
        for (int i = 0; i < creases.Count; i++)
        {
            var c = creases[i];
            int v1 = c.v1 - 1;
            int v2 = c.v2 - 1;
            if (v1 == vertexIndex || v2 == vertexIndex)
            {
                if (i < creaseObjects.Count && creaseObjects[i] != null)
                {
                    LineRenderer lr = creaseObjects[i].GetComponent<LineRenderer>();
                    lr.SetPosition(0, GetVertexWorldPosition(v1));
                    lr.SetPosition(1, GetVertexWorldPosition(v2));
                }
            }
        }
    }

    private void TryDeleteCrease(Vector3 position)
    {
        int cIdx = FindNearestCreaseIndex(position, Mathf.Max(creaseWidth * 2f, 0.12f));
        if (cIdx < 0)
        {
            Debug.Log("[CreaseEditor] 未找到附近折痕以删除");
            return;
        }

        Destroy(creaseObjects[cIdx]);
        creaseObjects.RemoveAt(cIdx);
        creases.RemoveAt(cIdx);

        // 重置折痕ID
        for (int i = 0; i < creases.Count; i++)
            creases[i].id = i + 1;

        // 更新UI
        for (int i = 0; i < creaseObjects.Count; i++)
            creaseObjects[i].name = $"Crease_{creases[i].id}";

        Debug.Log($"[CreaseEditor] 删除折痕 #{cIdx + 1}");
    }

    private int FindNearestCreaseIndex(Vector3 clickPos, float maxDistance)
    {
        float minDist = float.MaxValue;
        int best = -1;
        for (int i = 0; i < creases.Count; i++)
        {
            var c = creases[i];
            Vector3 a = GetVertexWorldPosition(c.v1 - 1);
            Vector3 b = GetVertexWorldPosition(c.v2 - 1);
            float d = DistancePointToSegmentXZ(clickPos, a, b);
            if (d < minDist) { minDist = d; best = i; }
        }
        return (minDist <= maxDistance) ? best : -1;
    }

    private float DistancePointToSegmentXZ(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector2 p2 = new Vector2(p.x, p.z);
        Vector2 a2 = new Vector2(a.x, a.z);
        Vector2 b2 = new Vector2(b.x, b.z);
        Vector2 ab = b2 - a2;
        float abLenSq = ab.sqrMagnitude;
        if (abLenSq < 1e-6f) return Vector2.Distance(p2, a2);
        float t = Vector2.Dot(p2 - a2, ab) / abLenSq;
        t = Mathf.Clamp01(t);
        Vector2 closest = a2 + t * ab;
        return Vector2.Distance(p2, closest);
    }
    #endregion

    #region Crease Attribute Editing (FIXED!)
    private void OpenCreaseAttributePanel(Vector3 clickPos)
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        int cIdx = FindNearestCreaseIndex(clickPos, Mathf.Max(creaseWidth * 2f, 0.12f));
        if (cIdx < 0)
        {
            Debug.Log("[CreaseEditor] 未找到附近折痕以编辑");
            return;
        }

        selectedCreaseIndex = cIdx;
        var c = creases[cIdx];

        if (creaseAttributePanel != null)
        {
            creaseAttributePanel.Show(c.id, c, (newType, newMinAngle, newMaxAngle) =>
            {
                Debug.Log($"[CreaseEditor] 更新折痕 #{c.id} 类型: {newType}, Min角度: {newMinAngle}, Max角度: {newMaxAngle}");

                // 调用支持 min/max 的更新方法
                UpdateCreaseAttributes(c.id, newType, minAngle: newMinAngle, maxAngle: newMaxAngle);
            });
        }

        else
        {
            // 旧版：直接用Toggle设置类型
            CreaseType type = CreaseType.Flat;
            if (mountainToggle != null && mountainToggle.isOn) type = CreaseType.Mountain;
            else if (valleyToggle != null && valleyToggle.isOn) type = CreaseType.Valley;
            else if (boundaryToggle != null && boundaryToggle.isOn) type = CreaseType.Boundary;

            c.creaseType = type;
            if (cIdx < creaseObjects.Count)
                creaseObjects[cIdx].GetComponent<LineRenderer>().material = ChooseCreaseMaterial(c);
        }
    }

    // 新增：支持 minAngle / maxAngle 的重载
    private void UpdateCreaseAttributes(
        int creaseId,
        CreaseType newType,
        float minAngle,
        float maxAngle)
    {
        for (int i = 0; i < creases.Count; i++)
        {
            if (creases[i].id == creaseId)
            {
                var c = creases[i];
                c.creaseType = newType;
                c.minAngle = minAngle;
                c.maxAngle = maxAngle;

                // 可选：如果需要同步 angle 字段（例如取中间值）
                // c.angle = Mathf.Clamp(c.angle, minAngle, maxAngle);

                UpdateCreaseVisual(i); // 更新颜色等视觉效果
                return;
            }
        }
    }

    // FIX: 新增此方法（补全缺失逻辑）
    private void UpdateCreaseVisual(int creaseIndex)
    {
        if (creaseIndex >= 0 && creaseIndex < creaseObjects.Count)
        {
            var lr = creaseObjects[creaseIndex].GetComponent<LineRenderer>();
            lr.material = ChooseCreaseMaterial(creases[creaseIndex]);
        }
    }
    #endregion

    #region Save / Load
    private void SaveToJson()
    {
        if (vertices.Count < 1)
        {
            Debug.LogWarning("[CreaseEditor] 需要至少1个顶点才能保存");
            return;
        }

        // ---------------------------
        // 1. 转换为 OrigamiData.OrigamiVertex 列表
        // ---------------------------
        List<global::OrigamiVertex> outVertices = new List<global::OrigamiVertex>();
        foreach (var v in vertices)
        {
            outVertices.Add(new global::OrigamiVertex
            {
                x = v.x,
                y = v.y,
                z = v.z
            });
        }

        // ---------------------------
        // 2. 转换为 OrigamiData.OrigamiCrease 列表
        // ---------------------------
        List<global::OrigamiCrease> outCreases = new List<global::OrigamiCrease>();
        foreach (var c in creases)
        {
            // 将内部 creaseType 映射到 OrigamiData 的 Type
            global::OrigamiCrease.Type mappedType = global::OrigamiCrease.Type.Valley;
            switch (c.creaseType)
            {
                case CreaseType.Mountain:
                    mappedType = global::OrigamiCrease.Type.Mountain;
                    break;
                case CreaseType.Valley:
                    mappedType = global::OrigamiCrease.Type.Valley;
                    break;
                case CreaseType.Boundary:
                    mappedType = global::OrigamiCrease.Type.Boundary;
                    break;
                case CreaseType.Flat:
                    mappedType = global::OrigamiCrease.Type.Valley; // Flat 作为普通折痕显示
                    break;
            }

            // 采用 angle 作为 restAngle，提供宽泛的限位与默认刚度
            float rest = c.angle;
            float min = rest - 180f;
            float max = rest + 180f;
            float stiff = 1.0f;

            var outC = new global::OrigamiCrease
            {
                id = c.id,
                v1 = c.v1,
                v2 = c.v2,
                type = mappedType,
                restAngle = rest,
                minAngle = min,
                maxAngle = max,
                stiffness = stiff,
                width = c.width
            };
            outCreases.Add(outC);
        }

        // ---------------------------
        // 3. faces: CreaseEditor 不管理面数据 → 空列表
        // ---------------------------
        List<global::OrigamiFace> outFaces = OrigamiFaceGenerator.GenerateFaces(
            outVertices, // 使用转换后的 vertices
            outCreases   // 使用转换后的 creases
        );        // 如果未来你加了面，这里填写生成规则

        // ---------------------------
        // 4. connections：CreaseEditor 不管理 → 空列表
        // ---------------------------
        List<global::OrigamiConnection> outConnections = new List<global::OrigamiConnection>();

        // ---------------------------
        // 5. 构建 OrigamiData.OrigamiModel
        // ---------------------------
        global::OrigamiModel model = new global::OrigamiModel
        {
            name = string.IsNullOrEmpty(fileNameInput?.text) ? "CustomPattern" : fileNameInput.text,
            description = "Point-edge based crease pattern",

            vertices = outVertices,
            faces = outFaces,
            creases = outCreases,
            connections = outConnections,

            material = new global::OrigamiMaterial
            {
                name = "default",
                metallic = 0f,
                smoothness = 0.5f,
                doubleSided = true
            },

            defaultCreaseWidth = creaseWidth,

            // 提供默认折痕颜色
            mountainCreaseColor = "#FF0000",
            valleyCreaseColor = "#0000FF",
            boundaryCreaseColor = "#000000"
        };

        // ---------------------------
        // 6. 保存为 JSON
        // ---------------------------
        try
        {
            string modelsDir = Path.Combine(Application.dataPath, "Models");
            if (!Directory.Exists(modelsDir))
                Directory.CreateDirectory(modelsDir);

            string path = Path.Combine(modelsDir, $"{model.name}.json");
            string json = JsonUtility.ToJson(model, true);

            File.WriteAllText(path, json);

            Debug.Log($"[CreaseEditor] 保存成功: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CreaseEditor] 保存失败: {e.Message}");
        }
    }


    private void LoadFromJson()
    {
        string fileName = loadFileInput != null ? loadFileInput.text.Trim() : "";

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning("[CreaseEditor] 请输入 JSON 文件名，例如 mypattern.json");
            return;
        }

        ClearAll();

        string jsonFullPath;
        if (Path.IsPathRooted(fileName))
        {
            jsonFullPath = fileName;
        }
        else
        {
            jsonFullPath = Path.Combine(Application.dataPath, "Models", fileName);
        }

        if (!File.Exists(jsonFullPath))
        {
            Debug.LogError($"❌ JSON 文件不存在: {jsonFullPath}");
            return;
        }

        string json = File.ReadAllText(jsonFullPath);
        global::OrigamiModel gModel = JsonUtility.FromJson<global::OrigamiModel>(json);

        if (gModel == null)
        {
            Debug.LogError("❌ JSON 解析失败");
            return;
        }

        if (gModel.vertices != null)
        {
            for (int i = 0; i < gModel.vertices.Count; i++)
            {
                var gv = gModel.vertices[i];
                var v = new OrigamiVertex
                {
                    id = vertices.Count + 1,
                    x = gv.x,
                    y = gv.y,
                    z = gv.z
                };
                vertices.Add(v);

                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = $"Vertex_{v.id}";
                go.transform.parent = transform;
                go.transform.position = new Vector3(v.x, v.y, v.z);
                go.transform.localScale = Vector3.one * vertexSize;
                if (vertexMaterial != null) go.GetComponent<Renderer>().material = vertexMaterial;
                vertexObjects.Add(go);
            }
        }

        if (gModel.creases != null)
        {
            for (int i = 0; i < gModel.creases.Count; i++)
            {
                var gc = gModel.creases[i];
                CreaseType type = CreaseType.Valley;
                switch (gc.type)
                {
                    case global::OrigamiCrease.Type.Mountain: type = CreaseType.Mountain; break;
                    case global::OrigamiCrease.Type.Valley: type = CreaseType.Valley; break;
                    case global::OrigamiCrease.Type.Boundary: type = CreaseType.Boundary; break;
                }

                var c = new OrigamiCrease
                {
                    id = creases.Count + 1,
                    v1 = gc.v1,
                    v2 = gc.v2,
                    creaseType = type,
                    angle = gc.restAngle,
                    width = gc.width,
                    minAngle = gc.minAngle,
                    maxAngle = gc.maxAngle
                };
                creases.Add(c);

                GameObject go = new GameObject($"Crease_{c.id}");
                go.transform.parent = transform;
                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPosition(0, GetVertexWorldPosition(c.v1 - 1));
                lr.SetPosition(1, GetVertexWorldPosition(c.v2 - 1));
                lr.startWidth = lr.endWidth = (type == CreaseType.Boundary ? creaseWidth * 1.5f : (c.width > 0 ? c.width : creaseWidth));
                lr.material = ChooseCreaseMaterial(c);
                lr.useWorldSpace = true;
                creaseObjects.Add(go);
            }
        }

        currentVertices = vertices.Select(v => new Vector3(v.x, v.y, v.z)).ToList();
        currentCreases = new List<OrigamiCrease>(creases);

        Debug.Log($"✅ 数据加载完成: 顶点数={vertices.Count}, 折痕数={creases.Count}");
    }



    private void ClearAll()
    {
        vertexObjects.ForEach(o => { if (o != null) Destroy(o); });
        creaseObjects.ForEach(o => { if (o != null) Destroy(o); });
        vertexObjects.Clear();
        creaseObjects.Clear();
        vertices.Clear();
        creases.Clear();
        startVertexIndex = -1;
        selectedVertexIndex = -1;
        selectedCreaseIndex = -1;
    }
    #endregion

    #region Helpers
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);
        if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            worldPos.y = 0f;
            if (gridSize > 0)
            {
                worldPos.x = Mathf.Round(worldPos.x / gridSize) * gridSize;
                worldPos.z = Mathf.Round(worldPos.z / gridSize) * gridSize;
            }
            return worldPos;
        }
        return Vector3.zero;
    }
    #endregion
}