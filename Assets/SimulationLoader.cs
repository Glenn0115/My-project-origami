using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 加载控制器：提供UI界面，让用户输入文件路径并调用OrigamiLoader加载模型。
/// </summary>
public class SimulationLoader: MonoBehaviour
{
    [Header("UI 元素")]
    [Tooltip("用于输入文件路径的输入框")]
    public TMP_InputField pathInputField;

    [Tooltip("触发加载的按钮")]
    public Button loadButton;

    [Tooltip("显示加载状态信息的文本")]
    public TextMeshProUGUI statusText;

    [Header("模型加载器")]
    [Tooltip("场景中负责实际加载模型的OrigamiLoader实例")]
    public OrigamiLoader origamiLoader;

    private void Awake()
    {
        // 自动查找场景中的OrigamiLoader，如果没有手动指定的话
        if (origamiLoader == null)
        {
            origamiLoader = FindObjectOfType<OrigamiLoader>();
            if (origamiLoader == null)
            {
                SetStatus("错误：场景中未找到 OrigamiLoader 实例！", Color.red);
                Debug.LogError("LoaderController: 无法找到 OrigamiLoader 组件。");
                // 禁用按钮，防止报错
                if (loadButton != null)
                    loadButton.interactable = false;
            }
        }
    }

    private void Start()
    {
        // 绑定按钮的点击事件
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadButtonClicked);
        }

        // 设置默认状态
        if (origamiLoader != null)
        {
            SetStatus("请输入模型路径并点击加载", Color.white);
            // 可以将OrigamiLoader的默认路径设置到输入框中
            if (pathInputField != null && !string.IsNullOrEmpty(origamiLoader.jsonPath))
            {
                pathInputField.text = origamiLoader.jsonPath;
            }
        }
    }

    /// <summary>
    /// 当加载按钮被点击时调用
    /// </summary>
    private void OnLoadButtonClicked()
    {
        if (origamiLoader == null)
        {
            SetStatus("错误：OrigamiLoader 未就绪！", Color.red);
            return;
        }

        if (pathInputField == null || string.IsNullOrEmpty(pathInputField.text))
        {
            SetStatus("错误：请输入有效的文件路径！", Color.red);
            return;
        }

        string userPath = pathInputField.text.Trim();

        // 这里可以添加路径验证逻辑，比如检查路径格式等
        // 注意：OrigamiLoader.LoadModel会将路径与Application.dataPath结合
        // 所以用户应该输入相对Assets的路径，例如 "Models/miura0.json"

        SetStatus($"正在加载: {userPath} ...", Color.yellow);

        // 调用OrigamiLoader的加载方法
        origamiLoader.LoadModel(userPath);

        // 提示加载完成（实际成功与否取决于OrigamiLoader内部）
        SetStatus($"加载指令已发出: {userPath}", Color.green);
    }

    /// <summary>
    /// 更新状态文本
    /// </summary>
    /// <param name="message">要显示的消息</param>
    /// <param name="color">消息颜色</param>
    private void SetStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        Debug.Log(message);
    }

    /// <summary>
    /// （可选）提供一个公共方法，让其他脚本也能通过代码调用这个加载流程
    /// </summary>
    /// <param name="path">模型文件路径</param>
    public void LoadModelFromPath(string path)
    {
        if (pathInputField != null)
        {
            pathInputField.text = path;
        }
        OnLoadButtonClicked();
    }
}