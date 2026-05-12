using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FaceFinder
{
    // 半边数据结构
    public class HalfEdge
    {
        public int vertexIndex;       // 起点顶点索引
        public HalfEdge next;         // 下一条半边
        public HalfEdge twin;         // 对边（反向半边）
        public int faceIndex = -1;    // 所属面索引（-1表示未分配）
        public Vector3 position;      // 用于射线检测的3D位置
    }

    private Dictionary<int, HalfEdge> halfEdges = new Dictionary<int, HalfEdge>();
    private List<List<int>> faceVertexLists = new List<List<int>>(); // 缓存所有面的顶点列表

    private List<OrigamiVertex> vertices;
    private List<OrigamiCrease> creases;

    public FaceFinder(List<OrigamiVertex> vertices, List<OrigamiCrease> creases)
    {
        this.vertices = vertices;
        this.creases = creases;
    }

    public List<int> FindFace(Vector3 clickPos)
    {
        if (!RebuildHalfEdgeStructure())
        {
            Debug.LogWarning("⚠️ 无法构建面拓扑（检查折痕是否形成封闭区域）");
            return null;
        }

        HalfEdge nearestEdge = FindNearestHalfEdge(clickPos);
        if (nearestEdge == null)
        {
            Debug.LogWarning("⚠️ 未找到附近折痕，无法标记面");
            return null;
        }

        int faceIndex = GetFaceIndex(nearestEdge);
        if (faceIndex < 0)
        {
            Debug.LogWarning("⚠️ 未找到有效面（折痕可能未闭合）");
            return null;
        }

        List<int> faceVertices = faceVertexLists[faceIndex];
        if (faceVertices.Count < 3)
        {
            Debug.LogWarning("⚠️ 无效面（顶点数不足）");
            return null;
        }

        return faceVertices;
    }

    private bool RebuildHalfEdgeStructure()
    {
        halfEdges.Clear();
        faceVertexLists.Clear();

        foreach (var crease in creases)
        {
            int v1 = crease.v1 - 1;
            int v2 = crease.v2 - 1;

            HalfEdge he1 = new HalfEdge { vertexIndex = v1 };
            HalfEdge he2 = new HalfEdge { vertexIndex = v2 };

            he1.twin = he2;
            he2.twin = he1;

            halfEdges[GetHalfEdgeKey(v1, v2)] = he1;
            halfEdges[GetHalfEdgeKey(v2, v1)] = he2;
        }

        var vertexOutEdges = new Dictionary<int, List<HalfEdge>>();
        foreach (var kvp in halfEdges)
        {
            int startVertex = kvp.Value.vertexIndex;
            if (!vertexOutEdges.ContainsKey(startVertex))
                vertexOutEdges[startVertex] = new List<HalfEdge>();
            vertexOutEdges[startVertex].Add(kvp.Value);
        }

        foreach (var kvp in vertexOutEdges)
        {
            int vertexIdx = kvp.Key;
            Vector3 vertexPos = GetVertexPosition(vertexIdx);

            kvp.Value.Sort((a, b) =>
            {
                Vector3 dirA = (GetVertexPosition(a.twin.vertexIndex) - vertexPos).normalized;
                Vector3 dirB = (GetVertexPosition(b.twin.vertexIndex) - vertexPos).normalized;

                float angleA = Mathf.Atan2(dirA.z, dirA.x);
                float angleB = Mathf.Atan2(dirB.z, dirB.x);

                return angleA.CompareTo(angleB);
            });
        }

        foreach (var kvp in vertexOutEdges)
        {
            List<HalfEdge> edges = kvp.Value;
            for (int i = 0; i < edges.Count; i++)
            {
                HalfEdge current = edges[i];
                HalfEdge nextEdge = edges[(i + 1) % edges.Count];
                current.next = nextEdge.twin;
            }
        }

        int currentFaceIndex = 0;
        foreach (var he in halfEdges.Values)
        {
            if (he.faceIndex >= 0) continue;

            List<int> faceVertices = new List<int>();
            HalfEdge current = he;

            do
            {
                faceVertices.Add(current.vertexIndex + 1);
                current.faceIndex = currentFaceIndex;
                current = current.next;
            }
            while (current != he && current != null && faceVertices.Count < 100);

            if (current != he || faceVertices.Count < 3)
            {
                current = he;
                do
                {
                    current.faceIndex = -1;
                    current = current.next;
                } while (current != he && current != null);
                continue;
            }

            faceVertexLists.Add(faceVertices);
            currentFaceIndex++;
        }

        return faceVertexLists.Count > 0;
    }

    private int GetHalfEdgeKey(int start, int end)
    {
        return (start << 16) | end;
    }

    private HalfEdge FindNearestHalfEdge(Vector3 clickPos)
    {
        float minDist = float.MaxValue;
        HalfEdge nearest = null;

        foreach (var kvp in halfEdges)
        {
            if (kvp.Key % 2 != 0) continue;

            HalfEdge he = kvp.Value;
            Vector3 start = GetVertexPosition(he.vertexIndex);
            Vector3 end = GetVertexPosition(he.twin.vertexIndex);

            Vector3 closest = ClosestPointOnSegment(start, end, clickPos);
            float dist = Vector3.Distance(clickPos, closest);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = he;
            }
        }

        return minDist < 5f ? nearest : null;
    }

    private Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }

    private int GetFaceIndex(HalfEdge edge)
    {
        if (edge.faceIndex >= 0)
            return edge.faceIndex;

        if (edge.twin != null && edge.twin.faceIndex >= 0)
            return edge.twin.faceIndex;

        return -1;
    }

    private Vector3 GetVertexPosition(int vertexIndex)
    {
        if (vertexIndex < 0 || vertexIndex >= vertices.Count)
            return Vector3.zero;

        var v = vertices[vertexIndex];
        return new Vector3(v.x, v.y, v.z);
    }
}