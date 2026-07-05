using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreaseAttributePanel : MonoBehaviour
{
    [Header("UI Bindings")]
    public GameObject panelRoot;
    public Toggle mountainToggle;
    public Toggle valleyToggle;
    public Toggle boundaryToggle;
    public TMP_InputField minAngleInput;
    public TMP_InputField maxAngleInput;
    public Button applyButton;
    public Button cancelButton;

    private int _currentCreaseId = -1;
    private Action<CreasePatternEditor.CreaseType, float, float> _onApply;
    private Action _onCancel;
    private float _currentMinAngle;
    private float _currentMaxAngle;

    public void Show(
        int creaseId,
        CreasePatternEditor.OrigamiCrease crease,
        Action<CreasePatternEditor.CreaseType, float, float> onApply,
        Action onCancel = null)
    {
        if (crease == null)
        {
            Debug.LogError("[CreaseAttributePanel] Cannot show panel for a null crease.");
            return;
        }

        if (!ValidateBindings())
            return;

        _currentCreaseId = creaseId;
        _onApply = onApply;
        _onCancel = onCancel;
        _currentMinAngle = crease.minAngle;
        _currentMaxAngle = crease.maxAngle;

        mountainToggle.isOn = crease.creaseType == CreasePatternEditor.CreaseType.Mountain;
        valleyToggle.isOn = crease.creaseType == CreasePatternEditor.CreaseType.Valley;
        boundaryToggle.isOn = crease.creaseType == CreasePatternEditor.CreaseType.Boundary;

        minAngleInput.text = Mathf.Round(crease.minAngle).ToString(CultureInfo.InvariantCulture);
        maxAngleInput.text = Mathf.Round(crease.maxAngle).ToString(CultureInfo.InvariantCulture);

        applyButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        applyButton.onClick.AddListener(OnApplyClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        _currentCreaseId = -1;
        _onApply = null;
        _onCancel = null;
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnCancelClicked()
    {
        _onCancel?.Invoke();
        Hide();
    }

    private void OnApplyClicked()
    {
        if (!ValidateBindings())
            return;

        var type =
            boundaryToggle.isOn ? CreasePatternEditor.CreaseType.Boundary :
            mountainToggle.isOn ? CreasePatternEditor.CreaseType.Mountain :
            CreasePatternEditor.CreaseType.Valley;

        float min = TryReadAngleInput(minAngleInput, _currentMinAngle, "minAngle");
        float max = TryReadAngleInput(maxAngleInput, _currentMaxAngle, "maxAngle");

        if (min > max)
        {
            Debug.LogWarning("[CreaseAttributePanel] minAngle cannot be greater than maxAngle; keeping the previous values.");
            min = _currentMinAngle;
            max = _currentMaxAngle;
        }

        _onApply?.Invoke(type, min, max);
        Hide();
    }

    private bool ValidateBindings()
    {
        bool valid =
            panelRoot != null &&
            mountainToggle != null &&
            valleyToggle != null &&
            boundaryToggle != null &&
            minAngleInput != null &&
            maxAngleInput != null &&
            applyButton != null &&
            cancelButton != null;

        if (!valid)
        {
            Debug.LogError("[CreaseAttributePanel] Missing UI binding. Assign panelRoot, type toggles, min/max TMP_InputFields, and apply/cancel buttons in the Inspector.");
        }

        return valid;
    }

    private float TryReadAngleInput(TMP_InputField input, float fallback, string fieldName)
    {
        if (!float.TryParse(input.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            Debug.LogWarning($"[CreaseAttributePanel] Invalid {fieldName}; keeping previous value {fallback}.");
            return fallback;
        }

        if (value < 0f || value > 180f)
        {
            float clamped = Mathf.Clamp(value, 0f, 180f);
            Debug.LogWarning($"[CreaseAttributePanel] {fieldName} must be between 0 and 180; using {clamped}.");
            return clamped;
        }

        return value;
    }
}
