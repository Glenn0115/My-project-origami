using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Static necessary-condition check for rigid flat-foldability, ported from
// .scratch/check_flat_foldability.py so the same Maekawa/Kawasaki analysis
// runs automatically every time a model is loaded in Unity instead of
// requiring a separate manual script invocation.
public static class OrigamiFoldabilityChecker
{
    private const float AngleTolerance = 1.5f; // degrees, matches the Python tool

    public struct VertexResult
    {
        public int vertexId;
        public int degree;
        public int mountainCount;
        public int valleyCount;
        public bool maekawaOk;
        public bool kawasakiOk;
        public string note;
    }

    public struct Report
    {
        public List<VertexResult> failures;
        public int interiorVertexCount;

        public bool AllPassed => failures == null || failures.Count == 0;
    }

    private struct CreaseRef
    {
        public int other;
        public OrigamiCrease.Type type;
        public int creaseId;
    }

    public static Report CheckModel(OrigamiModel model)
    {
        var report = new Report { failures = new List<VertexResult>() };
        if (model?.vertices == null || model.faces == null || model.creases == null)
            return report;

        Dictionary<int, List<CreaseRef>> creaseIndex = BuildCreaseIndex(model);

        for (int vid = 1; vid <= model.vertices.Count; vid++)
        {
            float cornerSum = FaceCornerAngleSum(model, vid);
            bool isInterior = Mathf.Abs(cornerSum - 360f) < AngleTolerance;
            if (!isInterior)
                continue;

            report.interiorVertexCount++;

            List<CreaseRef> adjacent = creaseIndex.TryGetValue(vid, out var list)
                ? list
                : new List<CreaseRef>();

            VertexResult result = CheckVertex(model, vid, adjacent);
            if (!result.maekawaOk || !result.kawasakiOk)
                report.failures.Add(result);
        }

        return report;
    }

    // Logs one warning per failing vertex and returns the report, so callers
    // can also drive an on-screen banner without recomputing anything.
    public static Report CheckAndLog(OrigamiModel model)
    {
        Report report = CheckModel(model);
        foreach (VertexResult r in report.failures)
        {
            Debug.LogWarning(
                $"[OrigamiFoldabilityChecker] V{r.vertexId} degree={r.degree} "
                + $"M={r.mountainCount} V={r.valleyCount} - {r.note}");
        }

        if (report.failures.Count > 0)
        {
            Debug.LogWarning(
                $"[OrigamiFoldabilityChecker] {report.failures.Count}/{report.interiorVertexCount} "
                + "interior vertices violate Maekawa/Kawasaki - expect hinge gap to grow with fold "
                + "progress at these vertices (geometric, not a physics tuning issue).");
        }
        else
        {
            Debug.Log(
                $"[OrigamiFoldabilityChecker] All {report.interiorVertexCount} interior vertices "
                + "satisfy Maekawa's and Kawasaki's theorems.");
        }

        return report;
    }

    private static Dictionary<int, List<CreaseRef>> BuildCreaseIndex(OrigamiModel model)
    {
        var index = new Dictionary<int, List<CreaseRef>>();
        foreach (OrigamiCrease c in model.creases)
        {
            if (c.type == OrigamiCrease.Type.Boundary)
                continue;

            AddCreaseRef(index, c.v1, c.v2, c.type, c.id);
            AddCreaseRef(index, c.v2, c.v1, c.type, c.id);
        }

        return index;
    }

    private static void AddCreaseRef(
        Dictionary<int, List<CreaseRef>> index, int vertex, int other, OrigamiCrease.Type type, int creaseId)
    {
        if (!index.TryGetValue(vertex, out var list))
        {
            list = new List<CreaseRef>();
            index[vertex] = list;
        }

        list.Add(new CreaseRef { other = other, type = type, creaseId = creaseId });
    }

    private static Vector3 VertexPos(OrigamiModel model, int vid)
    {
        OrigamiVertex v = model.vertices[vid - 1];
        return new Vector3(v.x, v.y, v.z);
    }

    private static float FaceCornerAngleSum(OrigamiModel model, int vid)
    {
        float total = 0f;
        foreach (OrigamiFace face in model.faces)
        {
            List<int> fv = face.vertices;
            int i = fv.IndexOf(vid);
            if (i < 0)
                continue;

            int n = fv.Count;
            int prevV = fv[(i - 1 + n) % n];
            int nextV = fv[(i + 1) % n];
            if (prevV == vid || nextV == vid)
                continue;

            Vector3 p = VertexPos(model, vid);
            Vector3 a = VertexPos(model, prevV) - p;
            Vector3 b = VertexPos(model, nextV) - p;
            total += Vector3.Angle(a, b);
        }

        return total;
    }

    private static VertexResult CheckVertex(OrigamiModel model, int vid, List<CreaseRef> adjacent)
    {
        int degree = adjacent.Count;
        int mountainCount = 0;
        int valleyCount = 0;
        foreach (CreaseRef c in adjacent)
        {
            if (c.type == OrigamiCrease.Type.Mountain) mountainCount++;
            else if (c.type == OrigamiCrease.Type.Valley) valleyCount++;
        }

        var result = new VertexResult
        {
            vertexId = vid,
            degree = degree,
            mountainCount = mountainCount,
            valleyCount = valleyCount,
        };

        if (degree == 0)
        {
            result.maekawaOk = false;
            result.kawasakiOk = false;
            result.note = "interior vertex with no non-boundary creases (invalid data)";
            return result;
        }

        result.maekawaOk = Mathf.Abs(mountainCount - valleyCount) == 2;

        var sb = new StringBuilder();
        if (!result.maekawaOk)
        {
            sb.Append($"Maekawa violated: M-V={mountainCount - valleyCount} (need +/-2).");
            if (degree % 2 != 0)
                sb.Append(" Degree is odd, can never satisfy Maekawa regardless of labeling.");
        }

        Vector3 p = VertexPos(model, vid);
        adjacent.Sort((x, y) =>
        {
            float ax = Mathf.Atan2(VertexPos(model, x.other).z - p.z, VertexPos(model, x.other).x - p.x);
            float ay = Mathf.Atan2(VertexPos(model, y.other).z - p.z, VertexPos(model, y.other).x - p.x);
            return ax.CompareTo(ay);
        });

        if (degree % 2 == 0 && degree > 0)
        {
            var sectors = new float[degree];
            for (int i = 0; i < degree; i++)
            {
                Vector3 a = VertexPos(model, adjacent[i].other) - p;
                Vector3 b = VertexPos(model, adjacent[(i + 1) % degree].other) - p;
                sectors[i] = Vector3.Angle(a, b);
            }

            float oddSum = 0f, evenSum = 0f;
            for (int i = 0; i < degree; i++)
            {
                if (i % 2 == 0) oddSum += sectors[i];
                else evenSum += sectors[i];
            }

            result.kawasakiOk = Mathf.Abs(oddSum - evenSum) < AngleTolerance;
            if (!result.kawasakiOk)
                sb.Append($" Kawasaki violated: odd sector sum={oddSum:F1}, even sector sum={evenSum:F1} (should be equal).");
        }
        else
        {
            result.kawasakiOk = false;
            sb.Append(" Odd number of sectors, Kawasaki condition undefined - generally not flat-foldable.");
        }

        result.note = sb.ToString();
        return result;
    }
}

// Small overlay that shows a warning banner right after a model with a
// failing foldability check is loaded, reusing the color/style convention
// from OrigamiPhysicsMonitor's OnGUI.
public sealed class OrigamiFoldabilityWarning : MonoBehaviour
{
    private string message;
    private bool hasFailure;
    private GUIStyle overlayStyle;

    public void ShowReport(OrigamiFoldabilityChecker.Report report)
    {
        hasFailure = !report.AllPassed;
        if (!hasFailure)
            return;

        OrigamiFoldabilityChecker.VertexResult worst = report.failures[0];
        message = $"Foldability check failed: {report.failures.Count}/{report.interiorVertexCount} "
            + $"interior vertices violate Maekawa/Kawasaki (e.g. V{worst.vertexId}: "
            + $"M={worst.mountainCount},V={worst.valleyCount}). Expect growing hinge gap during folding.";
    }

    private void OnGUI()
    {
        if (!hasFailure)
            return;

        if (overlayStyle == null)
        {
            overlayStyle = new GUIStyle(GUI.skin.box);
            overlayStyle.alignment = TextAnchor.MiddleCenter;
            overlayStyle.fontSize = 14;
            overlayStyle.wordWrap = true;
            overlayStyle.normal.textColor = Color.white;
            overlayStyle.padding = new RectOffset(16, 16, 10, 10);
        }

        float overlayWidth = Mathf.Max(240f, Mathf.Min(560f, Screen.width - 24f));
        Rect rect = new Rect((Screen.width - overlayWidth) * 0.5f, 88f, overlayWidth, 64f);
        Color previousColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.75f, 0.45f, 0.02f, 0.95f);
        GUI.Box(rect, message, overlayStyle);
        GUI.backgroundColor = previousColor;
    }
}
