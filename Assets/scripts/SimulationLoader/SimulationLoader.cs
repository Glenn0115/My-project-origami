using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ���ؿ��������ṩUI���棬���û������ļ�·��������OrigamiLoader����ģ�͡�
/// </summary>
public class SimulationLoader: MonoBehaviour
{
    [Header("UI Ԫ��")]
    [Tooltip("���������ļ�·���������")]
    public TMP_InputField pathInputField;

    [Tooltip("�������صİ�ť")]
    public Button loadButton;

    [Tooltip("��ʾ����״̬��Ϣ���ı�")]
    public TextMeshProUGUI statusText;

    [Header("ģ�ͼ�����")]
    [Tooltip("�����и���ʵ�ʼ���ģ�͵�OrigamiLoaderʵ��")]
    public OrigamiLoader origamiLoader;

    [Header("DXF ����·��")]
    [Tooltip("Auto�������ۺ�ͼ�߾ɶ�άת�������ǹ���ռ��������¿ռ�ת������")]
    public DxfImportMode dxfImportMode = DxfImportMode.Auto;

    [Min(0.000001f)]
    [Tooltip("DXF �˵�ƫ��ͬһƽ�������ݲ����������Ϊ�ռ� DXF ���롣")]
    public float dxfCoplanarityTolerance = Dxf3DToOrigamiConverter.DefaultCoplanarityTolerance;

    [Tooltip("��ά DXF ���� JSON ��Ŀ¼������ʱʹ�� Assets/Models/Dxf2D��")]
    public string flatDxfOutputDirectory = "";

    [Tooltip("�ռ� DXF ���� JSON ��Ŀ¼������ʱʹ�� Assets/Models/Spatial��")]
    public string spatialDxfOutputDirectory = "";

    [Tooltip("Rhino/DXF ͨ���� Z �����ϣ�Unity �� Y �����ϡ�")]
    public bool rhinoZUpToUnityYUp = true;

    [Tooltip("ת����ģ�Ͱ�Χ�����ķŵ� Unity ԭ�㡣")]
    public bool centerSpatialDxfAtOrigin = true;

    private void Awake()
    {
        // �Զ����ҳ����е�OrigamiLoader�����û���ֶ�ָ���Ļ�
        if (origamiLoader == null)
        {
            origamiLoader = FindObjectOfType<OrigamiLoader>();
            if (origamiLoader == null)
            {
                SetStatus("���󣺳�����δ�ҵ� OrigamiLoader ʵ����", Color.red);
                Debug.LogError("LoaderController: �޷��ҵ� OrigamiLoader �����");
                // ���ð�ť����ֹ����
                if (loadButton != null)
                    loadButton.interactable = false;
            }
        }
    }

    private void Start()
    {
        // �󶨰�ť�ĵ���¼�
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadButtonClicked);
        }

        // ����Ĭ��״̬
        if (origamiLoader != null)
        {
            SetStatus("������ģ��·�����������", Color.white);
            // ���Խ�OrigamiLoader��Ĭ��·�����õ��������
            if (pathInputField != null && !string.IsNullOrEmpty(origamiLoader.jsonPath))
            {
                pathInputField.text = origamiLoader.jsonPath;
            }
        }
    }

    /// <summary>
    /// �����ذ�ť�����ʱ����
    /// </summary>
    private void OnLoadButtonClicked()
    {
        LoadCurrentInput(dxfImportMode);
    }

    /// <summary>��δ�����Զ� DXF����ť���á�</summary>
    public void LoadCurrentInputWithAutoDxfRouting()
    {
        LoadCurrentInput(DxfImportMode.Auto);
    }

    /// <summary>��δ������ά DXF����ť���á��ǹ�������ᰲȫ�ظ���������ʾ��</summary>
    public void LoadCurrentInputAs2DDxf()
    {
        LoadCurrentInput(DxfImportMode.Flat2D);
    }

    /// <summary>��δ�����ռ� DXF����ť���á�</summary>
    public void LoadCurrentInputAs3DDxf()
    {
        LoadCurrentInput(DxfImportMode.Spatial3D);
    }

    private void LoadCurrentInput(DxfImportMode requestedDxfMode)
    {
        if (origamiLoader == null)
        {
            SetStatus("����OrigamiLoader δ������", Color.red);
            return;
        }

        if (pathInputField == null || string.IsNullOrEmpty(pathInputField.text))
        {
            SetStatus("������������Ч���ļ�·����", Color.red);
            return;
        }

        string userPath = pathInputField.text.Trim();

        // DXF ͨ������·�����Զ�/ǿ��ѡ��ɶ�ά���¿ռ�ת������JSON ԭ�����ء�
        if (userPath.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase))
        {
            ImportDxfAndLoad(userPath, requestedDxfMode);
            return;
        }

        // �����������·����֤�߼���������·����ʽ��
        // ע�⣺OrigamiLoader.LoadModel�Ὣ·����Application.dataPath���
        // �����û�Ӧ���������Assets��·�������� "Models/miura0.json"

        SetStatus($"���ڼ���: {userPath} ...", Color.yellow);

        // ����OrigamiLoader�ļ��ط���
        origamiLoader.LoadModel(userPath);

        // ��ʾ������ɣ�ʵ�ʳɹ����ȡ����OrigamiLoader�ڲ���
        SetStatus($"����ָ���ѷ���: {userPath}", Color.green);
    }

    private void ImportDxfAndLoad(string userPath, DxfImportMode requestedMode)
    {
        string sourcePath = ResolveInputPath(userPath);
        if (!File.Exists(sourcePath))
        {
            SetStatus($"δ�ҵ� DXF �ļ�: {sourcePath}", Color.red);
            return;
        }

        SetStatus($"���ڷ��� DXF: {Path.GetFileName(sourcePath)} ...", Color.yellow);

        DxfImportOptions options = new DxfImportOptions
        {
            mode = requestedMode,
            flatOutputDirectory = ResolveDxfOutputDirectory(flatDxfOutputDirectory),
            spatialOutputDirectory = ResolveDxfOutputDirectory(spatialDxfOutputDirectory),
            coplanarityTolerance = dxfCoplanarityTolerance,
            spatialOptions = new SpatialDxfImportOptions
            {
                rhinoZUpToUnityYUp = rhinoZUpToUnityYUp,
                centerAtOrigin = centerSpatialDxfAtOrigin,
                // �ռ� DXF ���Ѿ��ۺõĿ��գ���ר�� Kresling ����������ǰ��
                // ���þɵ�ͳһ foldProgress �����������н�����
                markImportedCreasesPassive = true
            }
        };

        DxfImportResult result = DxfImportRouter.Convert(sourcePath, options);

        if (!result.success)
        {
            SetStatus($"DXF ����ʧ��: {result.error}", Color.red);
            LogDxfWarnings(result);
            return;
        }

        origamiLoader.LoadModel(result.jsonPath);
        string warningSuffix = result.warnings.Count > 0 ? $"��{result.warnings.Count} �����棬��� Console��" : "";
        string routeLabel = result.resolvedMode == DxfImportMode.Flat2D ? "��ά DXF" : "�ռ� DXF";
        SetStatus($"{routeLabel} �Ѽ���: {Path.GetFileName(result.jsonPath)}{warningSuffix}", Color.green);
        LogDxfWarnings(result);
    }

    private static void LogDxfWarnings(DxfImportResult result)
    {
        for (int i = 0; i < result.warnings.Count; i++)
            Debug.LogWarning($"[DxfImport] {result.warnings[i]}");
    }

    private static string ResolveInputPath(string path)
    {
        if (Path.IsPathRooted(path))
            return path;

        bool hasSeparator = path.IndexOfAny(new[] { '/', '\\' }) >= 0;
        if (!hasSeparator)
            return Path.Combine(Application.dataPath, "Models", path);

        string normalized = path.Replace('\\', '/');
        if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            normalized = normalized.Substring("Assets/".Length);

        return Path.Combine(Application.dataPath, normalized);
    }

    private static string ResolveDxfOutputDirectory(string configuredDirectory)
    {
        if (string.IsNullOrWhiteSpace(configuredDirectory))
            return null;

        if (Path.IsPathRooted(configuredDirectory))
            return configuredDirectory;

        string normalized = configuredDirectory.Replace('\\', '/');
        if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            normalized = normalized.Substring("Assets/".Length);

        return Path.Combine(Application.dataPath, normalized);
    }

    /// <summary>
    /// ����״̬�ı�
    /// </summary>
    /// <param name="message">Ҫ��ʾ����Ϣ</param>
    /// <param name="color">��Ϣ��ɫ</param>
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
    /// ����ѡ���ṩһ�������������������ű�Ҳ��ͨ��������������������
    /// </summary>
    /// <param name="path">ģ���ļ�·��</param>
    public void LoadModelFromPath(string path)
    {
        if (pathInputField != null)
        {
            pathInputField.text = path;
        }
        OnLoadButtonClicked();
    }
}