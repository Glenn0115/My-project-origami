using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Replays the four-channel PWM order used by the STM32 prototype:
/// 1+3 open, 2+4 open, 2+4 close, then 1+3 close.
///
/// Each servo channel controls one actuator crease for one module in
/// Candidates_NoCut_Waterbomb_4Module_Movable.json. Pulse width is converted to a
/// normalized position inside each hinge's own min/max angle range.
/// </summary>
public sealed class FourServoSequenceController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private OrigamiController origamiController;

    [Header("STM32 PWM Values (microseconds)")]
    [SerializeField] private float closedPulseUs = 1350f;
    [SerializeField] private float openPulseUs = 1650f;

    [Header("Command Delays (seconds)")]
    [SerializeField] private float firstPairDelay = 0.5f;
    [SerializeField] private float allOpenHold = 1.2f;
    [SerializeField] private float secondPairDelay = 0.5f;
    [SerializeField] private float allClosedHold = 1.2f;
    [SerializeField] private float valleyTargetAngle = 100f;
    [SerializeField] private float valleyTargetTolerance = 5f;
    [SerializeField] private float valleyOpenTimeout = 6f;
    [SerializeField] private float mountainDriveTorque = 8f;
    [SerializeField] private float mountainStallSeconds = 0.5f;
    [SerializeField] private float mountainDriveTimeout = 10f;
    [SerializeField] private bool loop = true;

    [Header("Servo-to-crease mapping for the movable four-module B model")]
    [SerializeField] private int[] servo1CreaseIds = { 1 };
    [SerializeField] private int[] servo2CreaseIds = { 3 };
    [SerializeField] private int[] servo3CreaseIds = { 5 };
    [SerializeField] private int[] servo4CreaseIds = { 7 };

    private readonly Dictionary<int, HingeJoint> hingesByCreaseId = new Dictionary<int, HingeJoint>();
    private readonly Dictionary<HingeJoint, bool> passiveSpringStates = new Dictionary<HingeJoint, bool>();
    private readonly List<int>[] modelActuatorCreaseIds =
    {
        new List<int>(), new List<int>(), new List<int>(), new List<int>()
    };
    private bool useThreeServoSequence;
    private Coroutine sequenceCoroutine;
    private bool abortRequested;
    private bool sequenceOwnsControllerState;
    private bool controllerWasEnabled;

    public bool IsRunning { get { return sequenceCoroutine != null; } }

    /// <summary>
    /// Unity UI entry point for Auto_Simu. The first click starts the STM32
    /// sequence; clicking it again stops the loop and returns all servos to
    /// the 1350 us position.
    /// </summary>
    public void ToggleServoSequence()
    {
        if (IsRunning)
        {
            StopAndResetServoSequence();
            return;
        }

        StartServoSequence();
    }

    /// <summary>Unity UI entry point for the Auto_Simu button.</summary>
    public void StartServoSequence()
    {
        StopServoSequence();

        if (origamiController == null)
            origamiController = GetComponent<OrigamiController>();
        if (origamiController == null)
            origamiController = FindObjectOfType<OrigamiController>();
        if (origamiController == null)
        {
            Debug.LogError("[FourServoSequence] OrigamiController was not found.");
            return;
        }

        RebuildHingeLookup();
        if (!HasCompleteServoMapping())
        {
            Debug.LogError("[ServoSequence] The loaded model does not provide a complete three- or four-servo mapping.");
            return;
        }

        abortRequested = false;
        AcquireServoDrive();
        SetAllServos(closedPulseUs);
        Debug.Log(useThreeServoSequence
            ? "[ServoSequence] 920 mode ready: valley 11+12, then mountain 10."
            : "[ServoSequence] Four-servo mode ready.");
        sequenceCoroutine = StartCoroutine(RunSequence());
    }

    public void StopServoSequence()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        abortRequested = false;
        ReleaseServoDrive(false);
    }

    /// <summary>Stops the repeating sequence and commands all four servos home.</summary>
    public void StopAndResetServoSequence()
    {
        StopServoSequence();

        if (origamiController == null)
            origamiController = GetComponent<OrigamiController>();
        if (origamiController == null)
            origamiController = FindObjectOfType<OrigamiController>();
        if (origamiController == null)
        {
            Debug.LogError("[FourServoSequence] Reset failed because OrigamiController was not found.");
            return;
        }

        RebuildHingeLookup();
        if (!HasCompleteServoMapping())
        {
            Debug.LogWarning("[ServoSequence] Reset skipped because the servo mapping is incomplete.");
            return;
        }

        AcquireServoDrive();
        SetAllServos(closedPulseUs);
        ReleaseServoDrive(true);
        Debug.Log("[ServoSequence] All mapped servos -> closed position (reset)");
    }

    private IEnumerator RunSequence()
    {
        if (useThreeServoSequence)
            yield return RunThreeServo920Sequence();
        else
            yield return RunFourServoSequence();

        sequenceCoroutine = null;
        ReleaseServoDrive(!abortRequested);
    }

    private IEnumerator RunFourServoSequence()
    {
        do
        {
            // TIM2_CCR1 = 1650; TIM2_CCR3 = 1650;
            SetServoPair(0, 2, openPulseUs);
            Debug.Log("[FourServoSequence] Servo 1 + 3 -> 1650 us (open)");
            yield return WaitAndWatchPhysics(firstPairDelay);
            if (abortRequested) break;

            // TIM2_CCR2 = 1650; TIM2_CCR4 = 1650;
            SetServoPair(1, 3, openPulseUs);
            Debug.Log("[FourServoSequence] Servo 2 + 4 -> 1650 us (open)");
            yield return WaitAndWatchPhysics(allOpenHold);
            if (abortRequested) break;

            // TIM2_CCR2 = 1350; TIM2_CCR4 = 1350;
            SetServoPair(1, 3, closedPulseUs);
            Debug.Log("[FourServoSequence] Servo 2 + 4 -> 1350 us (close)");
            yield return WaitAndWatchPhysics(secondPairDelay);
            if (abortRequested) break;

            // TIM2_CCR1 = 1350; TIM2_CCR3 = 1350;
            SetServoPair(0, 2, closedPulseUs);
            Debug.Log("[FourServoSequence] Servo 1 + 3 -> 1350 us (close)");
            yield return WaitAndWatchPhysics(allClosedHold);
            if (abortRequested) break;
        }
        while (loop);
    }

    private IEnumerator RunThreeServo920Sequence()
    {
        SetServoAngle(0, valleyTargetAngle, true);
        SetServoAngle(1, valleyTargetAngle, true);
        Debug.Log($"[ServoSequence] Valley servos 1 + 2 -> {valleyTargetAngle:F0} degrees");

        yield return WaitForValleysNearTarget();
        if (abortRequested)
            yield break;

        Debug.Log("[ServoSequence] Valley target reached; mountain torque drive started without angle limits.");
        yield return DriveMountainUntilCollisionOrStall(2);

        // Keep the collision-limited pose. Clicking Auto/Reset again stops
        // this coroutine and restores all three hinges to their home target.
        while (!abortRequested)
            yield return null;
    }

    private void AcquireServoDrive()
    {
        if (origamiController == null || sequenceOwnsControllerState)
            return;

        // OrigamiController normally writes one global fold target every
        // FixedUpdate. Pause it while this component owns the four servo
        // groups, otherwise it would immediately overwrite these targets.
        origamiController.StopTorqueAutoFoldToMax();
        controllerWasEnabled = origamiController.enabled;
        origamiController.enabled = false;
        MakeUnactuatedCreasesPassive();
        sequenceOwnsControllerState = true;
    }

    private void ReleaseServoDrive(bool resetGlobalProgress)
    {
        if (origamiController == null || !sequenceOwnsControllerState)
            return;

        if (resetGlobalProgress)
            origamiController.SetFoldProgress(0f);

        RestorePassiveCreaseSprings();
        origamiController.enabled = controllerWasEnabled;
        sequenceOwnsControllerState = false;
    }

    private void MakeUnactuatedCreasesPassive()
    {
        passiveSpringStates.Clear();
        var actuatedIds = new HashSet<int>();
        for (int channel = 0; channel < ActiveChannelCount; channel++)
        {
            int[] creaseIds = GetCreaseIds(channel);
            for (int i = 0; i < creaseIds.Length; i++)
                actuatedIds.Add(creaseIds[i]);
        }

        foreach (KeyValuePair<int, HingeJoint> entry in hingesByCreaseId)
        {
            HingeJoint hinge = entry.Value;
            if (hinge == null || actuatedIds.Contains(entry.Key))
                continue;

            passiveSpringStates[hinge] = hinge.useSpring;
            hinge.useSpring = false;
        }
    }

    private void RestorePassiveCreaseSprings()
    {
        foreach (KeyValuePair<HingeJoint, bool> entry in passiveSpringStates)
        {
            if (entry.Key != null)
                entry.Key.useSpring = entry.Value;
        }
        passiveSpringStates.Clear();
    }

    private IEnumerator WaitAndWatchPhysics(float seconds)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0f, seconds);
        while (elapsed < duration)
        {
            // The compact 920 model can report an initial separation warning
            // before its 180-degree hinges have had one physics step to move.
            // Do not cancel that three-servo sequence before it can start.
            if (origamiController == null
                || (!useThreeServoSequence
                    && origamiController.PhysicsState == OrigamiPhysicsHealthState.Danger))
            {
                abortRequested = true;
                Debug.LogWarning("[FourServoSequence] Sequence stopped because the origami physics monitor reported danger.");
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void RebuildHingeLookup()
    {
        hingesByCreaseId.Clear();
        for (int i = 0; i < modelActuatorCreaseIds.Length; i++)
            modelActuatorCreaseIds[i].Clear();

        OrigamiHingeInfo[] infos = origamiController.GetComponentsInChildren<OrigamiHingeInfo>(true);
        for (int i = 0; i < infos.Length; i++)
        {
            OrigamiHingeInfo info = infos[i];
            if (info == null || info.hinge == null || info.creaseId <= 0)
                continue;
            hingesByCreaseId[info.creaseId] = info.hinge;
            if (info.actuatorGroup >= 1 && info.actuatorGroup <= 4)
                modelActuatorCreaseIds[info.actuatorGroup - 1].Add(info.creaseId);
        }

        // Compatibility fallback for manually edited 920.json files whose
        // duplicated JSON keys may be read as actuatorGroup=0 by JsonUtility.
        bool hasNoDeclaredGroups = true;
        for (int i = 0; i < modelActuatorCreaseIds.Length; i++)
            hasNoDeclaredGroups &= modelActuatorCreaseIds[i].Count == 0;

        if (hasNoDeclaredGroups
            && hingesByCreaseId.ContainsKey(10)
            && hingesByCreaseId.ContainsKey(11)
            && hingesByCreaseId.ContainsKey(12))
        {
            modelActuatorCreaseIds[0].Add(11); // Valley 1
            modelActuatorCreaseIds[1].Add(12); // Valley 2
            modelActuatorCreaseIds[2].Add(10); // Mountain
            Debug.LogWarning("[ServoSequence] 920 actuator metadata was missing; using crease fallback 11, 12, then 10.");
        }

        useThreeServoSequence = modelActuatorCreaseIds[0].Count > 0
            && modelActuatorCreaseIds[1].Count > 0
            && modelActuatorCreaseIds[2].Count > 0
            && modelActuatorCreaseIds[3].Count == 0;

        Debug.Log($"[ServoSequence] Hinges={hingesByCreaseId.Count}, groups="
            + $"{modelActuatorCreaseIds[0].Count}/"
            + $"{modelActuatorCreaseIds[1].Count}/"
            + $"{modelActuatorCreaseIds[2].Count}/"
            + $"{modelActuatorCreaseIds[3].Count}, threeServo={useThreeServoSequence}.");
    }

    private IEnumerator WaitForValleysNearTarget()
    {
        float elapsed = 0f;
        float timeout = Mathf.Max(0.1f, valleyOpenTimeout);
        while (elapsed < timeout)
        {
            if (AreChannelsNearAngle(0, 1, valleyTargetAngle))
            {
                Debug.Log("[ServoSequence] Both valley folds reached the 100-degree stage; starting mountain fold.");
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning("[ServoSequence] Valley folds did not reach 100 degrees before timeout; attempting mountain fold now.");
    }

    private bool AreChannelsNearAngle(int firstChannel, int secondChannel, float targetAngle)
    {
        return IsChannelNearAngle(firstChannel, targetAngle)
            && IsChannelNearAngle(secondChannel, targetAngle);
    }

    private bool IsChannelNearAngle(int channel, float targetAngle)
    {
        int[] creaseIds = GetCreaseIds(channel);
        if (creaseIds == null || creaseIds.Length == 0)
            return false;

        for (int i = 0; i < creaseIds.Length; i++)
        {
            if (!hingesByCreaseId.TryGetValue(creaseIds[i], out HingeJoint hinge) || hinge == null)
                return false;

            if (Mathf.Abs(Mathf.DeltaAngle(hinge.angle, targetAngle)) > Mathf.Max(0.1f, valleyTargetTolerance))
                return false;
        }

        return true;
    }

    private IEnumerator DriveMountainUntilCollisionOrStall(int channel)
    {
        int[] creaseIds = GetCreaseIds(channel);
        var mountainHinges = new List<HingeJoint>();
        var previousAngles = new Dictionary<HingeJoint, float>();
        var stableTimes = new Dictionary<HingeJoint, float>();

        for (int i = 0; i < creaseIds.Length; i++)
        {
            if (!hingesByCreaseId.TryGetValue(creaseIds[i], out HingeJoint hinge) || hinge == null)
                continue;

            hinge.useLimits = false;
            hinge.useSpring = false;
            mountainHinges.Add(hinge);
            previousAngles[hinge] = hinge.angle;
            stableTimes[hinge] = 0f;
        }

        float elapsed = 0f;
        bool stalled = false;
        while (!stalled && elapsed < Mathf.Max(0.5f, mountainDriveTimeout))
        {
            yield return new WaitForFixedUpdate();
            elapsed += Time.fixedDeltaTime;
            stalled = elapsed >= 0.25f && mountainHinges.Count > 0;

            for (int i = 0; i < mountainHinges.Count; i++)
            {
                HingeJoint hinge = mountainHinges[i];
                if (hinge == null)
                    continue;

                Rigidbody ownerBody = hinge.GetComponent<Rigidbody>();
                Vector3 worldAxis = hinge.transform.TransformDirection(hinge.axis).normalized;
                if (ownerBody != null && worldAxis.sqrMagnitude > 0.0001f)
                {
                    Vector3 torque = worldAxis * Mathf.Abs(mountainDriveTorque);
                    ownerBody.AddTorque(torque, ForceMode.Acceleration);
                    if (hinge.connectedBody != null && !hinge.connectedBody.isKinematic)
                        hinge.connectedBody.AddTorque(-torque, ForceMode.Acceleration);
                }

                float angleDelta = Mathf.Abs(Mathf.DeltaAngle(previousAngles[hinge], hinge.angle));
                bool stopped = angleDelta <= 0.05f && Mathf.Abs(hinge.velocity) <= 0.5f;
                stableTimes[hinge] = stopped ? stableTimes[hinge] + Time.fixedDeltaTime : 0f;
                previousAngles[hinge] = hinge.angle;
                stalled &= stableTimes[hinge] >= Mathf.Max(0.1f, mountainStallSeconds);
            }
        }

        for (int i = 0; i < mountainHinges.Count; i++)
        {
            HingeJoint hinge = mountainHinges[i];
            if (hinge == null)
                continue;
            JointSpring spring = hinge.spring;
            spring.targetPosition = hinge.angle;
            hinge.spring = spring;
            hinge.useSpring = true;
        }

        Debug.Log(stalled
            ? "[ServoSequence] Mountain hinge stopped by collision/mechanical stall; holding current angle."
            : "[ServoSequence] Mountain torque drive reached safety timeout; holding current angle.");
    }

    private bool HasCompleteServoMapping()
    {
        if (useThreeServoSequence)
        {
            return HasMappedHinge(GetCreaseIds(0))
                && HasMappedHinge(GetCreaseIds(1))
                && HasMappedHinge(GetCreaseIds(2));
        }

        return HasMappedHinge(GetCreaseIds(0))
            && HasMappedHinge(GetCreaseIds(1))
            && HasMappedHinge(GetCreaseIds(2))
            && HasMappedHinge(GetCreaseIds(3));
    }

    private bool HasMappedHinge(int[] creaseIds)
    {
        if (creaseIds == null || creaseIds.Length == 0)
            return false;
        for (int i = 0; i < creaseIds.Length; i++)
        {
            if (!hingesByCreaseId.TryGetValue(creaseIds[i], out HingeJoint hinge) || hinge == null)
                return false;
        }
        return true;
    }

    private void SetAllServos(float pulseUs)
    {
        for (int channel = 0; channel < ActiveChannelCount; channel++)
            SetServoPulse(channel, pulseUs);
    }

    private void SetServoPair(int firstChannel, int secondChannel, float pulseUs)
    {
        SetServoPulse(firstChannel, pulseUs);
        SetServoPulse(secondChannel, pulseUs);
    }

    private void SetServoAngle(int channel, float targetAngle, bool useLimits)
    {
        int[] creaseIds = GetCreaseIds(channel);
        for (int i = 0; i < creaseIds.Length; i++)
        {
            if (!hingesByCreaseId.TryGetValue(creaseIds[i], out HingeJoint hinge) || hinge == null)
                continue;

            JointSpring spring = hinge.spring;
            spring.targetPosition = useLimits
                ? Mathf.Clamp(targetAngle, hinge.limits.min, hinge.limits.max)
                : targetAngle;
            hinge.spring = spring;
            hinge.useLimits = useLimits;
            hinge.useSpring = true;
        }
    }

    private void SetServoPulse(int channel, float pulseUs)
    {
        int[] creaseIds = GetCreaseIds(channel);
        float denominator = openPulseUs - closedPulseUs;
        float progress = Mathf.Abs(denominator) < 0.001f
            ? 0f
            : Mathf.Clamp01((pulseUs - closedPulseUs) / denominator);

        for (int i = 0; i < creaseIds.Length; i++)
        {
            if (!hingesByCreaseId.TryGetValue(creaseIds[i], out HingeJoint hinge) || hinge == null)
                continue;

            JointLimits limits = hinge.limits;
            JointSpring spring = hinge.spring;
            spring.targetPosition = Mathf.Lerp(limits.min, limits.max, progress);
            hinge.useLimits = true;
            hinge.useSpring = true;
            hinge.spring = spring;
        }
    }

    private int[] GetCreaseIds(int channel)
    {
        if (channel >= 0 && channel < modelActuatorCreaseIds.Length
            && modelActuatorCreaseIds[channel].Count > 0)
            return modelActuatorCreaseIds[channel].ToArray();

        switch (channel)
        {
            case 0: return servo1CreaseIds;
            case 1: return servo2CreaseIds;
            case 2: return servo3CreaseIds;
            case 3: return servo4CreaseIds;
            default: return new int[0];
        }
    }

    private int ActiveChannelCount { get { return useThreeServoSequence ? 3 : 4; } }

    private void OnDisable()
    {
        StopServoSequence();
    }
}
