using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// DXF 文件转 JSON（OrigamiModel）转换器。
/// 参考 CreasePatternEditor.SaveToJson() 的输出格式。
///
/// 支持的 DXF 实体：
///   - LINE：提取为折痕边
///   - POLYLINE + VERTEX … SEQEND：提取为折痕边
///
/// 颜色映射（RGB → 折痕类型）：
///   (255, 0, 0) = 红色  → Mountain（山折）
///   (0, 0, 255)  = 蓝色 → Valley（谷折）
///   (0, 0, 0)    = 黑色 → Boundary（边界）
///   其它                  → Boundary（边界）
///
/// 颜色来源优先级：实体真彩色(420) > 实体ACI色(62) > 图层真彩色(420) > 图层ACI色(62) > 默认(0,0,0)
///
/// 流程：
///   1. 解析 DXF → 收集所有边 + 层颜色表
///   2. 顶点去重（容差合并）
///   3. 旋转到 XOZ 平面 + 居中
///   4. 构建 OrigamiVertex / OrigamiCrease（含颜色→类型映射）
///   5. 调用 OrigamiFaceGenerator.GenerateFaces 生成面
///   6. 组装 OrigamiModel 并保存为 JSON
/// </summary>
public static class DxfToJsonConverter
{
    private const float VertexMergeTolerance = 0.01f;

    /// <summary>
    /// 解析并转换 DXF 文件为 OrigamiModel JSON
    /// </summary>
    public static bool Convert(string dxfPath, string outputDir = null, string outputFileName = null)
    {
        // --- 1. 读取 & 解析 DXF ---
        if (!File.Exists(dxfPath))
        {
            Debug.LogError($"[DxfToJson] 文件不存在: {dxfPath}");
            return false;
        }

        string[] lines;
        try { lines = File.ReadAllLines(dxfPath); }
        catch (Exception e)
        {
            Debug.LogError($"[DxfToJson] 读取 DXF 失败: {e.Message}");
            return false;
        }

        var rawEdges = ParseDxf(lines);
        if (rawEdges.Count == 0)
        {
            Debug.LogWarning("[DxfToJson] DXF 中未找到任何 LINE 或 POLYLINE 实体");
            return false;
        }

        Debug.Log($"[DxfToJson] 解析到 {rawEdges.Count} 条边");

        // --- 2. 顶点去重 & 索引分配 ---
        var (vertices, edgeIndices) = BuildUniqueVertices(rawEdges);
        Debug.Log($"[DxfToJson] 去重后顶点数: {vertices.Count}");

        // --- 3. 旋转到 XOZ 平面 ---
        RotateToXOZ(vertices);
        Debug.Log($"[DxfToJson] 已旋转到 XOZ 平面");

        // --- 4. 居中 ---
        CenterVertices(vertices);
        Debug.Log($"[DxfToJson] 已居中到原点");

        // --- 5. 构建 OrigamiVertex 列表 ---
        List<global::OrigamiVertex> outVertices = new List<global::OrigamiVertex>();
        for (int i = 0; i < vertices.Count; i++)
        {
            outVertices.Add(new global::OrigamiVertex
            {
                x = vertices[i].x,
                y = vertices[i].y,
                z = vertices[i].z
            });
        }

        // --- 6. 构建 OrigamiCrease 列表（含颜色→类型映射）---
        List<global::OrigamiCrease> outCreases = new List<global::OrigamiCrease>();
        for (int i = 0; i < edgeIndices.Count; i++)
        {
            var (v1Idx, v2Idx) = edgeIndices[i];

            if (v1Idx == v2Idx) continue; // 跳过退化边

            // RGB 颜色 → 折痕类型
            uint rgb = (i < rawEdges.Count) ? rawEdges[i].RgbColor : 0u;
            global::OrigamiCrease.Type mappedType = MapRgbToCreaseType(rgb);

            outCreases.Add(new global::OrigamiCrease
            {
                id = outCreases.Count + 1,
                v1 = v1Idx + 1,
                v2 = v2Idx + 1,
                type = mappedType,
                restAngle = 0f,
                minAngle = 0f,
                maxAngle = 180f,
                stiffness = 1.0f,
                width = 0.02f
            });
        }

        int mountainCount = outCreases.Count(c => c.type == global::OrigamiCrease.Type.Mountain);
        int valleyCount = outCreases.Count(c => c.type == global::OrigamiCrease.Type.Valley);
        int boundaryCount = outCreases.Count(c => c.type == global::OrigamiCrease.Type.Boundary);
        Debug.Log($"[DxfToJson] 有效折痕数: {outCreases.Count} (Mountain:{mountainCount} Valley:{valleyCount} Boundary:{boundaryCount})");

        // 诊断：打印原始边的颜色分布
        var colorStats = rawEdges
            .GroupBy(e => e.RgbColor)
            .Select(g => $"0x{g.Key:X6}(count={g.Count()})");
        Debug.Log($"[DxfToJson] 原始边颜色分布: {string.Join(", ", colorStats)}");

        // --- 7. 生成面 ---
        List<global::OrigamiFace> outFaces =
            OrigamiFaceGenerator.GenerateFaces(outVertices, outCreases);

        Debug.Log($"[DxfToJson] 生成面数: {outFaces.Count}");

        // --- 8. 组装 OrigamiModel ---
        string modelName = string.IsNullOrEmpty(outputFileName)
            ? Path.GetFileNameWithoutExtension(dxfPath)
            : outputFileName;

        global::OrigamiModel model = new global::OrigamiModel
        {
            name = modelName,
            description = $"Converted from DXF: {Path.GetFileName(dxfPath)}",

            vertices = outVertices,
            faces = outFaces,
            creases = outCreases,
            connections = new List<global::OrigamiConnection>(),

            material = new global::OrigamiMaterial
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

        // --- 9. 保存为 JSON ---
        string dir = outputDir ?? Path.Combine(Application.dataPath, "Models");
        try
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string jsonPath = Path.Combine(dir, modelName + ".json");
            string json = JsonUtility.ToJson(model, true);
            File.WriteAllText(jsonPath, json);

            Debug.Log($"[DxfToJson] ✅ 转换成功: {jsonPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DxfToJson] 保存 JSON 失败: {e.Message}");
            return false;
        }
    }

    #region DXF 解析

    private struct DxfToken
    {
        public int Code;
        public string Value;
    }

    /// <summary>
    /// 原始边（含 RGB 颜色，0xRRGGBB 格式）
    /// </summary>
    private struct RawEdge
    {
        public Vector3 Start;
        public Vector3 End;
        public uint RgbColor; // 24-bit RGB: 0xRRGGBB
    }

    /// <summary>
    /// 图层颜色信息（同时存储 ACI 和 TrueColor）
    /// </summary>
    private struct LayerColorInfo
    {
        public int AciColor;    // 组码 62，-1 表示未设置
        public uint TrueColor;  // 组码 420，0 表示未设置
    }

    private static List<RawEdge> ParseDxf(string[] lines)
    {
        // Step 0: 文本 → token 流
        List<DxfToken> tokens = new List<DxfToken>(lines.Length / 2);
        for (int i = 0; i < lines.Length - 1; i += 2)
        {
            if (int.TryParse(lines[i].Trim(), out int code))
            {
                tokens.Add(new DxfToken
                {
                    Code = code,
                    Value = i + 1 < lines.Length ? lines[i + 1].Trim() : ""
                });
            }
        }

        // Step 1: 解析 LAYER 表
        Dictionary<string, LayerColorInfo> layerColors = ParseLayerTable(tokens);
        Debug.Log($"[DxfToJson] 解析到 {layerColors.Count} 个图层");

        // Step 2: 定位到 ENTITIES section
        int idx = 0;
        bool inEntities = false;
        while (idx < tokens.Count)
        {
            if (tokens[idx].Code == 0 && tokens[idx].Value == "SECTION" &&
                idx + 1 < tokens.Count && tokens[idx + 1].Code == 2 &&
                tokens[idx + 1].Value == "ENTITIES")
            {
                inEntities = true;
                idx += 2;
                break;
            }
            idx++;
        }

        if (!inEntities)
        {
            Debug.LogWarning("[DxfToJson] 未找到 ENTITIES section");
            return new List<RawEdge>();
        }

        // Step 3: 遍历实体
        List<RawEdge> edges = new List<RawEdge>();

        while (idx < tokens.Count)
        {
            if (tokens[idx].Code == 0 && tokens[idx].Value == "ENDSEC")
                break;

            if (tokens[idx].Code == 0 && tokens[idx].Value == "LINE")
            {
                ParseLineEntity(tokens, ref idx, edges, layerColors);
            }
            else if (tokens[idx].Code == 0 && tokens[idx].Value == "POLYLINE")
            {
                ParsePolylineEntity(tokens, ref idx, edges, layerColors);
            }
            else
            {
                idx++;
            }
        }

        return edges;
    }

    private static Dictionary<string, LayerColorInfo> ParseLayerTable(List<DxfToken> tokens)
    {
        var layerColors = new Dictionary<string, LayerColorInfo>();

        int idx = 0;
        while (idx < tokens.Count)
        {
            if (tokens[idx].Code == 0 && tokens[idx].Value == "SECTION" &&
                idx + 1 < tokens.Count && tokens[idx + 1].Code == 2 &&
                tokens[idx + 1].Value == "TABLES")
            {
                idx += 2;
                break;
            }
            idx++;
        }

        if (idx >= tokens.Count) return layerColors;

        bool inLayerTable = false;
        while (idx < tokens.Count)
        {
            if (tokens[idx].Code == 0 && tokens[idx].Value == "ENDSEC")
                return layerColors;

            if (tokens[idx].Code == 0 && tokens[idx].Value == "TABLE" &&
                idx + 1 < tokens.Count &&
                tokens[idx + 1].Code == 2 && tokens[idx + 1].Value == "LAYER")
            {
                inLayerTable = true;
                idx += 2;
                continue;
            }

            if (inLayerTable && tokens[idx].Code == 0 && tokens[idx].Value == "ENDTAB")
                return layerColors;

            if (inLayerTable && tokens[idx].Code == 0 && tokens[idx].Value == "LAYER")
            {
                string name = "";
                var info = new LayerColorInfo { AciColor = -1, TrueColor = 0 };
                idx++;

                while (idx < tokens.Count && tokens[idx].Code != 0)
                {
                    if (tokens[idx].Code == 2)
                        name = tokens[idx].Value;
                    else if (tokens[idx].Code == 62)
                        int.TryParse(tokens[idx].Value, out info.AciColor);
                    else if (tokens[idx].Code == 420)
                        uint.TryParse(tokens[idx].Value, out info.TrueColor);
                    idx++;
                }

                if (!string.IsNullOrEmpty(name) && !layerColors.ContainsKey(name))
                {
                    layerColors[name] = info;
                    Debug.Log($"[DxfToJson] 图层 \"{name}\": ACI={info.AciColor}, TrueColor={info.TrueColor} (0x{info.TrueColor:X6})");
                }
            }
            else
            {
                idx++;
            }
        }

        return layerColors;
    }

    /// <summary>
    /// 从 entity 的 ACI(62) 和 TrueColor(420) + layer 的颜色信息 → 最终 RGB 颜色
    /// 优先级: entity.420 > entity.62 > layer.420 > layer.62 > 默认(0,0,0)
    /// </summary>
    private static uint ResolveRgbColor(int entityAci, uint entityTrueColor,
        string layerName, Dictionary<string, LayerColorInfo> layerColors)
    {
        // 1. 实体真彩色 (420) 直接优先
        if (entityTrueColor > 0 && entityTrueColor <= 0xFFFFFF)
            return entityTrueColor;

        // 2. 实体 ACI (62) → RGB
        if (entityAci > 0 && entityAci <= 255)
            return AciToRgb(entityAci);

        // 3. 图层真彩色
        if (!string.IsNullOrEmpty(layerName) && layerColors.TryGetValue(layerName, out var info))
        {
            if (info.TrueColor > 0 && info.TrueColor <= 0xFFFFFF)
                return info.TrueColor;

            if (info.AciColor > 0 && info.AciColor <= 255)
                return AciToRgb(info.AciColor);
        }

        // 4. 默认黑色
        return 0x000000;
    }

    /// <summary>
    /// AutoCAD Color Index → 24-bit RGB (0xRRGGBB)
    /// 仅映射明确的纯色，其它返回 0（黑色=Boundary）。
    /// 不匹配模糊色域，避免将暗红/暗蓝等邻近色误判为折叠线。
    /// </summary>
    private static uint AciToRgb(int aci)
    {
        switch (aci)
        {
            case 1: return 0xFF0000; // Red → Mountain
            case 5: return 0x0000FF; // Blue → Valley
            case 7: return 0x000000; // Black/White → Boundary
            default: return 0x000000; // 其它 → Boundary
        }
    }

    private static void ParseLineEntity(List<DxfToken> tokens, ref int idx,
        List<RawEdge> edges, Dictionary<string, LayerColorInfo> layerColors)
    {
        float x1 = 0, y1 = 0, z1 = 0;
        float x2 = 0, y2 = 0, z2 = 0;
        bool hasStart = false, hasEnd = false;
        int entityAci = -1;
        uint entityTrueColor = 0;
        string layerName = "";

        idx++;

        while (idx < tokens.Count)
        {
            var t = tokens[idx];
            if (t.Code == 0) break;

            switch (t.Code)
            {
                case 10: x1 = TryParseFloat(t.Value); hasStart = true; break;
                case 20: y1 = TryParseFloat(t.Value); break;
                case 30: z1 = TryParseFloat(t.Value); break;
                case 11: x2 = TryParseFloat(t.Value); hasEnd = true; break;
                case 21: y2 = TryParseFloat(t.Value); break;
                case 31: z2 = TryParseFloat(t.Value); break;
                case 62: int.TryParse(t.Value, out entityAci); break;
                case 420: uint.TryParse(t.Value, out entityTrueColor); break;
                case 8: layerName = t.Value; break;
            }
            idx++;
        }

        if (hasStart && hasEnd)
        {
            edges.Add(new RawEdge
            {
                Start = new Vector3(x1, y1, z1),
                End = new Vector3(x2, y2, z2),
                RgbColor = ResolveRgbColor(entityAci, entityTrueColor, layerName, layerColors)
            });
        }
    }

    private static void ParsePolylineEntity(List<DxfToken> tokens, ref int idx,
        List<RawEdge> edges, Dictionary<string, LayerColorInfo> layerColors)
    {
        int entityAci = -1;
        uint entityTrueColor = 0;
        string layerName = "";

        idx++;

        while (idx < tokens.Count && tokens[idx].Code != 0)
        {
            if (tokens[idx].Code == 62)
                int.TryParse(tokens[idx].Value, out entityAci);
            else if (tokens[idx].Code == 420)
                uint.TryParse(tokens[idx].Value, out entityTrueColor);
            else if (tokens[idx].Code == 8)
                layerName = tokens[idx].Value;
            idx++;
        }

        uint rgb = ResolveRgbColor(entityAci, entityTrueColor, layerName, layerColors);

        // 收集 VERTEX 点
        List<Vector3> polyPoints = new List<Vector3>();

        while (idx < tokens.Count)
        {
            if (tokens[idx].Code != 0)
            {
                idx++;
                continue;
            }

            string entityType = tokens[idx].Value;

            if (entityType == "VERTEX")
            {
                float vx = 0, vy = 0, vz = 0;
                bool hasCoord = false;
                idx++;

                while (idx < tokens.Count && tokens[idx].Code != 0)
                {
                    switch (tokens[idx].Code)
                    {
                        case 10: vx = TryParseFloat(tokens[idx].Value); hasCoord = true; break;
                        case 20: vy = TryParseFloat(tokens[idx].Value); break;
                        case 30: vz = TryParseFloat(tokens[idx].Value); break;
                    }
                    idx++;
                }

                if (hasCoord)
                    polyPoints.Add(new Vector3(vx, vy, vz));
            }
            else if (entityType == "SEQEND")
            {
                idx++;
                while (idx < tokens.Count && tokens[idx].Code != 0)
                    idx++;
                break;
            }
            else
            {
                break;
            }
        }

        for (int i = 0; i < polyPoints.Count - 1; i++)
        {
            edges.Add(new RawEdge
            {
                Start = polyPoints[i],
                End = polyPoints[i + 1],
                RgbColor = rgb
            });
        }
    }

    #endregion

    #region RGB → 折痕类型 映射

    /// <summary>
    /// RGB 颜色 → OrigamiCrease.Type：
    ///
    ///   0xFF0000 = (255,0,0) = 红色 → Mountain（山折）
    ///   0x0000FF = (0,0,255)  = 蓝色 → Valley（谷折）
    ///   0x000000 = (0,0,0)    = 黑色 → Boundary（边界）
    ///   其它                     → Boundary（边界）
    /// </summary>
    private static global::OrigamiCrease.Type MapRgbToCreaseType(uint rgb)
    {
        // 屏蔽高字节，只取低 24 位
        rgb &= 0xFFFFFF;

        if (rgb == 0xFF0000) return global::OrigamiCrease.Type.Mountain; // Red
        if (rgb == 0x0000FF) return global::OrigamiCrease.Type.Valley;   // Blue
        if (rgb == 0x000000) return global::OrigamiCrease.Type.Boundary; // Black

        // 模糊匹配：红色系 (R dominant, G & B low)
        byte r = (byte)((rgb >> 16) & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)(rgb & 0xFF);

        if (r > 200 && g < 100 && b < 100) return global::OrigamiCrease.Type.Mountain;
        if (b > 200 && r < 100 && g < 100) return global::OrigamiCrease.Type.Valley;

        return global::OrigamiCrease.Type.Boundary;
    }

    #endregion

    #region 顶点去重

    private static (List<Vector3> vertices, List<(int, int)> edgeIndices) BuildUniqueVertices(
        List<RawEdge> edges)
    {
        List<Vector3> uniqueVerts = new List<Vector3>();
        List<(int, int)> edgePairs = new List<(int, int)>();

        foreach (var edge in edges)
        {
            int i1 = FindOrAddVertex(uniqueVerts, edge.Start);
            int i2 = FindOrAddVertex(uniqueVerts, edge.End);
            edgePairs.Add((i1, i2));
        }

        return (uniqueVerts, edgePairs);
    }

    private static int FindOrAddVertex(List<Vector3> vertices, Vector3 point)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vector3.Distance(vertices[i], point) < VertexMergeTolerance)
                return i;
        }
        vertices.Add(point);
        return vertices.Count - 1;
    }

    #endregion

    #region 几何变换

    /// <summary>
    /// XOY → XOZ：绕 X 轴 +90° → (x, y, z) → (x, -z, y)
    /// DXF 数据 z=0，所以效果：DXF(x, y, 0) → Unity(x, 0, y)
    /// </summary>
    private static void RotateToXOZ(List<Vector3> vertices)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            var v = vertices[i];
            vertices[i] = new Vector3(v.x, -v.z, v.y);
        }
    }

    /// <summary>
    /// 平移使 bounding box 中心对齐到原点
    /// </summary>
    private static void CenterVertices(List<Vector3> vertices)
    {
        if (vertices.Count == 0) return;

        Vector3 min = vertices[0];
        Vector3 max = vertices[0];
        for (int i = 1; i < vertices.Count; i++)
        {
            min = Vector3.Min(min, vertices[i]);
            max = Vector3.Max(max, vertices[i]);
        }

        Vector3 center = (min + max) * 0.5f;
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] -= center;
    }

    #endregion

    #region 工具方法

    private static float TryParseFloat(string s)
    {
        if (float.TryParse(s, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float result))
            return result;
        return 0f;
    }

    #endregion
}
