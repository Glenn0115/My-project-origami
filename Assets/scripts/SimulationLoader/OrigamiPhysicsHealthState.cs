using System.Collections.Generic;
using UnityEngine;

public enum OrigamiPhysicsHealthState
{
    Normal,
    Warning,
    Danger
}

// Samples the real world-space distance between both anchors of every hinge.
public sealed class OrigamiPhysicsMonitor : MonoBehaviour
{
    [Header("Sampling")]
    [Min(0.02f)]
    public float sampleInterval = 0.2f;
    [Min(0f)]
    public float warningSeparation = 0.005f;
    [Min(0f)]
    public float dangerSeparation = 0.02f;
    [Min(1)]
    public int complexityWarningHingeCount = 12;

    [Header("Warning Overlay")]
    public bool showOverlay = true;

    public OrigamiPhysicsHealthState CurrentState { get; private set; }
    public float MaxSeparation { get; private set; }
    public int HingeCount { get; private set; }
    public int WorstCreaseId { get; private set; }
    public int LocallySlowedHingeCount { get; private set; }

    private readonly List<OrigamiHingeInfo> hingeInfos = new List<OrigamiHingeInfo>();
    private readonly Dictionary<OrigamiHingeInfo, float> hingeSeparations =
        new Dictionary<OrigamiHingeInfo, float>();
    private readonly HashSet<OrigamiHingeInfo> locallySlowedHinges =
        new HashSet<OrigamiHingeInfo>();
    private float elapsed;
    private GUIStyle overlayStyle;

    public bool ShouldSlowHinge(OrigamiHingeInfo info)
    {
        return info != null && locallySlowedHinges.Contains(info);
    }

    public void RegisterHinges(IList<OrigamiHingeInfo> source)
    {
        hingeInfos.Clear();
        if (source != null)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null && source[i].hinge != null)
                    hingeInfos.Add(source[i]);
            }
        }

        HingeCount = hingeInfos.Count;
        hingeSeparations.Clear();
        locallySlowedHinges.Clear();
        LocallySlowedHingeCount = 0;
        if (HingeCount == 0)
        {
            MaxSeparation = 0f;
            WorstCreaseId = 0;
            SetState(OrigamiPhysicsHealthState.Normal);
        }
    }

    private void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        if (elapsed < Mathf.Max(0.02f, sampleInterval))
            return;

        elapsed = 0f;
        SampleNow();
    }

    public void SampleNow()
    {
        float maxSeparation = 0f;
        int worstCreaseId = 0;
        int validCount = 0;
        float warningThreshold = Mathf.Min(warningSeparation, dangerSeparation);
        float dangerThreshold = Mathf.Max(warningSeparation, dangerSeparation);
        hingeSeparations.Clear();
        locallySlowedHinges.Clear();

        for (int i = 0; i < hingeInfos.Count; i++)
        {
            OrigamiHingeInfo info = hingeInfos[i];
            HingeJoint hinge = info != null ? info.hinge : null;
            if (hinge == null || hinge.connectedBody == null)
                continue;

            Vector3 ownerPoint = hinge.transform.TransformPoint(hinge.anchor);
            Vector3 connectedPoint = hinge.connectedBody.transform.TransformPoint(hinge.connectedAnchor);
            float separation = Vector3.Distance(ownerPoint, connectedPoint);
            validCount++;
            hingeSeparations[info] = separation;

            if (separation >= warningThreshold)
                locallySlowedHinges.Add(info);

            if (separation > maxSeparation)
            {
                maxSeparation = separation;
                worstCreaseId = info.creaseId;
            }
        }

        // Expand once to hinges sharing a face with an actual warning hinge.
        // Faces discovered from neighbours are deliberately not expanded again.
        var affectedFaceIds = new HashSet<int>();
        foreach (OrigamiHingeInfo info in locallySlowedHinges)
        {
            if (!info.hasFaceTopology)
                continue;
            affectedFaceIds.Add(info.faceAId);
            affectedFaceIds.Add(info.faceBId);
        }

        if (affectedFaceIds.Count > 0)
        {
            foreach (KeyValuePair<OrigamiHingeInfo, float> pair in hingeSeparations)
            {
                OrigamiHingeInfo info = pair.Key;
                if (info.hasFaceTopology
                    && (affectedFaceIds.Contains(info.faceAId)
                        || affectedFaceIds.Contains(info.faceBId)))
                {
                    locallySlowedHinges.Add(info);
                }
            }
        }

        HingeCount = validCount;
        MaxSeparation = maxSeparation;
        WorstCreaseId = worstCreaseId;
        LocallySlowedHingeCount = locallySlowedHinges.Count;

        OrigamiPhysicsHealthState nextState = OrigamiPhysicsHealthState.Normal;
        if (MaxSeparation >= dangerThreshold)
            nextState = OrigamiPhysicsHealthState.Danger;
        else if (MaxSeparation >= warningThreshold || HingeCount >= complexityWarningHingeCount)
            nextState = OrigamiPhysicsHealthState.Warning;

        SetState(nextState);
    }

    private void SetState(OrigamiPhysicsHealthState nextState)
    {
        if (CurrentState == nextState)
            return;

        CurrentState = nextState;
        string message = "[OrigamiPhysicsMonitor] State=" + CurrentState
            + ", hinges=" + HingeCount
            + ", max separation=" + MaxSeparation.ToString("F5")
            + ", crease=" + WorstCreaseId
            + ", locally slowed=" + LocallySlowedHingeCount;

        if (CurrentState == OrigamiPhysicsHealthState.Normal)
            Debug.Log(message);
        else
            Debug.LogWarning(message);
    }

    private void OnGUI()
    {
        if (!showOverlay || CurrentState == OrigamiPhysicsHealthState.Normal)
            return;

        if (overlayStyle == null)
        {
            overlayStyle = new GUIStyle(GUI.skin.box);
            overlayStyle.alignment = TextAnchor.MiddleCenter;
            overlayStyle.fontSize = 16;
            overlayStyle.wordWrap = true;
            overlayStyle.normal.textColor = Color.white;
            overlayStyle.padding = new RectOffset(16, 16, 10, 10);
        }

        string message;
        Color background;
        if (CurrentState == OrigamiPhysicsHealthState.Danger)
        {
            message = "Origami simulation paused: hinge gap "
                + MaxSeparation.ToString("F4")
                + " at crease "
                + WorstCreaseId
                + ".";
            background = new Color(0.65f, 0.08f, 0.08f, 0.95f);
        }
        else if (LocallySlowedHingeCount > 0)
        {
            message = "Origami stability warning: hinge gap "
                + MaxSeparation.ToString("F4")
                + ". Drive speed reduced for "
                + LocallySlowedHingeCount
                + " local hinges.";
            background = new Color(0.75f, 0.45f, 0.02f, 0.95f);
        }
        else
        {
            message = "Complex origami: "
                + HingeCount
                + " hinges. Local gap monitoring is active.";
            background = new Color(0.75f, 0.45f, 0.02f, 0.95f);
        }

        float overlayWidth = Mathf.Max(240f, Mathf.Min(520f, Screen.width - 24f));
        Rect rect = new Rect((Screen.width - overlayWidth) * 0.5f, 18f, overlayWidth, 64f);
        Color previousColor = GUI.backgroundColor;
        GUI.backgroundColor = background;
        GUI.Box(rect, message, overlayStyle);
        GUI.backgroundColor = previousColor;
    }
}
