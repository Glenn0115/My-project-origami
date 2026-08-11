using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Coordinates all hinge drives while leaving every HingeJoint in the model.
public class OrigamiController : MonoBehaviour
{
    private readonly List<HingeJoint> hinges = new List<HingeJoint>();
    private readonly List<OrigamiHingeInfo> hingeInfos = new List<OrigamiHingeInfo>();

    public float foldSpeed = 30f;
    public float rotationSpeed = 50f;
    public bool enableKeyboardRotation = false;
    public List<HingeJoint> GetHinges() { return hinges; }

    [Header("Fold State")]
    [Range(0f, 1f)]
    public float foldProgress = 0f;
    [Tooltip("Maximum target-angle speed for driver hinges, in degrees per second.")]
    public float maxDriverAngleSpeed = 20f;
    [Tooltip("Maximum target-angle speed for follower hinges, in degrees per second.")]
    public float maxFollowerAngleSpeed = 8f;
    [Range(0.05f, 1f)]
    public float warningSpeedMultiplier = 0.25f;
    public bool pauseOnDanger = true;

    [Header("Torque Auto Fold")]
    public bool autoFoldToMaxOnStart = false;
    public float torqueAutoFoldDirection = 1f;
    public float torqueAutoFoldDamping = 0.04f;
    public float torqueAutoFoldMaxTorque = 8f;
    public float torqueAutoFoldVelocityTolerance = 0.5f;
    public float torqueAutoFoldAngleChangeTolerance = 0.05f;
    public float torqueAutoFoldStallSeconds = 0.5f;
    public float torqueAutoFoldMinRunTime = 0.25f;
    public bool disableSpringDuringTorqueAutoFold = true;

    private float requestedFoldProgress;
    private float appliedFoldProgress;
    private float lastFoldProgress;
    private float lastStableFoldProgress;
    private bool dangerPauseApplied;

    private bool torqueAutoFoldActive;
    private float torqueAutoFoldElapsed;
    private readonly Dictionary<HingeJoint, float> torqueAutoFoldLastAngles = new Dictionary<HingeJoint, float>();
    private readonly Dictionary<HingeJoint, float> torqueAutoFoldStableTimes = new Dictionary<HingeJoint, float>();
    private readonly HashSet<HingeJoint> torqueAutoFoldStoppedHinges = new HashSet<HingeJoint>();

    private OrigamiPhysicsMonitor physicsMonitor;

    public float AppliedFoldProgress { get { return appliedFoldProgress; } }
    public float LastStableFoldProgress { get { return lastStableFoldProgress; } }
    public OrigamiPhysicsHealthState PhysicsState
    {
        get { return physicsMonitor != null ? physicsMonitor.CurrentState : OrigamiPhysicsHealthState.Normal; }
    }

    private static bool IsJointValid(HingeJoint joint)
    {
        return joint != null && joint.GetInstanceID() != 0;
    }

    private void Awake()
    {
        physicsMonitor = GetComponent<OrigamiPhysicsMonitor>();
        if (physicsMonitor == null)
            physicsMonitor = gameObject.AddComponent<OrigamiPhysicsMonitor>();
    }

    private void Start()
    {
        requestedFoldProgress = Mathf.Clamp01(foldProgress);
        lastFoldProgress = requestedFoldProgress;
        lastStableFoldProgress = requestedFoldProgress;

        var initialJoints = FindObjectsOfType<HingeJoint>().Where(IsJointValid).ToList();
        foreach (var joint in initialJoints)
            AddHinge(joint);

        if (physicsMonitor != null)
            physicsMonitor.RegisterHinges(hingeInfos);

        if (autoFoldToMaxOnStart)
            StartTorqueAutoFoldToMax();

        Debug.Log("[OrigamiController] Found " + initialJoints.Count + " valid hinge joints.");
    }

    public void AddHinge(HingeJoint hinge)
    {
        if (!IsJointValid(hinge))
        {
            Debug.LogWarning("[OrigamiController] Ignored an invalid hinge joint.");
            return;
        }

        OrigamiHingeInfo info = FindOrCreateHingeInfo(hinge);
        AddHinge(hinge, info);
    }

    public void AddHinge(HingeJoint hinge, OrigamiHingeInfo info)
    {
        if (!IsJointValid(hinge) || info == null || hinges.Contains(hinge))
            return;

        info.hinge = hinge;
        hinges.Add(hinge);
        hingeInfos.Add(info);

        if (torqueAutoFoldActive)
        {
            ConfigureHingeForTorqueAutoFold(hinge, info);
            if (info.isDriver)
                TrackTorqueAutoFoldHinge(hinge);
        }

        if (physicsMonitor != null)
            physicsMonitor.RegisterHinges(hingeInfos);
    }

    private OrigamiHingeInfo FindOrCreateHingeInfo(HingeJoint hinge)
    {
        var existing = hinge.GetComponents<OrigamiHingeInfo>();
        for (int i = 0; i < existing.Length; i++)
        {
            if (existing[i] != null && existing[i].hinge == hinge)
                return existing[i];
        }

        var created = hinge.gameObject.AddComponent<OrigamiHingeInfo>();
        created.hinge = hinge;
        created.isDriver = true;
        created.driveWeight = 1f;
        return created;
    }

    private void Update()
    {
        FilterInvalidJoints();
        if (hinges.Count == 0)
            return;

        if (!torqueAutoFoldActive)
        {
            HandleFoldInput();
            SyncFoldProgressWithSlider();
        }

        HandleRotationInput();
    }

    private void FixedUpdate()
    {
        FilterInvalidJoints();
        if (hinges.Count == 0)
            return;

        if (PhysicsState == OrigamiPhysicsHealthState.Danger && pauseOnDanger)
        {
            if (!dangerPauseApplied)
            {
                FreezeSpringTargetsAtCurrentAngles();
                requestedFoldProgress = lastStableFoldProgress;
                foldProgress = lastStableFoldProgress;
                lastFoldProgress = foldProgress;
                dangerPauseApplied = true;
                Debug.LogWarning("[OrigamiController] Physics danger detected. Hinge drive force was released; the last stable target is restored.");
            }

            if (torqueAutoFoldActive)
                StopTorqueAutoFoldToMax();
            return;
        }

        if (PhysicsState != OrigamiPhysicsHealthState.Danger)
            dangerPauseApplied = false;

        if (torqueAutoFoldActive)
        {
            ApplyTorqueAutoFoldToMax();
            return;
        }

        ApplySpringDrive();
    }

    private void FilterInvalidJoints()
    {
        bool changed = false;
        for (int i = hinges.Count - 1; i >= 0; i--)
        {
            bool hasInfo = i < hingeInfos.Count;
            if (IsJointValid(hinges[i]) && hasInfo && hingeInfos[i] != null)
                continue;

            hinges.RemoveAt(i);
            if (hasInfo)
                hingeInfos.RemoveAt(i);
            changed = true;
        }

        if (changed && physicsMonitor != null)
            physicsMonitor.RegisterHinges(hingeInfos);
    }

    private void HandleFoldInput()
    {
        float range = GetAverageAngleRange();
        float progressDelta = foldSpeed * Time.deltaTime / Mathf.Max(1f, range);
        if (Input.GetKey(KeyCode.UpArrow))
            AdjustFold(progressDelta);
        if (Input.GetKey(KeyCode.DownArrow))
            AdjustFold(-progressDelta);
    }

    private void HandleRotationInput()
    {
        if (!enableKeyboardRotation)
            return;

        if (Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void SyncFoldProgressWithSlider()
    {
        if (Mathf.Abs(foldProgress - lastFoldProgress) > 0.001f)
        {
            requestedFoldProgress = Mathf.Clamp01(foldProgress);
            lastFoldProgress = foldProgress;
        }
    }

    private void AdjustFold(float delta)
    {
        SetFoldProgress(requestedFoldProgress + delta);
    }

    // Keeps the public API used by SliderController. The actual targets move in FixedUpdate.
    public void SetFoldProgress(float progress)
    {
        requestedFoldProgress = Mathf.Clamp01(progress);
        foldProgress = requestedFoldProgress;
        lastFoldProgress = foldProgress;
    }

    private void ApplySpringDrive()
    {
        float speedMultiplier = PhysicsState == OrigamiPhysicsHealthState.Normal
            ? 1f
            : warningSpeedMultiplier;

        for (int i = 0; i < hingeInfos.Count; i++)
        {
            OrigamiHingeInfo info = hingeInfos[i];
            HingeJoint hinge = info != null ? info.hinge : null;
            if (!IsJointValid(hinge))
                continue;

            JointSpring spring = hinge.spring;
            float target = GetTargetAngle(hinge, requestedFoldProgress);
            float angleSpeed = info.isDriver
                ? maxDriverAngleSpeed * Mathf.Max(0.05f, info.driveWeight)
                : maxFollowerAngleSpeed;
            angleSpeed *= speedMultiplier;
            spring.targetPosition = Mathf.MoveTowards(
                spring.targetPosition,
                target,
                Mathf.Max(0.01f, angleSpeed) * Time.fixedDeltaTime);
            hinge.useSpring = true;
            hinge.spring = spring;
        }

        appliedFoldProgress = CalculateAppliedFoldProgress();
        bool gapIsHealthy = physicsMonitor == null
            || physicsMonitor.MaxSeparation < physicsMonitor.warningSeparation;
        if (PhysicsState != OrigamiPhysicsHealthState.Danger && gapIsHealthy)
            lastStableFoldProgress = appliedFoldProgress;
    }

    private void FreezeSpringTargetsAtCurrentAngles()
    {
        for (int i = 0; i < hinges.Count; i++)
        {
            HingeJoint hinge = hinges[i];
            if (!IsJointValid(hinge))
                continue;

            JointSpring spring = hinge.spring;
            spring.targetPosition = Mathf.Clamp(hinge.angle, hinge.limits.min, hinge.limits.max);
            hinge.spring = spring;
            hinge.useLimits = true;
            hinge.useSpring = true;
        }
    }

    private float GetTargetAngle(HingeJoint hinge, float progress)
    {
        return Mathf.Lerp(hinge.limits.min, hinge.limits.max, Mathf.Clamp01(progress));
    }

    private float GetAverageAngleRange()
    {
        float total = 0f;
        int count = 0;
        for (int i = 0; i < hinges.Count; i++)
        {
            if (!IsJointValid(hinges[i]))
                continue;
            total += Mathf.Abs(hinges[i].limits.max - hinges[i].limits.min);
            count++;
        }

        return count > 0 ? total / count : 180f;
    }

    private float CalculateAppliedFoldProgress()
    {
        float total = 0f;
        int count = 0;
        bool hasDriver = false;

        for (int i = 0; i < hingeInfos.Count; i++)
        {
            OrigamiHingeInfo info = hingeInfos[i];
            HingeJoint hinge = info != null ? info.hinge : null;
            if (IsJointValid(hinge) && info.isDriver)
                hasDriver = true;
        }

        for (int i = 0; i < hingeInfos.Count; i++)
        {
            OrigamiHingeInfo info = hingeInfos[i];
            HingeJoint hinge = info != null ? info.hinge : null;
            if (!IsJointValid(hinge) || (hasDriver && !info.isDriver))
                continue;

            float range = hinge.limits.max - hinge.limits.min;
            if (Mathf.Abs(range) < 0.001f)
                continue;
            total += Mathf.Clamp01((hinge.spring.targetPosition - hinge.limits.min) / range);
            count++;
        }

        return count > 0 ? total / count : requestedFoldProgress;
    }

    public void StartTorqueAutoFoldToMax()
    {
        FilterInvalidJoints();
        requestedFoldProgress = 1f;
        foldProgress = 1f;
        lastFoldProgress = 1f;
        torqueAutoFoldActive = true;
        torqueAutoFoldElapsed = 0f;
        ClearTorqueAutoFoldState();

        for (int i = 0; i < hingeInfos.Count; i++)
        {
            OrigamiHingeInfo info = hingeInfos[i];
            if (info == null || !IsJointValid(info.hinge))
                continue;
            ConfigureHingeForTorqueAutoFold(info.hinge, info);
            if (info.isDriver)
                TrackTorqueAutoFoldHinge(info.hinge);
        }
    }

    public void StopTorqueAutoFoldToMax()
    {
        torqueAutoFoldActive = false;
        torqueAutoFoldElapsed = 0f;
        ClearTorqueAutoFoldState();

        for (int i = 0; i < hingeInfos.Count; i++)
        {
            OrigamiHingeInfo info = hingeInfos[i];
            if (info == null || !IsJointValid(info.hinge))
                continue;
            info.hinge.useLimits = true;
            info.hinge.useSpring = true;
        }
    }

    private void ConfigureHingeForTorqueAutoFold(HingeJoint hinge, OrigamiHingeInfo info)
    {
        if (!IsJointValid(hinge))
            return;

        hinge.useLimits = true;
        hinge.useSpring = !info.isDriver || !disableSpringDuringTorqueAutoFold;
    }

    private void ApplyTorqueAutoFoldToMax()
    {
        torqueAutoFoldElapsed += Time.fixedDeltaTime;
        bool anyActiveDriver = false;
        float direction = Mathf.Sign(torqueAutoFoldDirection);
        if (Mathf.Abs(direction) < 0.001f)
            direction = 1f;
        float healthMultiplier = PhysicsState == OrigamiPhysicsHealthState.Normal
            ? 1f
            : warningSpeedMultiplier;

        for (int i = 0; i < hingeInfos.Count; i++)
        {
            OrigamiHingeInfo info = hingeInfos[i];
            HingeJoint hinge = info != null ? info.hinge : null;
            if (info == null || !info.isDriver || !IsJointValid(hinge))
                continue;

            if (!torqueAutoFoldLastAngles.ContainsKey(hinge))
                TrackTorqueAutoFoldHinge(hinge);
            if (torqueAutoFoldStoppedHinges.Contains(hinge))
                continue;
            if (IsTorqueHingeStateUnchanged(hinge))
            {
                torqueAutoFoldStoppedHinges.Add(hinge);
                continue;
            }

            Rigidbody ownerBody = hinge.GetComponent<Rigidbody>();
            if (ownerBody == null)
                continue;

            Vector3 worldAxis = hinge.transform.TransformDirection(hinge.axis).normalized;
            if (worldAxis.sqrMagnitude < 0.0001f)
                continue;

            float maxTorque = Mathf.Abs(torqueAutoFoldMaxTorque) * healthMultiplier;
            float drive = direction * maxTorque - hinge.velocity * torqueAutoFoldDamping;
            drive = Mathf.Clamp(drive, -maxTorque, maxTorque);
            Vector3 torque = worldAxis * drive;
            ownerBody.AddTorque(torque, ForceMode.Acceleration);
            if (hinge.connectedBody != null && !hinge.connectedBody.isKinematic)
                hinge.connectedBody.AddTorque(-torque, ForceMode.Acceleration);
            anyActiveDriver = true;
        }

        if (!anyActiveDriver)
            StopTorqueAutoFoldToMax();
    }

    private void TrackTorqueAutoFoldHinge(HingeJoint hinge)
    {
        if (!IsJointValid(hinge))
            return;
        torqueAutoFoldLastAngles[hinge] = hinge.angle;
        torqueAutoFoldStableTimes[hinge] = 0f;
    }

    private bool IsTorqueHingeStateUnchanged(HingeJoint hinge)
    {
        if (torqueAutoFoldElapsed < torqueAutoFoldMinRunTime)
        {
            TrackTorqueAutoFoldHinge(hinge);
            return false;
        }

        float previousAngle = torqueAutoFoldLastAngles.TryGetValue(hinge, out float angle)
            ? angle
            : hinge.angle;
        float angleDelta = Mathf.Abs(Mathf.DeltaAngle(previousAngle, hinge.angle));
        bool angleUnchanged = angleDelta <= torqueAutoFoldAngleChangeTolerance;
        bool velocitySettled = Mathf.Abs(hinge.velocity) <= torqueAutoFoldVelocityTolerance;

        if (angleUnchanged && velocitySettled)
        {
            float stableTime = torqueAutoFoldStableTimes.TryGetValue(hinge, out float time) ? time : 0f;
            stableTime += Time.fixedDeltaTime;
            torqueAutoFoldStableTimes[hinge] = stableTime;
            torqueAutoFoldLastAngles[hinge] = hinge.angle;
            return stableTime >= torqueAutoFoldStallSeconds;
        }

        torqueAutoFoldStableTimes[hinge] = 0f;
        torqueAutoFoldLastAngles[hinge] = hinge.angle;
        return false;
    }

    private void ClearTorqueAutoFoldState()
    {
        torqueAutoFoldLastAngles.Clear();
        torqueAutoFoldStableTimes.Clear();
        torqueAutoFoldStoppedHinges.Clear();
    }

    public void ClearAllHinges()
    {
        torqueAutoFoldActive = false;
        torqueAutoFoldElapsed = 0f;
        ClearTorqueAutoFoldState();
        hinges.Clear();
        hingeInfos.Clear();
        if (physicsMonitor != null)
            physicsMonitor.RegisterHinges(hingeInfos);
    }

    public int GetValidHingeCount()
    {
        FilterInvalidJoints();
        return hinges.Count;
    }
}
