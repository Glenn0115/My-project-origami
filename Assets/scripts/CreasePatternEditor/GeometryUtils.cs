using UnityEngine;
using System.Collections.Generic;

public static class GeometryUtils
{
    public static Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }

    public static bool IsPointInPolygon(Vector2[] polygon, Vector2 point)
    {
        bool inside = false;
        int n = polygon.Length;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if (IsPointOnLineSegment(polygon[i], polygon[j], point))
                return true;

            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) /
                (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    public static bool IsPointOnLineSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        float cross = (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
        if (Mathf.Abs(cross) > 0.01f)
            return false;

        float dot = (p.x - a.x) * (p.x - b.x) + (p.y - a.y) * (p.y - b.y);
        return dot <= 0.01f;
    }

    public static bool IsPointOnSegment(Vector2 a, Vector2 b, Vector2 c)
    {
        return c.x >= Mathf.Min(a.x, b.x) - 0.01f && c.x <= Mathf.Max(a.x, b.x) + 0.01f &&
               c.y >= Mathf.Min(a.y, b.y) - 0.01f && c.y <= Mathf.Max(a.y, b.y) + 0.01f;
    }

    public static bool DoSegmentsIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Vector2 a = new Vector2(p1.x, p1.z);
        Vector2 b = new Vector2(p2.x, p2.z);
        Vector2 c = new Vector2(p3.x, p3.z);
        Vector2 d = new Vector2(p4.x, p4.z);

        float Cross(Vector2 o, Vector2 a, Vector2 b) => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);

        if (Mathf.Max(a.x, b.x) < Mathf.Min(c.x, d.x) - 0.01f ||
            Mathf.Max(c.x, d.x) < Mathf.Min(a.x, b.x) - 0.01f ||
            Mathf.Max(a.y, b.y) < Mathf.Min(c.y, d.y) - 0.01f ||
            Mathf.Max(c.y, d.y) < Mathf.Min(a.y, b.y) - 0.01f)
            return false;

        float d1 = Cross(a, b, c);
        float d2 = Cross(a, b, d);
        float d3 = Cross(c, d, a);
        float d4 = Cross(c, d, b);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        if (d1 == 0 && IsPointOnSegment(a, b, c)) return true;
        if (d2 == 0 && IsPointOnSegment(a, b, d)) return true;
        if (d3 == 0 && IsPointOnSegment(c, d, a)) return true;
        if (d4 == 0 && IsPointOnSegment(c, d, b)) return true;

        return false;
    }

    public static float CalculateTurningAngle(Vector3 prevPos, Vector3 currentPos, Vector3 nextPos)
    {
        Vector3 incoming = (currentPos - prevPos).normalized;
        Vector3 outgoing = (nextPos - currentPos).normalized;
        return Vector3.Angle(incoming, outgoing);
    }

    public static bool IsClockwiseTurn(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector2 a2 = new Vector2(a.x, a.z);
        Vector2 b2 = new Vector2(b.x, b.z);
        Vector2 c2 = new Vector2(c.x, c.z);

        Vector2 ab = b2 - a2;
        Vector2 bc = c2 - b2;
        float cross = ab.x * bc.y - ab.y * bc.x;

        return cross < 0;
    }

    public static bool IsSimpleCycle(List<int> cycle, List<OrigamiVertex> vertices)
    {
        if (cycle.Count < 3) return false;

        var vertexSet = new HashSet<int>();
        for (int i = 0; i < cycle.Count - 1; i++)
        {
            if (vertexSet.Contains(cycle[i])) return false;
            vertexSet.Add(cycle[i]);
        }

        for (int i = 0; i < cycle.Count - 1; i++)
        {
            int j1 = cycle[i];
            int j2 = cycle[i + 1];
            Vector3 a1 = new Vector3(vertices[j1].x, vertices[j1].y, vertices[j1].z);
            Vector3 a2 = new Vector3(vertices[j2].x, vertices[j2].y, vertices[j2].z);

            for (int j = i + 2; j < cycle.Count - 1; j++)
            {
                if (j == i || j == i + 1 || (i == 0 && j == cycle.Count - 2)) continue;

                int k1 = cycle[j];
                int k2 = cycle[j + 1];
                Vector3 b1 = new Vector3(vertices[k1].x, vertices[k1].y, vertices[k1].z);
                Vector3 b2 = new Vector3(vertices[k2].x, vertices[k2].y, vertices[k2].z);

                if (DoSegmentsIntersect(a1, a2, b1, b2)) return false;
            }
        }
        return true;
    }
}