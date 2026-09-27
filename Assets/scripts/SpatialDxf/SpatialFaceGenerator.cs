using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Recovers triangular rigid panels from a spatial, undirected crease graph.
///
/// Vertex and edge indices accepted by <see cref="Generate"/> are zero-based.
/// Faces returned in <see cref="SpatialFaceGenerationResult.faces"/> use the project's
/// existing one-based OrigamiFace convention.
///
/// This intentionally does not try to infer mountain/valley direction.  M/V/B is a
/// property of the edge, whereas this class only recovers the rigid triangular panels
/// bounded by those edges.
/// </summary>
public static class SpatialFaceGenerator
{
    /// <summary>
    /// Finds every minimal non-degenerate three-edge loop.  A loop is rejected when a
    /// different input vertex lies in its plane and inside it (or on one of its edges),
    /// because that would make it a larger triangle composed of smaller panels.
    ///
    /// The returned winding is made locally consistent: two accepted faces sharing an
    /// edge traverse that edge in opposite directions.  Each disconnected component can
    /// still be globally flipped, which is unavoidable for an open surface.
    /// </summary>
    /// <param name="vertices">3D positions, indexed from zero.</param>
    /// <param name="edges">Undirected topology edges, whose v1/v2 fields are zero-based.</param>
    /// <param name="minimumTriangleArea">Smallest allowed geometric triangle area, in Unity units squared.</param>
    public static SpatialFaceGenerationResult Generate(
        List<Vector3> vertices,
        List<SpatialDxfIndexedEdge> edges,
        float minimumTriangleArea)
    {
        SpatialFaceGenerationResult result = new SpatialFaceGenerationResult();

        if (vertices == null)
        {
            result.warnings.Add("Face generation skipped because the vertex list is null.");
            return result;
        }

        if (edges == null)
        {
            result.warnings.Add("Face generation skipped because the edge list is null.");
            return result;
        }

        if (vertices.Count < 3)
        {
            result.warnings.Add("Face generation needs at least three vertices.");
            return result;
        }

        if (edges.Count < 3)
        {
            result.warnings.Add("Face generation needs at least three edges.");
            return result;
        }

        float safeMinimumArea = SanitizeNonNegative(minimumTriangleArea, 0.000001f);
        float coplanarDistanceTolerance = CalculateCoplanarDistanceTolerance(vertices);
        float barycentricTolerance = 0.0001f;
        float coincidenceTolerance = Mathf.Max(0.000001f, coplanarDistanceTolerance * 0.1f);

        bool[] validVertices = ValidateVertices(vertices, result);
        List<HashSet<int>> adjacency = CreateAdjacency(vertices.Count);
        List<EdgeKey> uniqueEdges = BuildUndirectedGraph(
            edges,
            validVertices,
            adjacency,
            result);

        if (uniqueEdges.Count < 3)
        {
            result.warnings.Add("Fewer than three valid unique edges remain after topology validation.");
            return result;
        }

        List<Triangle> triangles = FindMinimalTriangles(
            vertices,
            validVertices,
            adjacency,
            uniqueEdges,
            safeMinimumArea,
            coplanarDistanceTolerance,
            barycentricTolerance,
            coincidenceTolerance,
            result);

        if (triangles.Count == 0)
        {
            result.warnings.Add("No valid minimal triangular loops were found in the 3D edge graph.");
            return result;
        }

        triangles.Sort(CompareTriangles);
        int[] orientations = OrientFacesConsistently(triangles, result);

        for (int i = 0; i < triangles.Count; i++)
        {
            Triangle triangle = triangles[i];
            bool flipped = orientations[i] < 0;

            result.faces.Add(new global::OrigamiFace
            {
                id = i + 1,
                // OrigamiModel JSON uses one-based vertex ids.
                vertices = flipped
                    ? new List<int> { triangle.a + 1, triangle.c + 1, triangle.b + 1 }
                    : new List<int> { triangle.a + 1, triangle.b + 1, triangle.c + 1 },
                rigid = true
            });
        }

        return result;
    }

    private static bool[] ValidateVertices(List<Vector3> vertices, SpatialFaceGenerationResult result)
    {
        bool[] valid = new bool[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            valid[i] = IsFinite(vertices[i]);
            if (!valid[i])
                result.warnings.Add($"Vertex {i + 1} has a non-finite coordinate and was ignored.");
        }

        return valid;
    }

    private static List<HashSet<int>> CreateAdjacency(int vertexCount)
    {
        List<HashSet<int>> adjacency = new List<HashSet<int>>(vertexCount);
        for (int i = 0; i < vertexCount; i++)
            adjacency.Add(new HashSet<int>());
        return adjacency;
    }

    private static List<EdgeKey> BuildUndirectedGraph(
        List<SpatialDxfIndexedEdge> edges,
        bool[] validVertices,
        List<HashSet<int>> adjacency,
        SpatialFaceGenerationResult result)
    {
        HashSet<EdgeKey> unique = new HashSet<EdgeKey>();
        List<EdgeKey> output = new List<EdgeKey>();

        for (int i = 0; i < edges.Count; i++)
        {
            SpatialDxfIndexedEdge edge = edges[i];
            int vertexCount = validVertices.Length;

            if (edge.v1 < 0 || edge.v1 >= vertexCount || edge.v2 < 0 || edge.v2 >= vertexCount)
            {
                result.warnings.Add($"Edge {i + 1} references an out-of-range vertex and was ignored.");
                continue;
            }

            if (!validVertices[edge.v1] || !validVertices[edge.v2])
            {
                result.warnings.Add($"Edge {i + 1} touches a non-finite vertex and was ignored.");
                continue;
            }

            if (edge.v1 == edge.v2)
            {
                result.warnings.Add($"Edge {i + 1} has identical endpoints and was ignored.");
                continue;
            }

            EdgeKey key = new EdgeKey(edge.v1, edge.v2);
            if (!unique.Add(key))
            {
                result.warnings.Add($"Duplicate edge ({key.a + 1}, {key.b + 1}) was ignored while generating faces.");
                continue;
            }

            output.Add(key);
            adjacency[key.a].Add(key.b);
            adjacency[key.b].Add(key.a);
        }

        output.Sort(CompareEdges);
        return output;
    }

    private static List<Triangle> FindMinimalTriangles(
        List<Vector3> vertices,
        bool[] validVertices,
        List<HashSet<int>> adjacency,
        List<EdgeKey> edges,
        float minimumTriangleArea,
        float coplanarDistanceTolerance,
        float barycentricTolerance,
        float coincidenceTolerance,
        SpatialFaceGenerationResult result)
    {
        List<Triangle> output = new List<Triangle>();

        // For a sorted triangle a < b < c, it is discovered exactly once from edge (a, b).
        for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++)
        {
            EdgeKey edge = edges[edgeIndex];
            foreach (int c in adjacency[edge.a])
            {
                if (c <= edge.b || !adjacency[edge.b].Contains(c))
                    continue;

                Vector3 aPosition = vertices[edge.a];
                Vector3 bPosition = vertices[edge.b];
                Vector3 cPosition = vertices[c];
                float area = TriangleArea(aPosition, bPosition, cPosition);
                if (!IsFinite(area) || area <= minimumTriangleArea)
                {
                    result.warnings.Add(
                        $"Triangle ({edge.a + 1}, {edge.b + 1}, {c + 1}) is degenerate or below the minimum area and was ignored.");
                    continue;
                }

                int blockingVertex;
                bool blockingVertexOnEdge;
                if (TryFindCoplanarBlockingVertex(
                        vertices,
                        validVertices,
                        edge.a,
                        edge.b,
                        c,
                        coplanarDistanceTolerance,
                        barycentricTolerance,
                        coincidenceTolerance,
                        out blockingVertex,
                        out blockingVertexOnEdge))
                {
                    string location = blockingVertexOnEdge ? "on an edge" : "inside";
                    result.warnings.Add(
                        $"Triangle ({edge.a + 1}, {edge.b + 1}, {c + 1}) contains vertex {blockingVertex + 1} {location} in the same plane, so the larger loop was ignored.");
                    continue;
                }

                output.Add(new Triangle(edge.a, edge.b, c));
            }
        }

        return output;
    }

    private static bool TryFindCoplanarBlockingVertex(
        List<Vector3> vertices,
        bool[] validVertices,
        int aIndex,
        int bIndex,
        int cIndex,
        float planeTolerance,
        float barycentricTolerance,
        float coincidenceTolerance,
        out int blockingVertex,
        out bool blockingVertexOnEdge)
    {
        blockingVertex = -1;
        blockingVertexOnEdge = false;

        Vector3 a = vertices[aIndex];
        Vector3 b = vertices[bIndex];
        Vector3 c = vertices[cIndex];
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 normal = Vector3.Cross(ab, ac);
        float normalMagnitude = normal.magnitude;
        if (normalMagnitude <= Mathf.Epsilon)
            return false;

        Vector3 unitNormal = normal / normalMagnitude;
        float dot00 = Vector3.Dot(ab, ab);
        float dot01 = Vector3.Dot(ab, ac);
        float dot11 = Vector3.Dot(ac, ac);
        float denominator = dot00 * dot11 - dot01 * dot01;
        if (Mathf.Abs(denominator) <= Mathf.Epsilon)
            return false;

        float coincidenceSquared = coincidenceTolerance * coincidenceTolerance;
        for (int i = 0; i < vertices.Count; i++)
        {
            if (!validVertices[i] || i == aIndex || i == bIndex || i == cIndex)
                continue;

            Vector3 point = vertices[i];
            if (IsCoincident(point, a, coincidenceSquared) ||
                IsCoincident(point, b, coincidenceSquared) ||
                IsCoincident(point, c, coincidenceSquared))
            {
                // A separately-indexed coincident endpoint is a topology issue, but it is
                // not an interior vertex that should suppress an otherwise valid face.
                continue;
            }

            Vector3 fromA = point - a;
            if (Mathf.Abs(Vector3.Dot(fromA, unitNormal)) > planeTolerance)
                continue;

            float dot20 = Vector3.Dot(fromA, ab);
            float dot21 = Vector3.Dot(fromA, ac);
            float bWeight = (dot11 * dot20 - dot01 * dot21) / denominator;
            float cWeight = (dot00 * dot21 - dot01 * dot20) / denominator;
            float aWeight = 1f - bWeight - cWeight;

            if (aWeight < -barycentricTolerance ||
                bWeight < -barycentricTolerance ||
                cWeight < -barycentricTolerance ||
                aWeight > 1f + barycentricTolerance ||
                bWeight > 1f + barycentricTolerance ||
                cWeight > 1f + barycentricTolerance)
            {
                continue;
            }

            blockingVertex = i;
            blockingVertexOnEdge = !(aWeight > barycentricTolerance &&
                                      bWeight > barycentricTolerance &&
                                      cWeight > barycentricTolerance);
            return true;
        }

        return false;
    }

    private static int[] OrientFacesConsistently(List<Triangle> triangles, SpatialFaceGenerationResult result)
    {
        Dictionary<EdgeKey, List<int>> facesByEdge = new Dictionary<EdgeKey, List<int>>();
        for (int i = 0; i < triangles.Count; i++)
        {
            Triangle triangle = triangles[i];
            RegisterFaceEdge(facesByEdge, new EdgeKey(triangle.a, triangle.b), i);
            RegisterFaceEdge(facesByEdge, new EdgeKey(triangle.b, triangle.c), i);
            RegisterFaceEdge(facesByEdge, new EdgeKey(triangle.c, triangle.a), i);
        }

        foreach (KeyValuePair<EdgeKey, List<int>> entry in facesByEdge)
        {
            if (entry.Value.Count > 2)
            {
                result.warnings.Add(
                    $"Edge ({entry.Key.a + 1}, {entry.Key.b + 1}) belongs to {entry.Value.Count} recovered faces; the surface is non-manifold and winding may be ambiguous.");
            }
        }

        int[] orientations = new int[triangles.Count];
        HashSet<long> reportedConflicts = new HashSet<long>();
        Queue<int> pending = new Queue<int>();

        for (int root = 0; root < triangles.Count; root++)
        {
            if (orientations[root] != 0)
                continue;

            orientations[root] = 1;
            pending.Enqueue(root);

            while (pending.Count > 0)
            {
                int current = pending.Dequeue();
                Triangle triangle = triangles[current];
                OrientAcrossEdge(
                    current,
                    new EdgeKey(triangle.a, triangle.b),
                    triangles,
                    facesByEdge,
                    orientations,
                    pending,
                    reportedConflicts,
                    result);
                OrientAcrossEdge(
                    current,
                    new EdgeKey(triangle.b, triangle.c),
                    triangles,
                    facesByEdge,
                    orientations,
                    pending,
                    reportedConflicts,
                    result);
                OrientAcrossEdge(
                    current,
                    new EdgeKey(triangle.c, triangle.a),
                    triangles,
                    facesByEdge,
                    orientations,
                    pending,
                    reportedConflicts,
                    result);
            }
        }

        return orientations;
    }

    private static void OrientAcrossEdge(
        int currentFace,
        EdgeKey sharedEdge,
        List<Triangle> triangles,
        Dictionary<EdgeKey, List<int>> facesByEdge,
        int[] orientations,
        Queue<int> pending,
        HashSet<long> reportedConflicts,
        SpatialFaceGenerationResult result)
    {
        List<int> neighbours;
        if (!facesByEdge.TryGetValue(sharedEdge, out neighbours))
            return;

        int currentDirection = GetEdgeDirection(triangles[currentFace], sharedEdge, orientations[currentFace]);
        for (int i = 0; i < neighbours.Count; i++)
        {
            int neighbour = neighbours[i];
            if (neighbour == currentFace)
                continue;

            int neighbourBaseDirection = GetEdgeDirection(triangles[neighbour], sharedEdge, 1);
            int neededOrientation = neighbourBaseDirection == -currentDirection ? 1 : -1;

            if (orientations[neighbour] == 0)
            {
                orientations[neighbour] = neededOrientation;
                pending.Enqueue(neighbour);
                continue;
            }

            if (orientations[neighbour] != neededOrientation)
            {
                long pairKey = MakeFacePairKey(currentFace, neighbour);
                if (reportedConflicts.Add(pairKey))
                {
                    result.warnings.Add(
                        $"Recovered faces {currentFace + 1} and {neighbour + 1} impose conflicting winding across edge ({sharedEdge.a + 1}, {sharedEdge.b + 1}).");
                }
            }
        }
    }

    private static int GetEdgeDirection(Triangle triangle, EdgeKey edge, int orientation)
    {
        int first = triangle.a;
        int second = orientation >= 0 ? triangle.b : triangle.c;
        int third = orientation >= 0 ? triangle.c : triangle.b;

        if (first == edge.a && second == edge.b ||
            second == edge.a && third == edge.b ||
            third == edge.a && first == edge.b)
        {
            return 1;
        }

        return -1;
    }

    private static void RegisterFaceEdge(Dictionary<EdgeKey, List<int>> facesByEdge, EdgeKey edge, int faceIndex)
    {
        List<int> faces;
        if (!facesByEdge.TryGetValue(edge, out faces))
        {
            faces = new List<int>();
            facesByEdge.Add(edge, faces);
        }

        faces.Add(faceIndex);
    }

    private static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(b - a, c - a).magnitude * 0.5f;
    }

    private static float CalculateCoplanarDistanceTolerance(List<Vector3> vertices)
    {
        // The absolute floor handles tiny models.  The scale term avoids treating numerical
        // round-off in a large Rhino model as a genuinely non-coplanar vertex.
        Vector3 min = vertices[0];
        Vector3 max = vertices[0];
        bool hasFiniteVertex = false;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vertex = vertices[i];
            if (!IsFinite(vertex))
                continue;

            if (!hasFiniteVertex)
            {
                min = vertex;
                max = vertex;
                hasFiniteVertex = true;
                continue;
            }

            min = Vector3.Min(min, vertex);
            max = Vector3.Max(max, vertex);
        }

        if (!hasFiniteVertex)
            return 0.00001f;

        float scale = (max - min).magnitude;
        return Mathf.Max(0.00001f, scale * 0.000001f);
    }

    private static bool IsCoincident(Vector3 left, Vector3 right, float squaredTolerance)
    {
        return (left - right).sqrMagnitude <= squaredTolerance;
    }

    private static bool IsFinite(Vector3 value)
    {
        return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static float SanitizeNonNegative(float value, float fallback)
    {
        return IsFinite(value) && value >= 0f ? value : fallback;
    }

    private static int CompareEdges(EdgeKey left, EdgeKey right)
    {
        int byA = left.a.CompareTo(right.a);
        return byA != 0 ? byA : left.b.CompareTo(right.b);
    }

    private static int CompareTriangles(Triangle left, Triangle right)
    {
        int byA = left.a.CompareTo(right.a);
        if (byA != 0) return byA;
        int byB = left.b.CompareTo(right.b);
        return byB != 0 ? byB : left.c.CompareTo(right.c);
    }

    private static long MakeFacePairKey(int first, int second)
    {
        int low = Math.Min(first, second);
        int high = Math.Max(first, second);
        return ((long)low << 32) ^ (uint)high;
    }

    private struct Triangle
    {
        public readonly int a;
        public readonly int b;
        public readonly int c;

        public Triangle(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    private struct EdgeKey : IEquatable<EdgeKey>
    {
        public readonly int a;
        public readonly int b;

        public EdgeKey(int first, int second)
        {
            if (first < second)
            {
                a = first;
                b = second;
            }
            else
            {
                a = second;
                b = first;
            }
        }

        public bool Equals(EdgeKey other)
        {
            return a == other.a && b == other.b;
        }

        public override bool Equals(object obj)
        {
            return obj is EdgeKey && Equals((EdgeKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return a * 486187739 ^ b;
            }
        }
    }
}

/// <summary>
/// Diagnostics returned by <see cref="SpatialFaceGenerator"/>.  Faces use one-based
/// OrigamiFace vertex ids; warning strings also use one-based ids when they name vertices.
/// </summary>
public sealed class SpatialFaceGenerationResult
{
    public readonly List<global::OrigamiFace> faces = new List<global::OrigamiFace>();
    public readonly List<string> warnings = new List<string>();
}
