using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Converts a clean, spatial M/V/B DXF curve network into an OrigamiModel JSON file.
///
/// This is intentionally separate from DxfToJsonConverter:
/// - it keeps 3D coordinates instead of flattening them to XOZ;
/// - it closes closed POLYLINEs and honours their OCS normal;
/// - it asks SpatialFaceGenerator to recover triangular rigid panels from the edge graph.
/// </summary>
public static class Dxf3DToOrigamiConverter
{
    private const uint Black = 0x000000;
    private const uint Red = 0xFF0000;
    private const uint Blue = 0x0000FF;
    public const float DefaultCoplanarityTolerance = 0.001f;

    private struct DxfToken
    {
        public int Code;
        public string Value;
    }

    private struct LayerColorInfo
    {
        public int aciColor;
        public uint trueColor;
    }

    private sealed class Topology
    {
        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<SpatialDxfIndexedEdge> edges = new List<SpatialDxfIndexedEdge>();
    }

    /// <summary>
    /// Public entry point used by SimulationLoader. It returns diagnostics instead of only a bool
    /// so the UI can explain malformed source data without silently loading a wrong model.
    /// </summary>
    public static SpatialDxfImportResult Convert(
        string dxfPath,
        string outputDirectory = null,
        string outputFileName = null,
        SpatialDxfImportOptions options = null)
    {
        options = options ?? new SpatialDxfImportOptions();
        SpatialDxfImportResult result = new SpatialDxfImportResult();

        if (string.IsNullOrWhiteSpace(dxfPath) || !File.Exists(dxfPath))
        {
            result.error = $"3D DXF file does not exist: {dxfPath}";
            Debug.LogError($"[SpatialDxf] {result.error}");
            return result;
        }

        string[] lines;
        try
        {
            lines = File.ReadAllLines(dxfPath);
        }
        catch (Exception exception)
        {
            result.error = $"Unable to read DXF: {exception.Message}";
            Debug.LogError($"[SpatialDxf] {result.error}");
            return result;
        }

        List<DxfCurveSegment> rawEdges = ParseDxf(lines, result.warnings);
        result.parsedEdgeCount = rawEdges.Count;
        if (rawEdges.Count == 0)
        {
            result.error = "No LINE, POLYLINE, or LWPOLYLINE edges were found in the DXF.";
            LogWarnings(result.warnings);
            Debug.LogError($"[SpatialDxf] {result.error}");
            return result;
        }

        TransformCoordinates(rawEdges, options);
        Topology topology = BuildTopology(rawEdges, options.vertexMergeTolerance, result.warnings);
        if (topology.vertices.Count < 3 || topology.edges.Count < 3)
        {
            result.error = "The DXF does not contain enough unique vertices and edges to form a face.";
            LogWarnings(result.warnings);
            Debug.LogError($"[SpatialDxf] {result.error}");
            return result;
        }

        if (options.centerAtOrigin)
            CenterVertices(topology.vertices);

        SpatialFaceGenerationResult faceResult = SpatialFaceGenerator.Generate(
            topology.vertices,
            topology.edges,
            options.minimumTriangleArea);
        result.warnings.AddRange(faceResult.warnings);

        if (faceResult.faces.Count == 0)
        {
            result.error = "No triangular rigid faces could be recovered. Ensure all physical panel edges are present, endpoints meet exactly, and no guide curves are exported.";
            LogWarnings(result.warnings);
            Debug.LogError($"[SpatialDxf] {result.error}");
            return result;
        }

        int invalidFoldEdgeCount = ValidateFaceEdgeUsage(faceResult.faces, topology.edges, result.warnings);
        if (invalidFoldEdgeCount > 0)
        {
            result.error = $"{invalidFoldEdgeCount} Mountain/Valley edge(s) do not touch exactly two recovered faces. The model was not loaded because its hinge topology is incomplete.";
            LogWarnings(result.warnings);
            Debug.LogError($"[SpatialDxf] {result.error}");
            return result;
        }

        List<OrigamiVertex> outputVertices = new List<OrigamiVertex>(topology.vertices.Count);
        for (int i = 0; i < topology.vertices.Count; i++)
        {
            Vector3 vertex = topology.vertices[i];
            outputVertices.Add(new OrigamiVertex { x = vertex.x, y = vertex.y, z = vertex.z });
        }

        List<OrigamiCrease> outputCreases = BuildOutputCreases(topology.edges, options, result);
        float sliderDriveTravelAngle = Mathf.Clamp(options.sliderDriveTravelAngle, 0f, 180f);
        if (result.mountainCount == 0 && result.valleyCount == 0)
        {
            result.warnings.Add(
                "No red Mountain or blue Valley edges were found. All imported edges are Boundary, so no fold hinges will be created.");
        }

        OrigamiModel model = new OrigamiModel
        {
            name = string.IsNullOrWhiteSpace(outputFileName)
                ? Path.GetFileNameWithoutExtension(dxfPath)
                : Path.GetFileNameWithoutExtension(outputFileName),
            description = $"Spatial M/V/B curve import from {Path.GetFileName(dxfPath)}",
            vertices = outputVertices,
            faces = faceResult.faces,
            creases = outputCreases,
            connections = new List<OrigamiConnection>(),
            spatialDxfImportVersion = 1,
            spatialDxfSliderDriveEnabled = !options.markImportedCreasesPassive,
            spatialDxfSliderTravelAngle = sliderDriveTravelAngle,
            material = new OrigamiMaterial
            {
                name = "default",
                metallic = 0f,
                smoothness = 0.5f,
                doubleSided = true
            },
            defaultCreaseWidth = 0.02f,
            mountainCreaseColor = "#FF0000",
            valleyCreaseColor = "#0000FF",
            boundaryCreaseColor = "#000000"
        };

        string directory = outputDirectory;
        if (string.IsNullOrWhiteSpace(directory))
            directory = Path.Combine(Application.dataPath, "Models", "Spatial");

        string name = string.IsNullOrWhiteSpace(outputFileName)
            ? Path.GetFileNameWithoutExtension(dxfPath) + "_spatial"
            : Path.GetFileNameWithoutExtension(outputFileName);
        string jsonPath = Path.Combine(directory, name + ".json");

        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(jsonPath, JsonUtility.ToJson(model, true));

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            result.success = true;
            result.jsonPath = jsonPath;
            result.vertexCount = outputVertices.Count;
            result.faceCount = faceResult.faces.Count;
            Debug.Log($"[SpatialDxf] Imported {Path.GetFileName(dxfPath)}: {result.Summary}; JSON={jsonPath}");
        }
        catch (Exception exception)
        {
            result.error = $"Unable to write converted JSON: {exception.Message}";
            Debug.LogError($"[SpatialDxf] {result.error}");
        }

        return result;
    }

    /// <summary>
    /// Inspects supported curve entities in their DXF WCS coordinates.  2D
    /// POLYLINE and LWPOLYLINE OCS coordinates are converted before the plane
    /// test, which makes this safe for elevated and inclined Rhino drawings.
    /// </summary>
    public static DxfImportAnalysis AnalyzeSource(
        string dxfPath,
        float coplanarityTolerance = DefaultCoplanarityTolerance)
    {
        List<DxfCurveSegment> ignored;
        DxfImportAnalysis analysis;
        TryReadSourceCurves(dxfPath, coplanarityTolerance, out ignored, out analysis);
        return analysis;
    }

    /// <summary>
    /// Shared internal source reader for DxfImportRouter.  Keeping it beside
    /// the spatial parser guarantees that Auto routing sees exactly the same
    /// LINE / POLYLINE / LWPOLYLINE geometry as the 3D importer.
    /// </summary>
    internal static bool TryReadSourceCurves(
        string dxfPath,
        float coplanarityTolerance,
        out List<DxfCurveSegment> curves,
        out DxfImportAnalysis analysis)
    {
        curves = new List<DxfCurveSegment>();
        analysis = new DxfImportAnalysis();
        coplanarityTolerance = Mathf.Max(0.000001f, Mathf.Abs(coplanarityTolerance));

        if (string.IsNullOrWhiteSpace(dxfPath))
        {
            analysis.error = "DXF path is empty.";
            return false;
        }

        if (!File.Exists(dxfPath))
        {
            analysis.error = $"DXF file does not exist: {dxfPath}";
            return false;
        }

        analysis.sourceExists = true;
        string[] lines;
        try
        {
            lines = File.ReadAllLines(dxfPath);
        }
        catch (Exception exception)
        {
            analysis.error = $"Unable to read DXF: {exception.Message}";
            return false;
        }

        curves = ParseDxf(lines, analysis.warnings);
        analysis.curveCount = curves.Count;
        analysis.hasSupportedCurves = curves.Count > 0;
        if (!analysis.hasSupportedCurves)
        {
            analysis.error = "No LINE, POLYLINE, or LWPOLYLINE curve segments were found in the DXF.";
            return false;
        }

        AnalyzeCoplanarity(curves, coplanarityTolerance, analysis);
        if (!string.IsNullOrEmpty(analysis.error))
            return false;

        analysis.recommendedMode = analysis.isCoplanar
            ? DxfImportMode.Flat2D
            : DxfImportMode.Spatial3D;
        return true;
    }

    private static List<OrigamiCrease> BuildOutputCreases(
        List<SpatialDxfIndexedEdge> edges,
        SpatialDxfImportOptions options,
        SpatialDxfImportResult result)
    {
        List<OrigamiCrease> creases = new List<OrigamiCrease>(edges.Count);
        for (int i = 0; i < edges.Count; i++)
        {
            SpatialDxfIndexedEdge edge = edges[i];
            OrigamiCrease.Type type = edge.type;
            if (type == OrigamiCrease.Type.Mountain) result.mountainCount++;
            else if (type == OrigamiCrease.Type.Valley) result.valleyCount++;
            else result.boundaryCount++;

            // A HingeJoint treats the pose at creation as its zero-angle pose.
            // For direct slider drive, slider value 0 therefore holds the imported
            // spatial pose and value 1 advances every M/V hinge to the configured
            // common target angle. Boundary edges are never driven.
            bool useSliderDrive = type != OrigamiCrease.Type.Boundary
                && !options.markImportedCreasesPassive;
            float sliderTravelAngle = Mathf.Clamp(options.sliderDriveTravelAngle, 0f, 180f);

            creases.Add(new OrigamiCrease
            {
                id = i + 1,
                v1 = edge.v1 + 1,
                v2 = edge.v2 + 1,
                type = type,
                driveMode = useSliderDrive
                    ? OrigamiCrease.DriveMode.Auto
                    : OrigamiCrease.DriveMode.Passive,
                // The imported mesh pose is the HingeJoint zero pose. Keep slider
                // progress 0 at that pose; M/V owner selection in OrigamiLoader
                // already supplies the Mountain/Valley folding direction.
                restAngle = 0f,
                minAngle = useSliderDrive ? 0f : -180f,
                maxAngle = useSliderDrive ? sliderTravelAngle : 180f,
                stiffness = 1f,
                width = 0.02f
            });
        }

        return creases;
    }

    private static List<DxfCurveSegment> ParseDxf(string[] lines, List<string> warnings)
    {
        List<DxfToken> tokens = Tokenize(lines);
        Dictionary<string, LayerColorInfo> layers = ParseLayerTable(tokens);
        int index = FindEntitiesSection(tokens);
        List<DxfCurveSegment> edges = new List<DxfCurveSegment>();
        if (index < 0)
        {
            warnings.Add("DXF does not contain an ENTITIES section.");
            return edges;
        }

        HashSet<string> warnedUnsupported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (index < tokens.Count)
        {
            if (tokens[index].Code == 0 && Is(tokens[index].Value, "ENDSEC"))
                break;

            if (tokens[index].Code != 0)
            {
                index++;
                continue;
            }

            string entityType = tokens[index].Value;
            if (Is(entityType, "LINE"))
            {
                ParseLine(tokens, ref index, layers, edges);
            }
            else if (Is(entityType, "POLYLINE"))
            {
                ParsePolyline(tokens, ref index, layers, edges, warnings);
            }
            else if (Is(entityType, "LWPOLYLINE"))
            {
                ParseLightweightPolyline(tokens, ref index, layers, edges, warnings);
            }
            else
            {
                if ((Is(entityType, "REGION") || Is(entityType, "3DFACE") || Is(entityType, "MESH") || Is(entityType, "3DSOLID")) &&
                    warnedUnsupported.Add(entityType))
                {
                    warnings.Add($"{entityType} is not used by the curve importer. Export the M/V/B crease network as LINE or POLYLINE entities.");
                }
                SkipEntity(tokens, ref index);
            }
        }

        return edges;
    }

    private static List<DxfToken> Tokenize(string[] lines)
    {
        List<DxfToken> tokens = new List<DxfToken>(lines.Length / 2);
        for (int i = 0; i + 1 < lines.Length; i += 2)
        {
            if (int.TryParse(lines[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int code))
            {
                tokens.Add(new DxfToken { Code = code, Value = lines[i + 1].Trim() });
            }
        }

        return tokens;
    }

    private static int FindEntitiesSection(List<DxfToken> tokens)
    {
        for (int i = 0; i + 1 < tokens.Count; i++)
        {
            if (tokens[i].Code == 0 && Is(tokens[i].Value, "SECTION") &&
                tokens[i + 1].Code == 2 && Is(tokens[i + 1].Value, "ENTITIES"))
                return i + 2;
        }

        return -1;
    }

    private static Dictionary<string, LayerColorInfo> ParseLayerTable(List<DxfToken> tokens)
    {
        Dictionary<string, LayerColorInfo> layers = new Dictionary<string, LayerColorInfo>();
        int index = 0;
        while (index < tokens.Count)
        {
            if (tokens[index].Code == 0 && Is(tokens[index].Value, "SECTION") &&
                index + 1 < tokens.Count && tokens[index + 1].Code == 2 && Is(tokens[index + 1].Value, "TABLES"))
            {
                index += 2;
                break;
            }
            index++;
        }

        bool inLayerTable = false;
        while (index < tokens.Count)
        {
            if (tokens[index].Code == 0 && Is(tokens[index].Value, "ENDSEC"))
                break;

            if (tokens[index].Code == 0 && Is(tokens[index].Value, "TABLE") &&
                index + 1 < tokens.Count && tokens[index + 1].Code == 2 && Is(tokens[index + 1].Value, "LAYER"))
            {
                inLayerTable = true;
                index += 2;
                continue;
            }

            if (inLayerTable && tokens[index].Code == 0 && Is(tokens[index].Value, "ENDTAB"))
                break;

            if (inLayerTable && tokens[index].Code == 0 && Is(tokens[index].Value, "LAYER"))
            {
                string name = string.Empty;
                LayerColorInfo info = new LayerColorInfo { aciColor = -1, trueColor = 0 };
                index++;
                while (index < tokens.Count && tokens[index].Code != 0)
                {
                    if (tokens[index].Code == 2) name = tokens[index].Value;
                    else if (tokens[index].Code == 62) int.TryParse(tokens[index].Value, out info.aciColor);
                    else if (tokens[index].Code == 420) uint.TryParse(tokens[index].Value, out info.trueColor);
                    index++;
                }

                if (!string.IsNullOrWhiteSpace(name))
                    layers[name] = info;
                continue;
            }

            index++;
        }

        return layers;
    }

    private static void ParseLine(
        List<DxfToken> tokens,
        ref int index,
        Dictionary<string, LayerColorInfo> layers,
        List<DxfCurveSegment> edges)
    {
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;
        bool hasStart = false;
        bool hasEnd = false;
        int aci = -1;
        uint trueColor = 0;
        string layer = string.Empty;

        index++;
        while (index < tokens.Count && tokens[index].Code != 0)
        {
            DxfToken token = tokens[index];
            switch (token.Code)
            {
                case 10: start.x = ParseFloat(token.Value); hasStart = true; break;
                case 20: start.y = ParseFloat(token.Value); break;
                case 30: start.z = ParseFloat(token.Value); break;
                case 11: end.x = ParseFloat(token.Value); hasEnd = true; break;
                case 21: end.y = ParseFloat(token.Value); break;
                case 31: end.z = ParseFloat(token.Value); break;
                case 62: int.TryParse(token.Value, out aci); break;
                case 420: uint.TryParse(token.Value, out trueColor); break;
                case 8: layer = token.Value; break;
            }
            index++;
        }

        if (hasStart && hasEnd)
            edges.Add(new DxfCurveSegment { start = start, end = end, rgbColor = ResolveRgbColor(aci, trueColor, layer, layers) });
    }

    private static void ParsePolyline(
        List<DxfToken> tokens,
        ref int index,
        Dictionary<string, LayerColorInfo> layers,
        List<DxfCurveSegment> edges,
        List<string> warnings)
    {
        int aci = -1;
        uint trueColor = 0;
        string layer = string.Empty;
        int flags = 0;
        float elevation = 0f;
        Vector3 normal = Vector3.forward;
        List<Vector3> rawPoints = new List<Vector3>();

        index++;
        while (index < tokens.Count && tokens[index].Code != 0)
        {
            DxfToken token = tokens[index];
            switch (token.Code)
            {
                case 8: layer = token.Value; break;
                case 62: int.TryParse(token.Value, out aci); break;
                case 420: uint.TryParse(token.Value, out trueColor); break;
                case 70: int.TryParse(token.Value, out flags); break;
                case 30: elevation = ParseFloat(token.Value); break;
                case 210: normal.x = ParseFloat(token.Value); break;
                case 220: normal.y = ParseFloat(token.Value); break;
                case 230: normal.z = ParseFloat(token.Value); break;
            }
            index++;
        }

        // Mesh POLYLINE records use VERTEX entries for mesh coordinates and face indices,
        // not for an ordered crease curve. Treating them as a curve would silently invent
        // wrong M/V/B edges. The spatial curve pipeline deliberately accepts only curves.
        if ((flags & 16) != 0 || (flags & 64) != 0)
        {
            warnings.Add("POLYLINE polygon/polyface mesh was ignored. Export only the coloured M/V/B curve network for this importer.");
            SkipPolylineSequence(tokens, ref index);
            return;
        }

        while (index < tokens.Count && tokens[index].Code == 0)
        {
            string entity = tokens[index].Value;
            if (Is(entity, "VERTEX"))
            {
                Vector3 point = Vector3.zero;
                bool hasPoint = false;
                bool hasZ = false;
                index++;
                while (index < tokens.Count && tokens[index].Code != 0)
                {
                    DxfToken token = tokens[index];
                    switch (token.Code)
                    {
                        case 10: point.x = ParseFloat(token.Value); hasPoint = true; break;
                        case 20: point.y = ParseFloat(token.Value); break;
                        case 30: point.z = ParseFloat(token.Value); hasZ = true; break;
                    }
                    index++;
                }
                if (hasPoint)
                {
                    if (!hasZ && (flags & 8) == 0)
                        point.z = elevation;
                    rawPoints.Add(point);
                }
                continue;
            }

            if (Is(entity, "SEQEND"))
            {
                SkipEntity(tokens, ref index);
                break;
            }

            break;
        }

        if (rawPoints.Count < 2)
        {
            warnings.Add("A POLYLINE with fewer than two vertices was ignored.");
            return;
        }

        bool isThreeDimensionalPolyline = (flags & 8) != 0;
        if (!isThreeDimensionalPolyline)
        {
            for (int i = 0; i < rawPoints.Count; i++)
                rawPoints[i] = OcsToWcs(rawPoints[i], normal);
        }

        AddPolylineSegments(rawPoints, (flags & 1) != 0, ResolveRgbColor(aci, trueColor, layer, layers), edges);
    }

    private static void ParseLightweightPolyline(
        List<DxfToken> tokens,
        ref int index,
        Dictionary<string, LayerColorInfo> layers,
        List<DxfCurveSegment> edges,
        List<string> warnings)
    {
        int aci = -1;
        uint trueColor = 0;
        string layer = string.Empty;
        int flags = 0;
        float elevation = 0f;
        Vector3 normal = Vector3.forward;
        List<Vector3> rawPoints = new List<Vector3>();
        bool hasPendingPoint = false;
        float pendingX = 0f;
        float pendingY = 0f;
        bool hasBulge = false;

        index++;
        while (index < tokens.Count && tokens[index].Code != 0)
        {
            DxfToken token = tokens[index];
            switch (token.Code)
            {
                case 8: layer = token.Value; break;
                case 62: int.TryParse(token.Value, out aci); break;
                case 420: uint.TryParse(token.Value, out trueColor); break;
                case 70: int.TryParse(token.Value, out flags); break;
                case 38: elevation = ParseFloat(token.Value); break;
                case 210: normal.x = ParseFloat(token.Value); break;
                case 220: normal.y = ParseFloat(token.Value); break;
                case 230: normal.z = ParseFloat(token.Value); break;
                case 10:
                    if (hasPendingPoint)
                        rawPoints.Add(new Vector3(pendingX, pendingY, elevation));
                    pendingX = ParseFloat(token.Value);
                    pendingY = 0f;
                    hasPendingPoint = true;
                    break;
                case 20: pendingY = ParseFloat(token.Value); break;
                case 42:
                    if (Mathf.Abs(ParseFloat(token.Value)) > 0.000001f)
                        hasBulge = true;
                    break;
            }
            index++;
        }

        if (hasPendingPoint)
            rawPoints.Add(new Vector3(pendingX, pendingY, elevation));
        if (hasBulge)
            warnings.Add("LWPOLYLINE bulge arcs are approximated as straight segments; use straight M/V/B fold lines for reliable topology.");
        if (rawPoints.Count < 2)
            return;

        for (int i = 0; i < rawPoints.Count; i++)
            rawPoints[i] = OcsToWcs(rawPoints[i], normal);

        AddPolylineSegments(rawPoints, (flags & 1) != 0, ResolveRgbColor(aci, trueColor, layer, layers), edges);
    }

    private static void AddPolylineSegments(List<Vector3> points, bool closed, uint rgbColor, List<DxfCurveSegment> edges)
    {
        for (int i = 0; i < points.Count - 1; i++)
            edges.Add(new DxfCurveSegment { start = points[i], end = points[i + 1], rgbColor = rgbColor });

        if (closed && points.Count > 2)
            edges.Add(new DxfCurveSegment { start = points[points.Count - 1], end = points[0], rgbColor = rgbColor });
    }

    private static void SkipPolylineSequence(List<DxfToken> tokens, ref int index)
    {
        while (index < tokens.Count && tokens[index].Code == 0)
        {
            string entity = tokens[index].Value;
            if (!Is(entity, "VERTEX") && !Is(entity, "SEQEND"))
                break;

            bool isSequenceEnd = Is(entity, "SEQEND");
            SkipEntity(tokens, ref index);
            if (isSequenceEnd)
                break;
        }
    }

    private static void SkipEntity(List<DxfToken> tokens, ref int index)
    {
        if (index < tokens.Count && tokens[index].Code == 0)
            index++;
        while (index < tokens.Count && tokens[index].Code != 0)
            index++;
    }

    private static uint ResolveRgbColor(
        int entityAci,
        uint entityTrueColor,
        string layerName,
        Dictionary<string, LayerColorInfo> layers)
    {
        if (entityTrueColor > 0 && entityTrueColor <= 0xFFFFFF)
            return entityTrueColor;
        if (entityAci > 0 && entityAci <= 255)
            return AciToRgb(entityAci);

        if (!string.IsNullOrEmpty(layerName) && layers.TryGetValue(layerName, out LayerColorInfo layer))
        {
            if (layer.trueColor > 0 && layer.trueColor <= 0xFFFFFF)
                return layer.trueColor;
            if (layer.aciColor > 0 && layer.aciColor <= 255)
                return AciToRgb(layer.aciColor);
        }

        return Black;
    }

    private static uint AciToRgb(int aci)
    {
        switch (aci)
        {
            case 1: return Red;
            case 5: return Blue;
            case 7: return Black;
            default: return Black;
        }
    }

    private static OrigamiCrease.Type MapRgbToCreaseType(uint rgb)
    {
        rgb &= 0xFFFFFF;
        if (rgb == Red) return OrigamiCrease.Type.Mountain;
        if (rgb == Blue) return OrigamiCrease.Type.Valley;

        byte r = (byte)((rgb >> 16) & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)(rgb & 0xFF);
        if (r > 200 && g < 100 && b < 100) return OrigamiCrease.Type.Mountain;
        if (b > 200 && r < 100 && g < 100) return OrigamiCrease.Type.Valley;
        return OrigamiCrease.Type.Boundary;
    }

    private static void TransformCoordinates(List<DxfCurveSegment> rawEdges, SpatialDxfImportOptions options)
    {
        for (int i = 0; i < rawEdges.Count; i++)
        {
            DxfCurveSegment edge = rawEdges[i];
            edge.start = ToUnityCoordinates(edge.start, options);
            edge.end = ToUnityCoordinates(edge.end, options);
            rawEdges[i] = edge;
        }
    }

    private static void AnalyzeCoplanarity(
        List<DxfCurveSegment> curves,
        float tolerance,
        DxfImportAnalysis analysis)
    {
        const float PointEqualityTolerance = 0.000001f;
        List<Vector3> points = new List<Vector3>(curves.Count * 2);
        float squaredPointTolerance = PointEqualityTolerance * PointEqualityTolerance;

        for (int i = 0; i < curves.Count; i++)
        {
            AddUniquePoint(points, curves[i].start, squaredPointTolerance);
            AddUniquePoint(points, curves[i].end, squaredPointTolerance);
        }

        analysis.uniquePointCount = points.Count;
        if (points.Count == 0)
        {
            analysis.error = "The DXF curve source did not contain any usable endpoints.";
            return;
        }

        Vector3 origin = points[0];
        analysis.planeOrigin = origin;

        int firstDifferentPoint = -1;
        for (int i = 1; i < points.Count; i++)
        {
            if ((points[i] - origin).sqrMagnitude > squaredPointTolerance)
            {
                firstDifferentPoint = i;
                break;
            }
        }

        if (firstDifferentPoint < 0)
        {
            analysis.planeNormal = Vector3.forward;
            analysis.maxDistanceFromPlane = 0f;
            analysis.isCoplanar = true;
            analysis.warnings.Add("All DXF curve endpoints are coincident; the source is treated as coplanar but cannot form a usable crease pattern.");
            return;
        }

        Vector3 lineDirection = points[firstDifferentPoint] - origin;
        float lineLength = lineDirection.magnitude;
        Vector3 normal = Vector3.zero;
        for (int i = firstDifferentPoint + 1; i < points.Count; i++)
        {
            Vector3 fromOrigin = points[i] - origin;
            Vector3 cross = Vector3.Cross(lineDirection, fromOrigin);
            float distanceFromLine = cross.magnitude / lineLength;
            if (distanceFromLine > PointEqualityTolerance)
            {
                normal = cross.normalized;
                break;
            }
        }

        if (normal.sqrMagnitude < 0.999f)
        {
            Vector3 reference = Mathf.Abs(lineDirection.y / lineLength) < 0.9f
                ? Vector3.up
                : Vector3.right;
            normal = Vector3.Cross(lineDirection, reference).normalized;
            analysis.warnings.Add("All DXF curve endpoints are collinear; the source is treated as coplanar but cannot form a rigid face.");
        }

        float maxDistance = 0f;
        for (int i = 0; i < points.Count; i++)
        {
            float distance = Mathf.Abs(Vector3.Dot(points[i] - origin, normal));
            if (distance > maxDistance)
                maxDistance = distance;
        }

        analysis.planeNormal = normal;
        analysis.maxDistanceFromPlane = maxDistance;
        analysis.isCoplanar = maxDistance <= tolerance;
        if (!analysis.isCoplanar)
        {
            analysis.warnings.Add(
                $"DXF endpoints deviate {maxDistance.ToString("G9", CultureInfo.InvariantCulture)} from one plane (tolerance {tolerance.ToString("G9", CultureInfo.InvariantCulture)}); Auto will use Spatial3D.");
        }
    }

    private static void AddUniquePoint(List<Vector3> points, Vector3 point, float squaredTolerance)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if ((points[i] - point).sqrMagnitude <= squaredTolerance)
                return;
        }

        points.Add(point);
    }

    private static Vector3 ToUnityCoordinates(Vector3 dxfPoint, SpatialDxfImportOptions options)
    {
        // Rhino is Z-up while Unity is Y-up. This is a +90 degree rotation around X,
        // not a projection, so a folded model remains spatial.
        return options.rhinoZUpToUnityYUp
            ? new Vector3(dxfPoint.x, dxfPoint.z, -dxfPoint.y)
            : dxfPoint;
    }

    private static Topology BuildTopology(List<DxfCurveSegment> rawEdges, float tolerance, List<string> warnings)
    {
        Topology topology = new Topology();
        Dictionary<long, int> edgeLookup = new Dictionary<long, int>();
        for (int i = 0; i < rawEdges.Count; i++)
        {
            DxfCurveSegment raw = rawEdges[i];
            int start = FindOrAddVertex(topology.vertices, raw.start, tolerance);
            int end = FindOrAddVertex(topology.vertices, raw.end, tolerance);
            if (start == end)
            {
                warnings.Add($"A zero-length segment at source edge {i + 1} was ignored.");
                continue;
            }

            OrigamiCrease.Type type = MapRgbToCreaseType(raw.rgbColor);
            long key = MakeEdgeKey(start, end);
            if (edgeLookup.TryGetValue(key, out int existingIndex))
            {
                SpatialDxfIndexedEdge existing = topology.edges[existingIndex];
                existing.type = MergeCreaseTypes(existing.type, type, start, end, warnings);
                topology.edges[existingIndex] = existing;
                continue;
            }

            edgeLookup[key] = topology.edges.Count;
            topology.edges.Add(new SpatialDxfIndexedEdge(start, end, type));
        }

        return topology;
    }

    private static OrigamiCrease.Type MergeCreaseTypes(
        OrigamiCrease.Type first,
        OrigamiCrease.Type second,
        int start,
        int end,
        List<string> warnings)
    {
        if (first == second)
            return first;
        if (first == OrigamiCrease.Type.Boundary)
            return second;
        if (second == OrigamiCrease.Type.Boundary)
            return first;

        warnings.Add($"Edge ({start + 1}, {end + 1}) was exported both Mountain and Valley; its first non-boundary colour is kept.");
        return first;
    }

    private static int FindOrAddVertex(List<Vector3> vertices, Vector3 point, float tolerance)
    {
        float squaredTolerance = tolerance * tolerance;
        for (int i = 0; i < vertices.Count; i++)
        {
            if ((vertices[i] - point).sqrMagnitude <= squaredTolerance)
                return i;
        }

        vertices.Add(point);
        return vertices.Count - 1;
    }

    private static void CenterVertices(List<Vector3> vertices)
    {
        if (vertices.Count == 0)
            return;

        Vector3 min = vertices[0];
        Vector3 max = vertices[0];
        for (int i = 1; i < vertices.Count; i++)
        {
            min = Vector3.Min(min, vertices[i]);
            max = Vector3.Max(max, vertices[i]);
        }

        Vector3 offset = (min + max) * 0.5f;
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] -= offset;
    }

    private static int ValidateFaceEdgeUsage(
        List<OrigamiFace> faces,
        List<SpatialDxfIndexedEdge> edges,
        List<string> warnings)
    {
        Dictionary<long, int> faceUseCount = new Dictionary<long, int>();
        for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
        {
            List<int> faceVertices = faces[faceIndex].vertices;
            if (faceVertices == null || faceVertices.Count < 3)
                continue;

            for (int i = 0; i < faceVertices.Count; i++)
            {
                int first = faceVertices[i] - 1;
                int second = faceVertices[(i + 1) % faceVertices.Count] - 1;
                long key = MakeEdgeKey(first, second);
                faceUseCount[key] = faceUseCount.TryGetValue(key, out int current) ? current + 1 : 1;
            }
        }

        int invalidFoldEdges = 0;
        for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++)
        {
            SpatialDxfIndexedEdge edge = edges[edgeIndex];
            int faceCount = faceUseCount.TryGetValue(MakeEdgeKey(edge.v1, edge.v2), out int count) ? count : 0;
            if (edge.type == OrigamiCrease.Type.Boundary)
            {
                if (faceCount != 1)
                {
                    warnings.Add(
                        $"Boundary edge ({edge.v1 + 1}, {edge.v2 + 1}) belongs to {faceCount} recovered faces; expected one.");
                }
                continue;
            }

            if (faceCount != 2)
            {
                invalidFoldEdges++;
                warnings.Add(
                    $"{edge.type} edge ({edge.v1 + 1}, {edge.v2 + 1}) belongs to {faceCount} recovered faces; a fold hinge needs exactly two.");
            }
        }

        return invalidFoldEdges;
    }

    private static Vector3 OcsToWcs(Vector3 point, Vector3 normal)
    {
        if (normal.sqrMagnitude < 0.0000001f)
            return point;

        Vector3 zAxis = normal.normalized;
        Vector3 reference = Mathf.Abs(zAxis.x) < (1f / 64f) && Mathf.Abs(zAxis.y) < (1f / 64f)
            ? Vector3.up
            : Vector3.forward;
        Vector3 xAxis = Vector3.Cross(reference, zAxis).normalized;
        Vector3 yAxis = Vector3.Cross(zAxis, xAxis).normalized;
        return point.x * xAxis + point.y * yAxis + point.z * zAxis;
    }

    private static long MakeEdgeKey(int a, int b)
    {
        int low = Mathf.Min(a, b);
        int high = Mathf.Max(a, b);
        return ((long)low << 32) | (uint)high;
    }

    private static float ParseFloat(string value)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
            ? parsed
            : 0f;
    }

    private static bool Is(string value, string expected)
    {
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static void LogWarnings(List<string> warnings)
    {
        for (int i = 0; i < warnings.Count; i++)
            Debug.LogWarning($"[SpatialDxf] {warnings[i]}");
    }
}
