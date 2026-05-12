using System.Collections.Generic;
using UnityEngine;

public class HalfEdgeMesh
{
    public class HalfEdge
    {
        public int vertexIndex;
        public HalfEdge next;
        public HalfEdge twin;
        public int faceIndex = -1;
    }

    private List<OrigamiVertex> _vertices;
    private List<OrigamiCrease> _creases;

    public Dictionary<int, HalfEdge> halfEdges { get; private set; }
    public List<List<int>> faceVertexLists { get; private set; }

    public HalfEdgeMesh(List<OrigamiVertex> vertices, List<OrigamiCrease> creases)
    {
        _vertices = vertices;
        _creases = creases;
        halfEdges = new Dictionary<int, HalfEdge>();
        faceVertexLists = new List<List<int>>();
    }

    public bool Rebuild()
    {
        halfEdges.Clear();
        faceVertexLists.Clear();

        foreach (var crease in _creases)
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
            kvp.Value.Sort((a, b) => CompareEdgesByAngle(a, b, vertexPos));
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
                faceVertices.Add(current.vertexIndex + 1); // 存储为1-based
                current.faceIndex = currentFaceIndex;
                current = current.next;
            } while (current != he && current != null && faceVertices.Count < 100);

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

    private int CompareEdgesByAngle(HalfEdge a, HalfEdge b, Vector3 origin)
    {
        Vector3 dirA = GetVertexPosition(a.twin.vertexIndex) - origin;
        Vector3 dirB = GetVertexPosition(b.twin.vertexIndex) - origin;

        float angleA = Mathf.Atan2(dirA.z, dirA.x);
        float angleB = Mathf.Atan2(dirB.z, dirB.x);

        return angleA.CompareTo(angleB);
    }

    private Vector3 GetVertexPosition(int vertexIndex)
    {
        var v = _vertices[vertexIndex];
        return new Vector3(v.x, v.y, v.z);
    }

    public int GetHalfEdgeKey(int start, int end)
    {
        return (start << 16) | end;
    }

    public HalfEdge FindNearestHalfEdge(Vector3 clickPos)
    {
        float minDist = float.MaxValue;
        HalfEdge nearest = null;

        // 使用一个HashSet来避免处理成对的半边
        HashSet<int> processedKeys = new HashSet<int>();

        foreach (var kvp in halfEdges)
        {
            int key = kvp.Key;
            if (processedKeys.Contains(key)) continue;

            HalfEdge he = kvp.Value;
            int twinKey = GetHalfEdgeKey(he.twin.vertexIndex, he.vertexIndex);
            processedKeys.Add(key);
            processedKeys.Add(twinKey);

            Vector3 start = GetVertexPosition(he.vertexIndex);
            Vector3 end = GetVertexPosition(he.twin.vertexIndex);

            Vector3 closest = GeometryUtils.ClosestPointOnSegment(start, end, clickPos);
            float dist = Vector3.Distance(clickPos, closest);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = he;
            }
        }
        return minDist < 5f ? nearest : null;
    }
}