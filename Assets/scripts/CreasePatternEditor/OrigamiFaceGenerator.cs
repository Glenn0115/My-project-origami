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
        public OrigamiCrease crease; // Store the associated crease

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

            // 下一条面边是：环绕点逆时针的上一条
            int nextIdx = (idx - 1 + list.Count) % list.Count;

            h.next = list[nextIdx];
        }

        // ---- Step 4: 面遍历 Face-Walk ----
        List<OrigamiFace> faces = new();

        foreach (var h in halfEdges)
        {
            if (h.visited) continue;

            List<int> face = new();
            HalfEdge cur = h;

            while (!cur.visited)
            {
                cur.visited = true;
                face.Add(cur.from);
                cur = cur.next;
            }

            // 去除重复 & 共线检查
            if (face.Count >= 3)
            {
                // 转成 1-based
                faces.Add(new OrigamiFace
                {
                    vertices = face.Select(v => v + 1).ToList()
                });
            }
        }

        if (faces.Count > 0 && creases != null)
        {
            // Step 1: 收集所有边界顶点（1-based）
            HashSet<int> boundaryVertices = new HashSet<int>();
            foreach (var c in creases)
            {
                if (c.type == OrigamiCrease.Type.Boundary)
                {
                    boundaryVertices.Add(c.v1);
                    boundaryVertices.Add(c.v2);
                }
            }

            // Step 2: 如果没有边界顶点，直接返回（避免空集错误）
            if (boundaryVertices.Count == 0)
                return faces;

            // Step 3: 删除顶点集合等于边界顶点集合的面（即外部面）
            faces = faces.Where(face =>
                !boundaryVertices.SetEquals(face.vertices.ToHashSet())
            ).ToList();

            Debug.Log($"✅ 删除外部面: 顶点集合匹配 {boundaryVertices.Count} 个边界顶点");
        }

        for (int i = 0; i < faces.Count; i++)
            faces[i].id = i + 1;

        return faces;
    }
}
