using System.IO;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class OrigamiLoader : MonoBehaviour
{
    public enum ElasticFlattenPlane
    {
        XY,
        XZ,
        YZ
    }

    [Header("模型文件")]
    public string jsonPath = "D:\\0work\\Develop\\unity\\Origami_Simulator\\Assets\\scripts\\CustomPattern_minAngle0.json";    //所需文件路径和文件名
    
    public Material defaultMaterial;

    [Header("可视化设置")]
    public bool showCreases = true;
    public bool useCustomColors = true;

    [Header("折痕材质")]
    public Material creaseMaterial; // ✅ 新增字段（在 Inspector 中设置）

    [Header("Physics Colliders")]
    [Min(0.001f)]
    public float colliderThickness = 0.01f;

    [Tooltip("Ignore collisions between runtime-generated origami faces. Keep this on for ideal zero-thickness folding, such as a Kresling model folding flat.")]
    public bool ignoreInternalFaceCollisions = false;

    [Header("Wedge Panel Angle")]
    [Tooltip("Override the JSON half-angle for wedge panels. Reload the model after changing this value.")]
    public bool overridePanelHalfAngle = false;

    [Range(0.01f, 89f)]
    public float panelHalfAngleOverrideDegrees = 5f;

    [Header("Elastic Flatten (soft-paper visualization)")]
    [Tooltip("For a closed paper model that can flatten only by bending/stretching. This is a kinematic visual mode, not a rigid-origami physics simulation.")]
    public bool enableElasticFlatten = false;

    [Tooltip("Plane used by the elastic flatten target. 927 uses XY because its compression axis is Z.")]
    public ElasticFlattenPlane elasticFlattenPlane = ElasticFlattenPlane.XY;

    [Header("Physics Stability")]
    public float driverSpring = 60f;
    public float driverDamper = 10f;
    public float followerSpring = 3f;
    public float followerDamper = 16f;
    public int solverIterations = 12;
    public int solverVelocityIterations = 8;
    public bool disableJointPreprocessing = true;
    public bool pinOneRootFace = true;

    [Header("UI 控制")]
    public Text modelNameText;
    public Text modelDescriptionText;

    [Header("空间 DXF 滑条兼容")]
    [Tooltip("旧版 *_spatial.json 没有驱动元数据时，使用现有滑条直接驱动其 M/V 折痕。")]
    public bool driveLegacySpatialDxfWithExistingSlider = true;

    [Range(0f, 180f)]
    [Tooltip("旧版空间 DXF 的滑条 0→180 对应折痕目标角度。")]
    public float legacySpatialDxfSliderTravelAngle = 180f;

    private OrigamiModel model;
    private List<Vector3> vertices = new();
    private Dictionary<int, GameObject> faceObjects = new();
    private List<HingeJoint> hinges = new();
    private GameObject creaseParent;
    private List<GameObject> faceListObjects = new();
    private readonly List<Vector3> elasticFlattenSourceVertices = new();
    private readonly List<Vector3> elasticFlattenTargetVertices = new();
    [SerializeField, Range(0f, 1f)] private float elasticFlattenProgress;
    private bool elasticFlattenActive;

    public bool IsElasticFlattenActive => elasticFlattenActive;
    public float ElasticFlattenProgress => elasticFlattenProgress;

    // ✅ 三种预生成材质（减少内存消耗）
    private Material mountainMat;
    private Material valleyMat;
    private Material boundaryMat;
    private List<GameObject> creaseLineObjects = new();


    void Start()
    {
        LoadModel();
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

        elasticFlattenActive = ShouldUseElasticFlatten();

        if (!elasticFlattenActive && PrepareSpatialDxfSliderDrive(jsonFullPath))
            ResetFoldStateAndSlider();

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
        {
            CreateFaces();
            if (!elasticFlattenActive && ignoreInternalFaceCollisions)
                IgnoreInternalFaceCollisions();
        }

        if (showCreases && model.creases != null && model.creases.Count > 0)
            DrawCreases();


        if (elasticFlattenActive)
        {
            InitializeElasticFlatten();
            ResetFoldStateAndSlider();
        }
        else
        {
            CreateCreasesAndConnections();
        }
    }

    private bool ShouldUseElasticFlatten()
    {
        if (!enableElasticFlatten || model == null || model.faces == null || model.faces.Count == 0)
            return false;

        return model.elasticFlattenEnabled;
    }

    // SimulationLoader uses this to give legacy *_spatial.json files the same
    // safe default as a newly imported spatial DXF. New files carry their own
    // metadata and are not overridden by this fallback.
    public void ConfigureLegacySpatialDxfSliderDrive(bool enabled, float travelAngle)
    {
        driveLegacySpatialDxfWithExistingSlider = enabled;
        legacySpatialDxfSliderTravelAngle = Mathf.Clamp(travelAngle, 0f, 180f);
    }

    private bool PrepareSpatialDxfSliderDrive(string jsonFullPath)
    {
        if (!IsSpatialDxfModel(jsonFullPath) || model.creases == null)
            return false;

        bool hasImportMetadata = model.spatialDxfImportVersion > 0;
        bool enableSliderDrive = hasImportMetadata
            ? model.spatialDxfSliderDriveEnabled
            : driveLegacySpatialDxfWithExistingSlider;
        if (!enableSliderDrive)
            return false;

        float travelAngle = hasImportMetadata
            ? model.spatialDxfSliderTravelAngle
            : legacySpatialDxfSliderTravelAngle;
        travelAngle = Mathf.Clamp(travelAngle, 0f, 180f);

        int drivenCreaseCount = 0;
        for (int i = 0; i < model.creases.Count; i++)
        {
            OrigamiCrease crease = model.creases[i];
            if (crease == null || crease.type == OrigamiCrease.Type.Boundary)
                continue;

            // This is deliberately the existing generic fold-drive path, not
            // a Kresling-specific controller. The M/V hinge-owner rule below
            // still determines the physical folding direction.
            crease.driveMode = OrigamiCrease.DriveMode.Auto;
            crease.restAngle = 0f;
            crease.minAngle = 0f;
            crease.maxAngle = travelAngle;
            drivenCreaseCount++;
        }

        Debug.Log($"[OrigamiLoader] Spatial slider drive enabled for {drivenCreaseCount} M/V creases (0° → {travelAngle:F1}°).");
        return drivenCreaseCount > 0;
    }

    private bool IsSpatialDxfModel(string jsonFullPath)
    {
        if (model.spatialDxfImportVersion > 0)
            return true;

        if (!string.IsNullOrEmpty(model.description)
            && model.description.StartsWith("Spatial M/V/B curve import", StringComparison.OrdinalIgnoreCase))
            return true;

        string fileName = Path.GetFileNameWithoutExtension(jsonFullPath);
        return !string.IsNullOrEmpty(fileName)
            && fileName.EndsWith("_spatial", StringComparison.OrdinalIgnoreCase);
    }

    private void ResetFoldStateAndSlider()
    {
        OrigamiController controller = GetComponent<OrigamiController>();
        if (controller == null)
            return;

        // A sequence from a previously loaded 920 model must release the
        // shared controller before the manual spatial-slider path takes over.
        FourServoSequenceController servoSequence = FindObjectOfType<FourServoSequenceController>();
        if (servoSequence != null && servoSequence.IsRunning)
            servoSequence.StopServoSequence();

        controller.ResetFoldState(0f);

        SliderController sliderController = FindObjectOfType<SliderController>();
        if (sliderController != null && sliderController.origami == controller)
            sliderController.UpdateSliderValue();
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

        elasticFlattenActive = false;
        elasticFlattenProgress = 0f;
        elasticFlattenSourceVertices.Clear();
        elasticFlattenTargetVertices.Clear();

        var controller = GetComponent<OrigamiController>();
        if (controller != null)
            controller.ClearAllHinges();

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
        bool useWedgePanels = model.panelHalfAngleDegrees > 0f;
        float halfAngleDegrees = overridePanelHalfAngle ? panelHalfAngleOverrideDegrees : model.panelHalfAngleDegrees;
        foreach (var face in model.faces)
        {
            if (face.vertices == null || face.vertices.Count < 3)
            {
                Debug.LogWarning($"[OrigamiLoader] Skip Face_{face.id}: fewer than 3 vertices");
                continue;
            }

            if (useWedgePanels && face.vertices.Count != 3)
            {
                Debug.LogError($"[OrigamiLoader] Skip Face_{face.id}: wedge panels require exactly 3 vertices");
                continue;
            }

            GameObject obj = new GameObject($"Face_{face.id}");
            obj.transform.parent = transform;

            Mesh mesh = new Mesh();
            Vector3[] fVerts = new Vector3[face.vertices.Count];

            for (int i = 0; i < face.vertices.Count; i++)
                fVerts[i] = vertices[face.vertices[i] - 1];

            if (useWedgePanels)
            {
                if (!TryBuildWedgePanelMesh(mesh, fVerts[0], fVerts[1], fVerts[2], halfAngleDegrees))
                {
                    Debug.LogError($"[OrigamiLoader] Skip Face_{face.id}: invalid wedge angle or degenerate triangle");
                    Destroy(mesh);
                    Destroy(obj);
                    continue;
                }
            }
            else
            {
                int[] tris = BuildFaceTriangles(fVerts);
                if (tris.Length < 3)
                {
                    Debug.LogWarning($"[OrigamiLoader] Skip Face_{face.id}: triangulation failed");
                    Destroy(mesh);
                    Destroy(obj);
                    continue;
                }

                mesh.vertices = fVerts;
                mesh.triangles = tris;
            }

            mesh.name = $"Face_{face.id}_Mesh";
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

            // At the planar endpoint some faces reverse their winding. Use
            // the material's cull setting when available so the soft-paper
            // visualization remains visible from either side.
            if (elasticFlattenActive && faceMat.HasProperty("_Cull"))
                faceMat.SetInt("_Cull", 0);

            renderer.material = faceMat;  // 直接使用材质

            // Elastic flatten is a soft-paper visualization. Do not create
            // colliders or rigidbodies that would fight the scripted mesh
            // deformation.
            if (!elasticFlattenActive)
            {
                if (useWedgePanels)
                {
                    MeshCollider collider = obj.AddComponent<MeshCollider>();
                    collider.sharedMesh = mesh;
                    collider.convex = true;
                }
                else
                {
                    CreateFaceColliderChildren(obj, mesh, face.id);
                }

                Rigidbody rb = obj.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.mass = face.rigid ? 0.2f : 0.1f;
                rb.drag = face.rigid ? 0.8f : 0.5f;
                rb.angularDrag = face.rigid ? 0.8f : 0.5f;
                rb.solverIterations = Mathf.Max(1, solverIterations);
                rb.solverVelocityIterations = Mathf.Max(1, solverVelocityIterations);
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }

            faceObjects[face.id] = obj;
            faceListObjects.Add(obj);
        }
    }

    private static bool TryBuildWedgePanelMesh(Mesh mesh, Vector3 first, Vector3 second, Vector3 third, float halfAngleDegrees)
    {
        if (halfAngleDegrees <= 0f || halfAngleDegrees >= 90f)
            return false;

        float oppositeFirst = Vector3.Distance(second, third);
        float oppositeSecond = Vector3.Distance(third, first);
        float oppositeThird = Vector3.Distance(first, second);
        float perimeter = oppositeFirst + oppositeSecond + oppositeThird;
        Vector3 cross = Vector3.Cross(second - first, third - first);
        float doubleArea = cross.magnitude;
        if (perimeter < 0.000001f || doubleArea < 0.000001f)
            return false;

        Vector3 incenter = (first * oppositeFirst + second * oppositeSecond + third * oppositeThird) / perimeter;
        float halfHeight = doubleArea / perimeter * Mathf.Tan(halfAngleDegrees * Mathf.Deg2Rad);
        Vector3 offset = cross / doubleArea * halfHeight;
        Vector3 top = incenter + offset;
        Vector3 bottom = incenter - offset;

        mesh.vertices = new[]
        {
            first, second, top,
            second, third, top,
            third, first, top,
            second, first, bottom,
            third, second, bottom,
            first, third, bottom
        };
        mesh.triangles = new[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8,
            9, 10, 11, 12, 13, 14, 15, 16, 17
        };
        return true;
    }

    private void InitializeElasticFlatten()
    {
        elasticFlattenSourceVertices.Clear();
        elasticFlattenTargetVertices.Clear();
        elasticFlattenSourceVertices.AddRange(vertices);

        float planeCoordinate = GetElasticFlattenPlaneCoordinate(vertices);
        for (int i = 0; i < elasticFlattenSourceVertices.Count; i++)
        {
            Vector3 target = elasticFlattenSourceVertices[i];
            switch (elasticFlattenPlane)
            {
                case ElasticFlattenPlane.XY:
                    target.z = planeCoordinate;
                    break;
                case ElasticFlattenPlane.XZ:
                    target.y = planeCoordinate;
                    break;
                case ElasticFlattenPlane.YZ:
                    target.x = planeCoordinate;
                    break;
            }
            elasticFlattenTargetVertices.Add(target);
        }

        elasticFlattenProgress = 0f;
        SetElasticFlattenProgress(0f);
        Debug.Log($"[OrigamiLoader] Elastic Flatten enabled for {model.name}. This is a soft-paper visual deformation, not rigid-fold physics.");
    }

    private float GetElasticFlattenPlaneCoordinate(List<Vector3> sourceVertices)
    {
        if (sourceVertices == null || sourceVertices.Count == 0)
            return 0f;

        float total = 0f;
        for (int i = 0; i < sourceVertices.Count; i++)
        {
            switch (elasticFlattenPlane)
            {
                case ElasticFlattenPlane.XY:
                    total += sourceVertices[i].z;
                    break;
                case ElasticFlattenPlane.XZ:
                    total += sourceVertices[i].y;
                    break;
                case ElasticFlattenPlane.YZ:
                    total += sourceVertices[i].x;
                    break;
            }
        }
        return total / sourceVertices.Count;
    }

    public void SetElasticFlattenProgress(float progress)
    {
        if (!elasticFlattenActive || model == null
            || elasticFlattenSourceVertices.Count != vertices.Count
            || elasticFlattenTargetVertices.Count != vertices.Count)
            return;

        elasticFlattenProgress = Mathf.Clamp01(progress);
        for (int faceIndex = 0; faceIndex < model.faces.Count; faceIndex++)
        {
            OrigamiFace face = model.faces[faceIndex];
            if (face == null || face.vertices == null
                || !faceObjects.TryGetValue(face.id, out GameObject faceObject)
                || faceObject == null)
                continue;

            MeshFilter meshFilter = faceObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.mesh == null)
                continue;

            Mesh mesh = meshFilter.mesh;
            Vector3[] meshVertices = mesh.vertices;
            if (meshVertices.Length != face.vertices.Count)
                continue;

            for (int vertexIndex = 0; vertexIndex < face.vertices.Count; vertexIndex++)
            {
                int modelVertexIndex = face.vertices[vertexIndex] - 1;
                if (modelVertexIndex < 0 || modelVertexIndex >= elasticFlattenSourceVertices.Count)
                    continue;

                meshVertices[vertexIndex] = Vector3.Lerp(
                    elasticFlattenSourceVertices[modelVertexIndex],
                    elasticFlattenTargetVertices[modelVertexIndex],
                    elasticFlattenProgress);
            }

            mesh.vertices = meshVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        UpdateElasticCreaseLines();
    }

    private Vector3 GetDisplayedVertex(int vertexIndex)
    {
        if (vertexIndex < 0 || vertexIndex >= vertices.Count)
            return Vector3.zero;

        if (!elasticFlattenActive
            || vertexIndex >= elasticFlattenSourceVertices.Count
            || vertexIndex >= elasticFlattenTargetVertices.Count)
            return vertices[vertexIndex];

        return Vector3.Lerp(
            elasticFlattenSourceVertices[vertexIndex],
            elasticFlattenTargetVertices[vertexIndex],
            elasticFlattenProgress);
    }

    private void UpdateElasticCreaseLines()
    {
        if (!elasticFlattenActive || model == null || model.creases == null)
            return;

        int count = Mathf.Min(model.creases.Count, creaseLineObjects.Count);
        for (int creaseIndex = 0; creaseIndex < count; creaseIndex++)
        {
            GameObject creaseLineObject = creaseLineObjects[creaseIndex];
            if (creaseLineObject == null)
                continue;

            LineRenderer lineRenderer = creaseLineObject.GetComponent<LineRenderer>();
            OrigamiCrease crease = model.creases[creaseIndex];
            if (lineRenderer == null || crease == null)
                continue;

            Vector3 firstPoint = GetDisplayedVertex(crease.v1 - 1);
            Vector3 secondPoint = GetDisplayedVertex(crease.v2 - 1);
            lineRenderer.useWorldSpace = true;
            lineRenderer.SetPosition(0, transform.TransformPoint(firstPoint));
            lineRenderer.SetPosition(1, transform.TransformPoint(secondPoint));
        }
    }

    // HingeJoint.enableCollision only suppresses collision for the two faces
    // joined by that hinge. A flat-folding origami model also needs distant
    // faces to pass through / stack on each other, so disable contacts between
    // every pair of generated face objects when the optional ideal-sheet mode
    // is enabled.
    private void IgnoreInternalFaceCollisions()
    {
        int ignoredPairCount = 0;
        for (int firstFaceIndex = 0; firstFaceIndex < faceListObjects.Count; firstFaceIndex++)
        {
            GameObject firstFace = faceListObjects[firstFaceIndex];
            if (firstFace == null)
                continue;

            Collider[] firstColliders = firstFace.GetComponentsInChildren<Collider>(true);
            if (firstColliders == null || firstColliders.Length == 0)
                continue;

            for (int secondFaceIndex = firstFaceIndex + 1; secondFaceIndex < faceListObjects.Count; secondFaceIndex++)
            {
                GameObject secondFace = faceListObjects[secondFaceIndex];
                if (secondFace == null)
                    continue;

                Collider[] secondColliders = secondFace.GetComponentsInChildren<Collider>(true);
                if (secondColliders == null || secondColliders.Length == 0)
                    continue;

                for (int firstColliderIndex = 0; firstColliderIndex < firstColliders.Length; firstColliderIndex++)
                {
                    Collider firstCollider = firstColliders[firstColliderIndex];
                    if (firstCollider == null)
                        continue;

                    for (int secondColliderIndex = 0; secondColliderIndex < secondColliders.Length; secondColliderIndex++)
                    {
                        Collider secondCollider = secondColliders[secondColliderIndex];
                        if (secondCollider == null)
                            continue;

                        Physics.IgnoreCollision(firstCollider, secondCollider, true);
                        ignoredPairCount++;
                    }
                }
            }
        }

        Debug.Log($"[OrigamiLoader] Ignored {ignoredPairCount} internal face-collider pair(s).");
    }

    private int[] BuildFaceTriangles(Vector3[] faceVertices)
    {
        if (faceVertices.Length == 3)
            return new[] { 0, 1, 2 };

        int[] triangulated = new Triangulator(faceVertices).Triangulate();
        if (triangulated != null && triangulated.Length >= 3)
            return OrientTrianglesToFaceNormal(faceVertices, triangulated);

        int[] fanTriangles = new int[(faceVertices.Length - 2) * 3];
        for (int i = 0; i < faceVertices.Length - 2; i++)
        {
            fanTriangles[i * 3] = 0;
            fanTriangles[i * 3 + 1] = i + 1;
            fanTriangles[i * 3 + 2] = i + 2;
        }

        return OrientTrianglesToFaceNormal(faceVertices, fanTriangles);
    }

    private int[] OrientTrianglesToFaceNormal(Vector3[] faceVertices, int[] triangles)
    {
        Vector3 faceNormal = CalculatePolygonNormal(faceVertices);
        if (faceNormal.sqrMagnitude < 0.000001f)
            return triangles;

        int[] oriented = (int[])triangles.Clone();
        for (int i = 0; i < oriented.Length; i += 3)
        {
            Vector3 a = faceVertices[oriented[i]];
            Vector3 b = faceVertices[oriented[i + 1]];
            Vector3 c = faceVertices[oriented[i + 2]];
            Vector3 triangleNormal = Vector3.Cross(b - a, c - a);

            if (Vector3.Dot(triangleNormal, faceNormal) < 0f)
            {
                int temp = oriented[i + 1];
                oriented[i + 1] = oriented[i + 2];
                oriented[i + 2] = temp;
            }
        }

        return oriented;
    }

    private Vector3 CalculatePolygonNormal(Vector3[] faceVertices)
    {
        Vector3 normal = Vector3.zero;
        for (int i = 0; i < faceVertices.Length; i++)
        {
            Vector3 current = faceVertices[i];
            Vector3 next = faceVertices[(i + 1) % faceVertices.Length];

            normal.x += (current.y - next.y) * (current.z + next.z);
            normal.y += (current.z - next.z) * (current.x + next.x);
            normal.z += (current.x - next.x) * (current.y + next.y);
        }

        return normal.normalized;
    }

    private void CreateFaceColliderChildren(GameObject faceObject, Mesh faceMesh, int faceId)
    {
        Vector3[] meshVertices = faceMesh.vertices;
        int[] meshTriangles = faceMesh.triangles;

        for (int i = 0; i < meshTriangles.Length; i += 3)
        {
            Vector3 a = meshVertices[meshTriangles[i]];
            Vector3 b = meshVertices[meshTriangles[i + 1]];
            Vector3 c = meshVertices[meshTriangles[i + 2]];

            GameObject colliderObject = new GameObject($"Face_{faceId}_Collider_{i / 3}");
            colliderObject.layer = faceObject.layer;
            colliderObject.transform.SetParent(faceObject.transform, false);
            colliderObject.transform.localPosition = Vector3.zero;
            colliderObject.transform.localRotation = Quaternion.identity;
            colliderObject.transform.localScale = Vector3.one;

            MeshCollider collider = colliderObject.AddComponent<MeshCollider>();
            collider.sharedMesh = CreateTrianglePrismMesh(a, b, c, colliderThickness, faceId, i / 3);
            collider.convex = true;
            collider.isTrigger = false;
        }
    }

    private Mesh CreateTrianglePrismMesh(Vector3 a, Vector3 b, Vector3 c, float thickness, int faceId, int triangleIndex)
    {
        Vector3 normal = Vector3.Cross(b - a, c - a);
        if (normal.sqrMagnitude < 0.000001f)
            normal = Vector3.up;
        else
            normal.Normalize();

        float halfThickness = Mathf.Max(0.001f, thickness) * 0.5f;
        Vector3 offset = normal * halfThickness;

        Mesh prismMesh = new Mesh();
        prismMesh.name = $"Face_{faceId}_Collider_{triangleIndex}_Mesh";
        prismMesh.vertices = new[]
        {
            a - offset,
            b - offset,
            c - offset,
            a + offset,
            b + offset,
            c + offset
        };
        prismMesh.triangles = new[]
        {
            0, 2, 1,
            3, 4, 5,
            0, 1, 4,
            0, 4, 3,
            1, 2, 5,
            1, 5, 4,
            2, 0, 3,
            2, 3, 5
        };
        prismMesh.RecalculateNormals();
        prismMesh.RecalculateBounds();

        return prismMesh;
    }

    private void CreateCreasesAndConnections()
    {
        Debug.Log($"[OrigamiLoader] Creating hinges for {(model.creases != null ? model.creases.Count : 0)} creases.");
        if (model.creases == null || model.creases.Count == 0 || model.faces == null || model.faces.Count == 0)
            return;

        var faceParents = new Dictionary<int, int>();
        var connectedFaceIds = new HashSet<int>();
        foreach (var face in model.faces)
        {
            if (faceObjects.ContainsKey(face.id))
                faceParents[face.id] = face.id;
        }

        int driverCount = 0;
        int followerCount = 0;
        OrigamiController controller = GetComponent<OrigamiController>();

        for (int i = 0; i < model.creases.Count; i++)
        {
            OrigamiCrease crease = model.creases[i];
            if (crease.type == OrigamiCrease.Type.Boundary)
                continue;

            List<int> touchingFaces = new List<int>();
            for (int fIdx = 0; fIdx < model.faces.Count; fIdx++)
            {
                OrigamiFace face = model.faces[fIdx];
                if (face.vertices != null && face.vertices.Contains(crease.v1) && face.vertices.Contains(crease.v2))
                    touchingFaces.Add(fIdx);
            }

            if (touchingFaces.Count < 2)
            {
                Debug.LogWarning($"[OrigamiLoader] Crease {crease.id} touches {touchingFaces.Count} faces; hinge skipped.");
                continue;
            }
            if (touchingFaces.Count > 2)
                Debug.LogWarning($"[OrigamiLoader] Crease {crease.id} is non-manifold; only the first two faces are used.");

            OrigamiFace faceAData = model.faces[touchingFaces[0]];
            OrigamiFace faceBData = model.faces[touchingFaces[1]];
            if (!faceObjects.TryGetValue(faceAData.id, out GameObject faceA)
                || !faceObjects.TryGetValue(faceBData.id, out GameObject faceB))
            {
                Debug.LogWarning($"[OrigamiLoader] Crease {crease.id} references a face that was not generated.");
                continue;
            }

            Rigidbody rbA = faceA.GetComponent<Rigidbody>();
            Rigidbody rbB = faceB.GetComponent<Rigidbody>();
            if (rbA == null || rbB == null)
                continue;

            Vector3 localV1 = vertices[crease.v1 - 1];
            Vector3 localV2 = vertices[crease.v2 - 1];
            Vector3 worldHingePoint = transform.TransformPoint((localV1 + localV2) * 0.5f);
            Vector3 worldCreaseDirection = transform.TransformDirection(localV2 - localV1).normalized;
            if (worldCreaseDirection.sqrMagnitude < 0.0001f)
            {
                Debug.LogWarning($"[OrigamiLoader] Crease {crease.id} has zero length; hinge skipped.");
                continue;
            }

            bool isFaceAOnLeft = IsFaceOnLeft_ByVertexOrder(faceAData, crease.v1, crease.v2);
            GameObject hingeOwner;
            if (crease.type == OrigamiCrease.Type.Mountain)
                hingeOwner = isFaceAOnLeft ? faceA : faceB;
            else
                hingeOwner = isFaceAOnLeft ? faceB : faceA;

            Rigidbody connectedRb = hingeOwner == faceA ? rbB : rbA;
            HingeJoint hinge = hingeOwner.AddComponent<HingeJoint>();
            hinge.connectedBody = connectedRb;
            hinge.autoConfigureConnectedAnchor = false;
            hinge.anchor = hingeOwner.transform.InverseTransformPoint(worldHingePoint);
            hinge.connectedAnchor = connectedRb.transform.InverseTransformPoint(worldHingePoint);
            hinge.axis = hingeOwner.transform.InverseTransformDirection(worldCreaseDirection).normalized;
            hinge.useLimits = true;
            hinge.useSpring = true;
            hinge.enableCollision = false;
            hinge.enablePreprocessing = !disableJointPreprocessing;

            float minAngle = Mathf.Min(crease.minAngle, crease.maxAngle);
            float maxAngle = Mathf.Max(crease.minAngle, crease.maxAngle);
            JointLimits limits = new JointLimits
            {
                min = minAngle,
                max = maxAngle,
                bounciness = 0f,
                contactDistance = 0f
            };
            hinge.limits = limits;

            bool isDriver = FindFaceRoot(faceParents, faceAData.id) != FindFaceRoot(faceParents, faceBData.id);
            if (isDriver)
            {
                UnionFaces(faceParents, faceAData.id, faceBData.id);
                driverCount++;
            }
            else
            {
                followerCount++;
            }

            float stiffnessScale = crease.stiffness > 0f
                ? Mathf.Clamp(crease.stiffness, 0.25f, 4f)
                : 1f;
            bool isActuated = crease.driveMode == OrigamiCrease.DriveMode.Actuated;
            JointSpring spring = new JointSpring
            {
                spring = (isDriver || isActuated ? driverSpring : followerSpring) * stiffnessScale,
                damper = isDriver || isActuated ? driverDamper : followerDamper,
                targetPosition = Mathf.Clamp(crease.restAngle, minAngle, maxAngle)
            };
            hinge.spring = spring;

            OrigamiHingeInfo info = hingeOwner.AddComponent<OrigamiHingeInfo>();
            info.Configure(
                hinge,
                crease.id,
                faceAData.id,
                faceBData.id,
                isDriver,
                isDriver ? 1f : 0.25f);
            info.springDriveEnabled = crease.driveMode != OrigamiCrease.DriveMode.Passive;
            info.actuatorGroup = Mathf.Clamp(crease.actuatorGroup, 0, 4);
            if (isActuated)
            {
                info.isDriver = true;
                info.driveWeight = 1f;
            }
            if (!info.springDriveEnabled)
                hinge.useSpring = false;

            hinges.Add(hinge);
            connectedFaceIds.Add(faceAData.id);
            connectedFaceIds.Add(faceBData.id);
            if (controller != null)
                controller.AddHinge(hinge, info);
        }

        if (pinOneRootFace)
            PinRootFaces(faceParents, connectedFaceIds);

        Debug.Log($"[OrigamiLoader] Hinges ready: drivers={driverCount}, followers={followerCount}.");
    }

    private int FindFaceRoot(Dictionary<int, int> parents, int faceId)
    {
        if (!parents.TryGetValue(faceId, out int parent))
            return faceId;
        if (parent == faceId)
            return faceId;

        int root = FindFaceRoot(parents, parent);
        parents[faceId] = root;
        return root;
    }

    private void UnionFaces(Dictionary<int, int> parents, int faceAId, int faceBId)
    {
        int rootA = FindFaceRoot(parents, faceAId);
        int rootB = FindFaceRoot(parents, faceBId);
        if (rootA != rootB)
            parents[rootB] = rootA;
    }

    private void PinRootFaces(Dictionary<int, int> parents, HashSet<int> connectedFaceIds)
    {
        var pinnedRoots = new HashSet<int>();
        foreach (int faceId in connectedFaceIds)
        {
            int root = FindFaceRoot(parents, faceId);
            if (!pinnedRoots.Add(root))
                continue;
            if (!faceObjects.TryGetValue(faceId, out GameObject faceObject))
                continue;

            Rigidbody body = faceObject.GetComponent<Rigidbody>();
            if (body != null)
                body.isKinematic = true;
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
            Vector3 p1 = GetDisplayedVertex(crease.v1 - 1);
            Vector3 p2 = GetDisplayedVertex(crease.v2 - 1);

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
            lr.useWorldSpace = elasticFlattenActive || attachFace == null;
            if (lr.useWorldSpace)
            {
                Vector3 worldP1 = elasticFlattenActive ? transform.TransformPoint(p1) : p1;
                Vector3 worldP2 = elasticFlattenActive ? transform.TransformPoint(p2) : p2;
                lr.SetPosition(0, worldP1);
                lr.SetPosition(1, worldP2);
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
