using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kinematic preview for a six-petal lotus driven by 2-6 servo channels.
/// With two channels, alternate petals share a channel and move simultaneously.
/// This is a mechanism/angle preview, not a structural-strength simulation.
/// </summary>
public sealed class LotusServoPreview : MonoBehaviour
{
    [Range(2, 6)] public int servoCount = 2;
    [Range(0f, 70f)] public float closedAngle = 55f;
    public float cycleSeconds = 4f;
    public bool autoRun = true;

    private readonly List<Transform> petalPivots = new List<Transform>();
    private float commandedAngle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<LotusServoPreview>() != null)
            return;

        GameObject legacySystem = GameObject.Find("OrigamiSystem");
        if (legacySystem != null)
            legacySystem.SetActive(false);
        GameObject legacyPattern = GameObject.Find("CreasePattern");
        if (legacyPattern != null)
            legacyPattern.SetActive(false);
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
            canvas.gameObject.SetActive(false);

        GameObject preview = new GameObject("Lotus Servo Preview (2-6 channels)");
        preview.AddComponent<LotusServoPreview>();
    }

    private void Start()
    {
        BuildLotus();
        FrameCamera();
    }

    private void Update()
    {
        if (autoRun)
        {
            float duration = Mathf.Max(0.5f, cycleSeconds);
            float progress = Mathf.PingPong(Time.time * 2f / duration, 1f);
            commandedAngle = Mathf.SmoothStep(0f, closedAngle, progress);
        }

        ApplyServoCommand(commandedAngle);
    }

    private void BuildLotus()
    {
        const int petalCount = 6;
        const float petalLength = 3.8f;
        const float petalWidth = 1.7f;

        for (int i = 0; i < petalCount; i++)
        {
            float yaw = i * 360f / petalCount;
            GameObject pivot = new GameObject("PetalPivot_" + (i + 1));
            pivot.transform.SetParent(transform, false);
            pivot.transform.localRotation = Quaternion.AngleAxis(yaw, Vector3.up);

            GameObject petal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            petal.name = "Petal_" + (i + 1) + "_Servo_" + (i % servoCount + 1);
            petal.transform.SetParent(pivot.transform, false);
            petal.transform.localPosition = new Vector3(petalLength * 0.5f, 0f, 0f);
            petal.transform.localScale = new Vector3(petalLength, 0.08f, petalWidth);

            Renderer renderer = petal.GetComponent<Renderer>();
            renderer.material.color = i % 2 == 0
                ? new Color(0.95f, 0.35f, 0.55f)
                : new Color(1f, 0.65f, 0.75f);
            petalPivots.Add(pivot.transform);
        }

        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        center.name = "LotusCenter";
        center.transform.SetParent(transform, false);
        center.transform.localScale = new Vector3(1.15f, 0.12f, 1.15f);
        center.GetComponent<Renderer>().material.color = new Color(1f, 0.75f, 0.1f);
    }

    private void ApplyServoCommand(float angle)
    {
        int channels = Mathf.Clamp(servoCount, 2, 6);
        for (int i = 0; i < petalPivots.Count; i++)
        {
            int channel = i % channels;
            float channelAngle = angle; // All enabled channels receive the same frame command.
            float yaw = i * 360f / petalPivots.Count;
            petalPivots[i].localRotation = Quaternion.AngleAxis(yaw, Vector3.up)
                * Quaternion.AngleAxis(-channelAngle, Vector3.forward);
        }
    }

    private void FrameCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;

        camera.transform.position = new Vector3(0f, 8f, -10f);
        camera.transform.LookAt(new Vector3(0f, 1f, 0f));
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(12, 12, 280, 72), "Lotus servo preview");
        GUI.Label(new Rect(24, 36, 250, 22), "Servo channels: " + Mathf.Clamp(servoCount, 2, 6));
        GUI.Label(new Rect(24, 56, 250, 22), "Synchronous command: " + commandedAngle.ToString("F1") + " deg");
    }
}

