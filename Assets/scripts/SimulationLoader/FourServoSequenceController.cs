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
    [SerializeField] private bool loop = true;

    [Header("Servo-to-crease mapping for the movable four-module B model")]
    [SerializeField] private int[] servo1CreaseIds = { 1 };
    [SerializeField] private int[] servo2CreaseIds = { 3 };
    [SerializeField] private int[] servo3CreaseIds = { 5 };
    [SerializeField] private int[] servo4CreaseIds = { 7 };

    private readonly Dictionary<int, HingeJoint> hingesByCreaseId = new Dictionary<int, HingeJoint>();
    private readonly Dictionary<HingeJoint, bool> passiveSpringStates = new Dictionary<HingeJoint, bool>();
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
            Debug.LogError("[FourServoSequence] The loaded model does not provide all four servo crease groups. Load Candidates_NoCut_Waterbomb_4Module_Movable.json first.");
            return;
        }

        abortRequested = false;
        AcquireServoDrive();
        SetAllServos(closedPulseUs);
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
            Debug.LogWarning("[FourServoSequence] Reset skipped because the four-module B model is not loaded completely.");
            return;
        }

        AcquireServoDrive();
        SetAllServos(closedPulseUs);
        ReleaseServoDrive(true);
        Debug.Log("[FourServoSequence] Servo 1-4 -> 1350 us (reset)");
    }

    private IEnumerator RunSequence()
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

        sequenceCoroutine = null;
        ReleaseServoDrive(!abortRequested);
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
        for (int channel = 0; channel < 4; channel++)
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
            if (origamiController == null || origamiController.PhysicsState == OrigamiPhysicsHealthState.Danger)
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
        OrigamiHingeInfo[] infos = origamiController.GetComponentsInChildren<OrigamiHingeInfo>(true);
        for (int i = 0; i < infos.Length; i++)
        {
            OrigamiHingeInfo info = infos[i];
            if (info == null || info.hinge == null || info.creaseId <= 0)
                continue;
            hingesByCreaseId[info.creaseId] = info.hinge;
        }
    }

    private bool HasCompleteServoMapping()
    {
        return HasMappedHinge(servo1CreaseIds)
            && HasMappedHinge(servo2CreaseIds)
            && HasMappedHinge(servo3CreaseIds)
            && HasMappedHinge(servo4CreaseIds);
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
        for (int channel = 0; channel < 4; channel++)
            SetServoPulse(channel, pulseUs);
    }

    private void SetServoPair(int firstChannel, int secondChannel, float pulseUs)
    {
        SetServoPulse(firstChannel, pulseUs);
        SetServoPulse(secondChannel, pulseUs);
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
        switch (channel)
        {
            case 0: return servo1CreaseIds;
            case 1: return servo2CreaseIds;
            case 2: return servo3CreaseIds;
            case 3: return servo4CreaseIds;
            default: return new int[0];
        }
    }

    private void OnDisable()
    {
        StopServoSequence();
    }
}
