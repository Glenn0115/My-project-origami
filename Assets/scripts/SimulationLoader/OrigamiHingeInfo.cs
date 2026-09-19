using UnityEngine;

// Metadata for one HingeJoint. Multiple instances can live on the same face.
public sealed class OrigamiHingeInfo : MonoBehaviour
{
    public HingeJoint hinge;
    public int creaseId;
    public int faceAId;
    public int faceBId;
    public bool hasFaceTopology;
    public bool isDriver = true;
    public bool springDriveEnabled = true;
    public int actuatorGroup;

    [Range(0.05f, 1f)]
    public float driveWeight = 1f;

    public void Configure(
        HingeJoint targetHinge,
        int targetCreaseId,
        int targetFaceAId,
        int targetFaceBId,
        bool driver,
        float weight)
    {
        hinge = targetHinge;
        creaseId = targetCreaseId;
        faceAId = targetFaceAId;
        faceBId = targetFaceBId;
        hasFaceTopology = true;
        isDriver = driver;
        driveWeight = Mathf.Clamp(weight, 0.05f, 1f);
    }
}
