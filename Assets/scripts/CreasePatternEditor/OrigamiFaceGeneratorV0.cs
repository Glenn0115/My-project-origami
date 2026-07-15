using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class OrigamiFaceGenerator
{
    class HalfEdge
    {
        public int from;
        public int to;
        public HalfEdge twin;
        public HalfEdge next;
        public float angle;
        public bool visited = false;
        public OrigamiCrease crease;

    }

    public static List<OrigamiFace> GenerateFaces(
        List<OrigamiVertex> vertices,
        List<OrigamiCrease> creases)
    {
        if (vertices.Count == 0 || creases.Count == 0)
            return new List<OrigamiFace>();

        // ---- Step 1: 建立半边 ----
        List<HalfEdge> halfEdges = new List<HalfEdge>();
        Dictionary<(int, int), HalfEdge> edgeMap = new();

        foreach (var c in creases)
        {
            int a = c.v1 - 1;
            int b = c.v2 - 1;
            if (a < 0 || b < 0 || a >= vertices.Count || b >= vertices.Count) continue;

            HalfEdge h1 = new HalfEdge { from = a, to = b };
            HalfEdge h2 = new HalfEdge { from = b, to = a };

            h1.twin = h2;
            h2.twin = h1;

            edgeMap[(a, b)] = h1;
            edgeMap[(b, a)] = h2;

            halfEdges.Add(h1);
            halfEdges.Add(h2);
        }

        // ---- Step 2: 按角度排序每个顶点的半边 ----
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
            kv.Value.Sort((a, b) => a.angle.CompareTo(b.angle));

        // ---- Step 3: 为每条半边找到 next ----
        foreach (var h in halfEdges)
        {
            var list = star[h.to];
            int idx = list.IndexOf(h.twin);

            int nextIdx = (idx - 1 + list.Count) % list.Count;

            h.next = list[nextIdx];
        }

        // ---- Step 4: 面遍历 Face-Walk ----
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

            if (rawFace.Count >= 3)
            {
                // 清理回溯顶点：...A, B, A... → ...A...
                List<int> cleaned = CollapseBacktrackVertices(rawFace);

                if (cleaned.Count >= 3)
                {
                    faces.Add(new OrigamiFace
                    {
                        vertices = cleaned.Select(v => v + 1).ToList()
                    });
                }
            }
        }

        // ---- Step 5: 移除外侧面 ----
        if (faces.Count > 1)
        {
            faces = RemoveExteriorFace(faces, creases);
        }

        for (int i = 0; i < faces.Count; i++)
            faces[i].id = i + 1;

        return faces;
    }

    /// <summary>
    /// 清理悬挂边导致的回溯顶点，例如 [6, 5, 41, 5, 14, 13] → [6, 5, 14, 13]
    /// 循环执行直到无变化（处理多层回溯）
    /// </summary>
    private static List<int> CollapseBacktrackVertices(List<int> verts)
    {
        var result = new List<int>(verts);

        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int i = 0; i < result.Count - 2; i++)
            {
                if (result[i] == result[i + 2] && result[i] != result[i + 1])
                {
                    result.RemoveAt(i + 2);
                    result.RemoveAt(i + 1);
                    changed = true;
                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 移除外侧面。
    /// 1. 优先用 Boundary 顶点集匹配法
    /// 2. 回退：顶点数最多的面 = 外侧面（全 Boundary / 零 Boundary 都适用）
    /// </summary>
    private static List<OrigamiFace> RemoveExteriorFace(
        List<OrigamiFace> faces, List<OrigamiCrease> creases)
    {
        OrigamiFace exterior = null;

        // 策略 1: Boundary 顶点集精确匹配
        HashSet<int> boundaryVerts = new HashSet<int>();
        foreach (var c in creases)
        {
            if (c.type == OrigamiCrease.Type.Boundary)
            {
                boundaryVerts.Add(c.v1);
                boundaryVerts.Add(c.v2);
            }
        }

        if (boundaryVerts.Count > 0)
        {
            exterior = faces.FirstOrDefault(f =>
                boundaryVerts.SetEquals(f.vertices.ToHashSet()));
        }

        // 策略 2: 回退 — 顶点数最多的面即为外侧面
        if (exterior == null && faces.Count > 1)
        {
            exterior = faces.OrderByDescending(f => f.vertices.Count).First();
            Debug.Log($"[FaceGen] 回退策略: 按顶点数({exterior.vertices.Count})移除外侧面");
        }

        if (exterior != null)
        {
            faces.Remove(exterior);
            Debug.Log($"[FaceGen] 移除外侧面(顶点数={exterior.vertices.Count}), 剩余={faces.Count}");
        }

        return faces;
    }
}
