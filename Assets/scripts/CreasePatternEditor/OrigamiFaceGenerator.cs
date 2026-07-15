using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 改进版的面生成器。
///
/// 相比原版 OrigamiFaceGenerator 的修复:
/// 1. 悬挂边(Dangling Edge)过滤 —— degree=1 的顶点不作为合法面的边
/// 2. 完整的回溯清理 —— 处理任意距离的重复顶点, 而非仅 [A,B,A] 三元组
/// 3. 共线边稳定排序 —— 同角度时用边长作为二级排序键
/// 4. 面边验证 —— 确保生成面的每条边在折痕集中存在
/// </summary>
public static class OrigamiFaceGeneratorV2
{
    class HalfEdge
    {
        public int from;
        public int to;
        public HalfEdge twin;
        public HalfEdge next;
        public float angle;
        public float length; // 二级排序: 边长
        public bool visited;
    }

    public static List<OrigamiFace> GenerateFaces(
        List<OrigamiVertex> vertices,
        List<OrigamiCrease> creases)
    {
        if (vertices.Count == 0 || creases.Count == 0)
            return new List<OrigamiFace>();

        int n = vertices.Count;

        // ---- Step 1: 计算每个顶点的度(degree) ----
        int[] degree = new int[n];
        foreach (var c in creases)
        {
            int a = c.v1 - 1;
            int b = c.v2 - 1;
            if (a < 0 || b < 0 || a >= n || b >= n) continue;
            degree[a]++;
            degree[b]++;
        }

        // ---- Step 2: 建立半边 ----
        List<HalfEdge> halfEdges = new List<HalfEdge>();
        Dictionary<(int, int), HalfEdge> edgeMap = new();

        foreach (var c in creases)
        {
            int a = c.v1 - 1;
            int b = c.v2 - 1;
            if (a < 0 || b < 0 || a >= n || b >= n) continue;

            Vector3 pa = new Vector3(vertices[a].x, vertices[a].z, 0);
            Vector3 pb = new Vector3(vertices[b].x, vertices[b].z, 0);
            float len = Vector3.Distance(pa, pb);

            HalfEdge h1 = new HalfEdge { from = a, to = b, length = len };
            HalfEdge h2 = new HalfEdge { from = b, to = a, length = len };

            h1.twin = h2;
            h2.twin = h1;

            edgeMap[(a, b)] = h1;
            edgeMap[(b, a)] = h2;

            halfEdges.Add(h1);
            halfEdges.Add(h2);
        }

        // ---- Step 3: 按角度排序每个顶点的出边 ----
        // 使用稳定排序: 先按角度, 同角度时按边长 (短边优先, 处理共线悬挂边)
        Dictionary<int, List<HalfEdge>> star = new();

        foreach (var h in halfEdges)
        {
            if (!star.ContainsKey(h.from))
                star[h.from] = new List<HalfEdge>();

            Vector3 p = new Vector3(vertices[h.from].x, vertices[h.from].z, 0);
            Vector3 q = new Vector3(vertices[h.to].x, vertices[h.to].z, 0);
            Vector3 v = q - p;

            h.angle = Mathf.Atan2(v.y, v.x);
            star[h.from].Add(h);
        }

        foreach (var kv in star)
        {
            kv.Value.Sort((a, b) =>
            {
                int cmp = a.angle.CompareTo(b.angle);
                if (cmp != 0) return cmp;
                // 同角度: 短边优先 (悬挂边通常较短)
                return a.length.CompareTo(b.length);
            });
        }

        // ---- Step 4: 为每条半边计算 next (跳过悬挂边) ----
        foreach (var h in halfEdges)
        {
            var list = star[h.to];
            int idx = list.IndexOf(h.twin);

            // 从 twin 的下一个开始 (CCW 方向), 跳过通向 degree=1 顶点的边
            int nextIdx = -1;
            for (int offset = 1; offset <= list.Count; offset++)
            {
                int candidateIdx = (idx + offset) % list.Count;
                int destVertex = list[candidateIdx].to;

                // 跳过悬挂边 (到达 degree=1 的顶点)
                if (degree[destVertex] <= 1)
                    continue;

                nextIdx = candidateIdx;
                break;
            }

            if (nextIdx >= 0)
            {
                h.next = list[nextIdx];
            }
            else
            {
                // 所有边都是悬挂边 — 回退到 twin (面追踪时会形成回溯, 后续清理)
                h.next = h.twin;
            }
        }

        // ---- Step 5: 面遍历 Face-Walk ----
        List<OrigamiFace> faces = new();

        foreach (var h in halfEdges)
        {
            if (h.visited) continue;

            List<int> rawFace = new();
            HalfEdge cur = h;

            while (!cur.visited)
            {
                cur.visited = true;
                rawFace.Add(cur.from);
                cur = cur.next;
            }

            // 清理回溯
            List<int> cleaned = CleanupFace(rawFace);

            if (cleaned.Count >= 3 && IsValidFace(cleaned, edgeMap, n))
            {
                faces.Add(new OrigamiFace
                {
                    vertices = cleaned.Select(v => v + 1).ToList() // 转回 1-based
                });
            }
        }

        // ---- Step 6: 移除外侧面 ----
        if (faces.Count > 1)
        {
            faces = RemoveExteriorFace(faces, creases, n);
        }

        // ---- Step 7: 重新编号 ----
        for (int i = 0; i < faces.Count; i++)
            faces[i].id = i + 1;

        return faces;
    }

    /// <summary>
    /// 通用回溯清理: 检测任意位置的重复顶点, 移除中间的无效路径。
    /// 例如 [1,2,17,1,57] → 找到重复的 1 在位置 0 和 3 → 移除 [2,17,1] 保留 [1,57] (2顶点丢弃)
    /// 例如 [6,5,41,5,14,13] → 找到重复的 5 在位置 1 和 3 → 移除 [41,5] → [6,5,14,13]
    /// </summary>
    private static List<int> CleanupFace(List<int> raw)
    {
        var result = new List<int>(raw);

        bool changed = true;
        int maxIterations = result.Count; // 防止无限循环

        while (changed && maxIterations-- > 0)
        {
            changed = false;

            // 找第一对重复顶点
            for (int i = 0; i < result.Count; i++)
            {
                for (int j = i + 1; j < result.Count; j++)
                {
                    if (result[i] == result[j])
                    {
                        // 找到了从 i 到 j 的回路 (不包含 j)
                        // 如果回路长度 >= 3, 保留回路
                        int cycleLength = j - i;
                        if (cycleLength >= 3)
                        {
                            // 提取回路 result[i..j-1] 作为面
                            result = result.GetRange(i, cycleLength);
                        }
                        else
                        {
                            // 回路太短 (< 3), 删除这段回溯
                            // 移除 i+1 到 j (包含)
                            result.RemoveRange(i + 1, j - i);
                        }
                        changed = true;
                        break;
                    }
                }
                if (changed) break;
            }
        }

        return result;
    }

    /// <summary>
    /// 验证面是否合法:
    /// 1. 无重复顶点
    /// 2. 每条边在折痕集中存在
    /// 3. 顶点数 >= 3
    /// </summary>
    private static bool IsValidFace(
        List<int> verts,
        Dictionary<(int, int), HalfEdge> edgeMap,
        int vertexCount)
    {
        if (verts.Count < 3) return false;

        // 检查无重复
        if (verts.Distinct().Count() != verts.Count) return false;

        // 检查每条边存在
        for (int i = 0; i < verts.Count; i++)
        {
            int a = verts[i];
            int b = verts[(i + 1) % verts.Count];

            if (a == b) return false;
            if (!edgeMap.ContainsKey((a, b)) && !edgeMap.ContainsKey((b, a)))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 移除外侧面。
    /// 策略 1: Boundary 顶点集精确匹配
    /// 策略 2: 顶点数最多的面 = 外侧面 (仅当只有一个候选时)
    /// </summary>
    private static List<OrigamiFace> RemoveExteriorFace(
        List<OrigamiFace> faces,
        List<OrigamiCrease> creases,
        int vertexCount)
    {
        OrigamiFace exterior = null;

        // 策略 1: Boundary 顶点集精确匹配
        HashSet<int> boundaryVerts = new HashSet<int>();
        foreach (var c in creases)
        {
            if (c.type == OrigamiCrease.Type.Boundary)
            {
                if (c.v1 >= 1 && c.v1 <= vertexCount) boundaryVerts.Add(c.v1);
                if (c.v2 >= 1 && c.v2 <= vertexCount) boundaryVerts.Add(c.v2);
            }
        }

        if (boundaryVerts.Count > 0)
        {
            foreach (var f in faces)
            {
                if (boundaryVerts.SetEquals(f.vertices.ToHashSet()))
                {
                    exterior = f;
                    break;
                }
            }
        }

        // 策略 2: 回退 — 顶点数最多的面
        if (exterior == null && faces.Count > 1)
        {
            int maxVerts = faces.Max(f => f.vertices.Count);
            var candidates = faces.Where(f => f.vertices.Count == maxVerts).ToList();

            // 仅当唯一候选时才移除 (多个同大小则不明确)
            if (candidates.Count == 1)
            {
                exterior = candidates[0];
                Debug.Log($"[FaceGenV2] 回退策略: 按顶点数({exterior.vertices.Count})移除外侧面");
            }
            else
            {
                Debug.LogWarning($"[FaceGenV2] 无法确定外侧面: {candidates.Count} 个面均有 {maxVerts} 个顶点, 跳过移除");
            }
        }

        if (exterior != null)
        {
            faces.Remove(exterior);
            Debug.Log($"[FaceGenV2] 移除外侧面(顶点数={exterior.vertices.Count}), 剩余={faces.Count}");
        }

        return faces;
    }
}
