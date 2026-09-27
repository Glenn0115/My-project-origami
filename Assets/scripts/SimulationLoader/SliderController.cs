using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    private const float SliderAngleMax = 180f;

    public Slider slider;
    public OrigamiController origami;
    public FourServoSequenceController servoSequence;
    private OrigamiLoader origamiLoader;

    private void Awake()
    {
        if (slider == null || origami == null)
            return;

        if (servoSequence == null)
            servoSequence = FindObjectOfType<FourServoSequenceController>();

        origamiLoader = FindObjectOfType<OrigamiLoader>();

        slider.minValue = 0f;
        slider.maxValue = SliderAngleMax;
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

        if (servoSequence != null && servoSequence.IsRunning)
            servoSequence.StopServoSequence();

        float progress = Mathf.InverseLerp(slider.minValue, slider.maxValue, value);

        if (origamiLoader == null)
            origamiLoader = FindObjectOfType<OrigamiLoader>();

        if (origamiLoader != null && origamiLoader.IsElasticFlattenActive)
        {
            origamiLoader.SetElasticFlattenProgress(progress);
            return;
        }

        origami.SetFoldProgress(progress);
    }

    public void UpdateSliderValue()
    {
        if (slider == null || origami == null)
            return;

        if (origamiLoader == null)
            origamiLoader = FindObjectOfType<OrigamiLoader>();

        float progress = origamiLoader != null && origamiLoader.IsElasticFlattenActive
            ? origamiLoader.ElasticFlattenProgress
            : origami.foldProgress;

        slider.SetValueWithoutNotify(
            Mathf.Lerp(slider.minValue, slider.maxValue, Mathf.Clamp01(progress)));
    }
}