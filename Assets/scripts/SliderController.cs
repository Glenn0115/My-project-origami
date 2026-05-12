using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider; // 在Inspector里拖入你的Slider组件
    public OrigamiController origami; // 拖入OrigamiController

    void Start()
    {
        // 起始状态：滑动条中点（0.5）对应0角度
        slider.value = 0.5f;
        UpdateSliderValue(); // 确保初始值正确
    }

    void Update()
    {
        // 当滑动条拖动时，自动转换到折叠进度
        if (Input.GetMouseButton(0) && slider.gameObject.activeInHierarchy)
        {
            ConvertSliderToFoldProgress();
        }
    }

    // 核心转换函数：将滑动条值 → 折叠进度
    private void ConvertSliderToFoldProgress()
    {
        // ✅ 修复1：使用OrigamiController的公共方法获取铰链（不再直接访问hinges）
        if (origami.GetValidHingeCount() == 0) return;

        // ✅ 修复2：使用GetHinges方法（需要在OrigamiController中添加这个方法）
        var hinges = origami.GetHinges();
        HingeJoint hinge = hinges[0]; // 取第一个铰链的限制值（假设所有铰链限制相同）

        float min = hinge.limits.min;
        float max = hinge.limits.max;
        float angleRange = max - min;

        // ✅ 关键计算：滑动条值 → 折叠进度
        float foldProgress = (slider.value - 0.5f) * 2f + (0f - min) / angleRange;
        foldProgress = Mathf.Clamp01(foldProgress);

        origami.SetFoldProgress(foldProgress);
    }

    // 用于自动同步控制器状态到滑动条
    public void UpdateSliderValue()
    {
        if (origami.GetValidHingeCount() == 0) return;

        var hinges = origami.GetHinges();
        HingeJoint hinge = hinges[0];

        float min = hinge.limits.min;
        float max = hinge.limits.max;
        float angleRange = max - min;

        // ✅ 关键计算：折叠进度 → 滑动条值
        float sliderValue = 0.5f + (origami.foldProgress - (0f - min) / angleRange) / 2f;
        slider.value = Mathf.Clamp01(sliderValue);
    }
}