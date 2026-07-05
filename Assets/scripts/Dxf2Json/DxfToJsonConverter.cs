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
///   - LINE：提取为折痕边（默认 Boundary 类型）
///   - POLYLINE + VERTEX … SEQEND：提取为折痕边
///   - 所有 Z 坐标取 0（DXF 是 2D 图纸）
///
/// 流程：
///   1. 解析 DXF → 收集所有边（线段端点）
///   2. 顶点去重（容差合并）
///   3. 旋转到 XOZ 平面（DXF 的 XOY → Unity 的 XOZ）
///   4. 构建 OrigamiVertex / OrigamiCrease 列表
///   5. 调用 OrigamiFaceGenerator.GenerateFaces 生成面
///   6. 组装 OrigamiModel 并保存为 JSON
/// </summary>
public static class DxfToJsonConverter
{
    private const float VertexMergeTolerance = 0.01f;

    /// <summary>
    /// 解析并转换 DXF 文件为 OrigamiModel JSON
    /// </summary>
    /// <param name="dxfPath">DXF 文件绝对路径</param>
    /// <param name="outputDir">输出目录（默认 Assets/Models）</param>
    /// <param name="outputFileName">输出文件名（不含 .json，默认取 DXF 文件名）</param>
    /// <returns>是否转换成功</returns>
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

        // --- 2.5 旋转到 XOZ 平面 ---
        // DXF 是 XOY 平面（2D 图纸），折纸模拟需要 XOZ 平面（Unity 水平面）
        // +90° 绕 X 轴: (x, y, z) → (x, -z, y) 即 DXF(x, y, 0) → (x, 0, y)
        RotateToXOZ(vertices);
        Debug.Log($"[DxfToJson] 已旋转到 XOZ 平面");

        // --- 2.6 居中 ---
        CenterVertices(vertices);
        Debug.Log($"[DxfToJson] 已居中到原点");

        // --- 3. 构建 OrigamiVertex 列表 ---
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

        // --- 4. 构建 OrigamiCrease 列表 ---
        List<global::OrigamiCrease> outCreases = new List<global::OrigamiCrease>();
        for (int i = 0; i < edgeIndices.Count; i++)
        {
            var (v1Idx, v2Idx) = edgeIndices[i];

            // 跳过退化边（两端点合并为同一点）
            if (v1Idx == v2Idx)
                continue;

            outCreases.Add(new global::OrigamiCrease
            {
                id = outCreases.Count + 1, // 连续 ID（跳过退化边后重新编号）
                v1 = v1Idx + 1, // 转为 1-based
                v2 = v2Idx + 1,
                type = global::OrigamiCrease.Type.Boundary, // DXF 默认当作边界
                restAngle = 0f,
                minAngle = 0f,
                maxAngle = 180f,
                stiffness = 1.0f,
                width = 0.02f
            });
        }

        Debug.Log($"[DxfToJson] 有效折痕数: {outCreases.Count}");

        // --- 5. 生成面 ---
        List<global::OrigamiFace> outFaces =
            OrigamiFaceGenerator.GenerateFaces(outVertices, outCreases);

        Debug.Log($"[DxfToJson] 生成面数: {outFaces.Count}");

        // --- 6. 组装 OrigamiModel ---
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

        // --- 7. 保存为 JSON ---
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

    /// <summary>
    /// DXF group-code token
    /// </summary>
    private struct DxfToken
    {
        public int Code;
        public string Value;
    }

    /// <summary>
    /// 原始边（两个端点，未去重）
    /// </summary>
    private struct RawEdge
    {
        public Vector3 Start;
        public Vector3 End;
    }

    /// <summary>
    /// 解析 DXF ASCII 格式，提取所有 LINE 和 POLYLINE 实体作为边
    /// </summary>
    private static List<RawEdge> ParseDxf(string[] lines)
    {
        // --- Step 0: 将 DXF 文本解析为 token 流 ---
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

        // --- Step 1: 找到 ENTITIES section ---
        int idx = 0;
        bool inEntities = false;

        // 定位到 ENTITIES section
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

        // --- Step 2: 遍历实体 ---
        List<RawEdge> edges = new List<RawEdge>();

        while (idx < tokens.Count)
        {
            // 遇到下一个 section 就结束
            if (tokens[idx].Code == 0 && tokens[idx].Value == "ENDSEC")
                break;

            if (tokens[idx].Code == 0 && tokens[idx].Value == "LINE")
            {
                ParseLineEntity(tokens, ref idx, edges);
            }
            else if (tokens[idx].Code == 0 && tokens[idx].Value == "POLYLINE")
            {
                ParsePolylineEntity(tokens, ref idx, edges);
            }
            else
            {
                idx++;
            }
        }

        return edges;
    }

    /// <summary>
    /// 解析 LINE 实体：跳过到下一个 0，沿途收集 10/20/30 和 11/21/31
    /// </summary>
    private static void ParseLineEntity(List<DxfToken> tokens, ref int idx, List<RawEdge> edges)
    {
        float x1 = 0, y1 = 0, z1 = 0;
        float x2 = 0, y2 = 0, z2 = 0;
        bool hasStart = false, hasEnd = false;

        idx++; // 跳过 "LINE" token

        while (idx < tokens.Count)
        {
            var t = tokens[idx];

            // 遇到下一个实体或 section 结束 → 停止
            if (t.Code == 0)
                break;

            switch (t.Code)
            {
                case 10: x1 = TryParseFloat(t.Value); hasStart = true; break;
                case 20: y1 = TryParseFloat(t.Value); break;
                case 30: z1 = TryParseFloat(t.Value); break;
                case 11: x2 = TryParseFloat(t.Value); hasEnd = true; break;
                case 21: y2 = TryParseFloat(t.Value); break;
                case 31: z2 = TryParseFloat(t.Value); break;
            }
            idx++;
        }

        if (hasStart && hasEnd)
        {
            edges.Add(new RawEdge
            {
                Start = new Vector3(x1, y1, z1),
                End = new Vector3(x2, y2, z2)
            });
        }
    }

    /// <summary>
    /// 解析 POLYLINE 实体及其 VERTEX 子实体。
    /// 从 POLYLINE 之后逐个 VERTEX 收集坐标，直到 SEQEND。
    /// 将相邻 VERTEX 两两连成边（开放折线）。
    /// </summary>
    private static void ParsePolylineEntity(List<DxfToken> tokens, ref int idx, List<RawEdge> edges)
    {
        idx++; // 跳过 "POLYLINE" token

        // 跳过 POLYLINE 的属性 token，直到下一个 0 实体标记
        while (idx < tokens.Count && tokens[idx].Code != 0)
            idx++;

        // 收集所有 VERTEX 点
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
                idx++; // 跳过 "VERTEX"

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
                idx++; // 跳过 SEQEND 的属性，到下一个实体
                while (idx < tokens.Count && tokens[idx].Code != 0)
                    idx++;
                break;
            }
            else
            {
                break; // 遇到其他实体（不应出现，但安全兜底）
            }
        }

        // 将相邻顶点连成边
        for (int i = 0; i < polyPoints.Count - 1; i++)
        {
            edges.Add(new RawEdge
            {
                Start = polyPoints[i],
                End = polyPoints[i + 1]
            });
        }
    }

    #endregion

    #region 顶点去重

    /// <summary>
    /// 将所有边的端点去重，返回唯一顶点列表和每条边对应的顶点索引对
    /// </summary>
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
        // 查找容差范围内的已有顶点
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vector3.Distance(vertices[i], point) < VertexMergeTolerance)
                return i;
        }

        // 未找到 → 新增
        vertices.Add(point);
        return vertices.Count - 1;
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 将顶点从 XOY 平面（DXF 2D 图纸）旋转到 XOZ 平面（Unity 水平面）。
    /// 旋转矩阵：绕 X 轴 +90°  →  (x, y, z) → (x, -z, y)
    /// DXF 数据 z=0，所以效果等同于：DXF(x, y, 0) → Unity(x, 0, y)
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
    /// 将顶点平移，使 bounding box 中心对齐到原点 (0, 0, 0)
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

    private static float TryParseFloat(string s)
    {
        if (float.TryParse(s, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float result))
            return result;
        return 0f;
    }

    #endregion
}
