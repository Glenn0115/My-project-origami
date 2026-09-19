using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider;
    public OrigamiController origami;
    public FourServoSequenceController servoSequence;

    private void Awake()
    {
        if (slider == null || origami == null)
            return;

        if (servoSequence == null)
            servoSequence = FindObjectOfType<FourServoSequenceController>();

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(0f);
        origami.SetFoldProgress(0f);
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        if (origami == null)
            return;

        // Manual slider input takes ownership back from Auto / Reset.
        // Otherwise the sequence keeps OrigamiController paused and the
        // slider value changes without applying any hinge targets.
        if (servoSequence != null && servoSequence.IsRunning)
            servoSequence.StopServoSequence();

        origami.SetFoldProgress(value);
    }

    public void UpdateSliderValue()
    {
        if (slider == null || origami == null)
            return;

        slider.SetValueWithoutNotify(Mathf.Clamp01(origami.foldProgress));
    }
}
