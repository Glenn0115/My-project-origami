using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DesignModeManager : MonoBehaviour
{
    // 引用不同模式的控制器脚本
    public CreasePatternEditor creasePatternEditor;  // 折痕图设计功能
    public OrigamiController simulationController; // 模拟控制器，假设你已经有一个模拟功能
    public GameObject creasepatternUI;
    public GameObject SimulationUI;
    public TMP_Text consoleText;          // 文本框，用于显示状态信息
    public GameObject CreasePattern;
    public GameObject OrigamiSystem;



    private enum Mode { None, CreaseDesign, Simulation }
    private Mode currentMode = Mode.None;
    private bool isDesignMode = false;

    void Start()
    {
        // 默认进入折痕图设计模式
        isDesignMode = false;
        currentMode = Mode.Simulation;
        SwitchToSimulationMode();
        UpdateModeDisplay();
    }

    void Update()
    {
        /*// 切换模式的检查，可以通过UI按钮或者快捷键来实现
        if (Input.GetKeyDown(KeyCode.F1))  // 假设 F1 切换到折痕设计模式
        {
            SwitchToCreaseDesignMode();
            UpdateModeDisplay();
            isDesignMode = true;
            LogToConsole(isDesignMode ? "切换到折痕设计模式" : "切换到折叠模拟模式");

        }
        else if (Input.GetKeyDown(KeyCode.F2))  // 假设 F2 切换到模拟模式
        {
            SwitchToSimulationMode();
            UpdateModeDisplay();
            isDesignMode = false;
            LogToConsole(isDesignMode ? "切换到折痕设计模式" : "切换到折叠模拟模式");
        }*/
    }

    /// <summary>
    /// 按钮触发：切换到折痕设计模式
    /// </summary>
    public void SwitchToCreaseDesignModeButton()
    {
        isDesignMode = true;
        SwitchToCreaseDesignMode();
        UpdateModeDisplay();
        LogToConsole("切换到折痕设计模式");
        OrigamiSystem.SetActive(false);
        CreasePattern.SetActive(true);
    }

    /// <summary>
    /// 按钮触发：切换到折叠模拟模式
    /// </summary>
    public void SwitchToSimulationModeButton()
    {
        isDesignMode = false;
        SwitchToSimulationMode();
        UpdateModeDisplay();
        LogToConsole("切换到折叠模拟模式");
        OrigamiSystem?.SetActive(true);
        CreasePattern.SetActive(false);
    }


    void UpdateModeDisplay()
    {
        if (creasepatternUI != null)
            creasepatternUI.SetActive(isDesignMode);
        if (SimulationUI != null)
            SimulationUI.SetActive(!isDesignMode);
    }

    public int maxLogLines = 2; // 最大显示行数，可在Inspector调整

    public void LogToConsole(string message)
    {
        if (consoleText == null) return;

        // 1. 添加新消息（带时间戳）
        string newMessage = $"{System.DateTime.Now:HH:mm:ss} - {message}\n";
        consoleText.text += newMessage;

        // 2. 拆分所有行，限制最大行数
        string[] allLines = consoleText.text.Split('\n');
        if (allLines.Length > maxLogLines)
        {
            // 截取最新的 maxLogLines 行（跳过空行）
            List<string> newLines = new List<string>();
            for (int i = allLines.Length - maxLogLines; i < allLines.Length; i++)
            {
                if (!string.IsNullOrEmpty(allLines[i]))
                    newLines.Add(allLines[i]);
            }
            // 重新拼接文本（最后加一个换行符保持格式）
            consoleText.text = string.Join("\n", newLines) + "\n";
        }
    }

    // 切换到折痕图设计模式
    private void SwitchToCreaseDesignMode()
    {
        if (currentMode == Mode.CreaseDesign) return;
        currentMode = Mode.CreaseDesign;

        // 启用折痕图设计功能，禁用模拟功能
        if (creasePatternEditor != null)
        {
            //creasePatternEditor.gameObject.SetActive(true);  // 启用折痕图设计功能
            creasePatternEditor.enabled = true;
        }
        if (simulationController != null)
        {
            //simulationController.gameObject.SetActive(false);  // 禁用模拟功能
            simulationController.enabled = false;
        }

        Debug.Log("切换到折痕图设计模式");
    }

    // 切换到模拟模式
    private void SwitchToSimulationMode()
    {
        if (currentMode == Mode.Simulation) return;
        currentMode = Mode.Simulation;

        // 启用模拟功能，禁用折痕图设计功能
        if (creasePatternEditor != null)
        {
            //creasePatternEditor.gameObject.SetActive(false);  // 禁用折痕图设计功能
            creasePatternEditor.enabled = false;
        }
        if (simulationController != null)
        {
            //simulationController.gameObject.SetActive(true);  // 启用模拟功能
            simulationController.enabled = true;
        }

        Debug.Log("切换到模拟模式");
    }
}
