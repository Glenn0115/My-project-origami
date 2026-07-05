using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider;
    public OrigamiController origami;

    void Start()
    {
        if (slider == null || origami == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        origami.SetFoldProgress(0f);
    }

    void Update()
    {
        if (slider == null || origami == null)
            return;

        if (Input.GetMouseButton(0) && slider.gameObject.activeInHierarchy)
            ConvertSliderToFoldProgress();
    }

    private void ConvertSliderToFoldProgress()
    {
        origami.SetFoldProgress(slider.value);
    }

    public void UpdateSliderValue()
    {
        if (slider == null || origami == null)
            return;

        slider.value = Mathf.Clamp01(origami.foldProgress);
    }
}
