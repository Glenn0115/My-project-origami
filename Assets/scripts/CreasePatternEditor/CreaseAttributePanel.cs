using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 折痕属性弹窗 UI 逻辑
public class CreaseAttributePanel : MonoBehaviour
{
    [Header("UI 组件绑定")]
    public GameObject panelRoot;
    public Toggle mountainToggle;
    public Toggle valleyToggle;
    public Toggle boundaryToggle;
    public TMP_InputField minAngleInput;   // ← 新增
    public TMP_InputField maxAngleInput;   // ← 新增
    // public TMP_InputField angleInput;   // ← 可以移除或保留（建议移除）
    public Button applyButton;
    public Button cancelButton;

    private int _currentCreaseId = -1;
    private Action<CreasePatternEditor.CreaseType, float, float> _onApply;

    /// <summary>
    /// 打开属性面板
    /// </summary>
    public void Show(
        int creaseId,
        CreasePatternEditor.OrigamiCrease crease,
        System.Action<CreasePatternEditor.CreaseType, float, float> onApply)
    {
        _currentCreaseId = creaseId;
        _onApply = onApply; // ← 现在类型一致了

        // 初始化 UI
        mountainToggle.isOn = crease.creaseType == CreasePatternEditor.CreaseType.Mountain;
        valleyToggle.isOn = crease.creaseType == CreasePatternEditor.CreaseType.Valley;
        boundaryToggle.isOn = crease.creaseType == CreasePatternEditor.CreaseType.Boundary;

        // 👇 使用 minAngle / maxAngle
        minAngleInput.text = Mathf.Round(crease.minAngle).ToString();
        maxAngleInput.text = Mathf.Round(crease.maxAngle).ToString();

        // 绑定按钮
        applyButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        applyButton.onClick.AddListener(OnApplyClicked);
        cancelButton.onClick.AddListener(Hide);

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        _currentCreaseId = -1;
        _onApply = null;
        panelRoot.SetActive(false);
    }

    private void OnApplyClicked()
    {
        var type =
            boundaryToggle.isOn ? CreasePatternEditor.CreaseType.Boundary :
            mountainToggle.isOn ? CreasePatternEditor.CreaseType.Mountain :
            CreasePatternEditor.CreaseType.Valley;

        float min = 0f, max = 0f;
        float.TryParse(minAngleInput.text, out min);
        float.TryParse(maxAngleInput.text, out max);

        _onApply?.Invoke(type, min, max); // ← 传两个角度

        Hide();
    }
}
