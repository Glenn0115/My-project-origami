using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 注意：移除了 UnityEditor 命名空间引用，避免运行时报错
public class OrigamiController : MonoBehaviour
{
    private List<HingeJoint> hinges = new List<HingeJoint>();
    public float foldSpeed = 30f;
    public float rotationSpeed = 50f;
    public bool enableKeyboardRotation = false;
    public List<HingeJoint> GetHinges() { return hinges; }


    [Header("折叠状态")]
    [Range(0f, 1f)]
    public float foldProgress = 0f;
    private float lastFoldProgress = 0f;

    // 修复：跨环境通用的关节有效性判断（无需 EditorUtility）
    // 原理：Unity 中销毁的对象实例ID为 0，且 == null 会返回 true（异步销毁后可能延迟，但结合实例ID可覆盖绝大多数情况）
    private static bool IsJointValid(HingeJoint joint)
    {
        // 双重判断：先判空，再检查实例ID（0 表示已销毁）
        return joint != null && joint.GetInstanceID() != 0;
    }

    void Start()
    {
        // 初始获取有效关节（过滤无效对象）
        var initialJoints = FindObjectsOfType<HingeJoint>().Where(IsJointValid).ToList();
        hinges.AddRange(initialJoints);
        Debug.Log($"初始找到 {initialJoints.Count} 个有效折痕铰链");
    }

    // 添加铰链方法（保留有效性和重复检查）
    public void AddHinge(HingeJoint hinge)
    {
        if (IsJointValid(hinge) && !hinges.Contains(hinge))
        {
            hinges.Add(hinge);
            Debug.Log($"添加新铰链，当前有效铰链数：{hinges.Count}");
        }
        else
        {
            Debug.LogWarning("尝试添加无效或重复的铰链，已忽略");
        }
    }

    void Update()
    {
        // 过滤无效关节（每次Update前清理，确保遍历的都是有效对象）
        FilterInvalidJoints();

        // 仅当有有效关节时执行控制逻辑
        if (hinges.Count > 0)
        {
            HandleFoldInput();
            HandleRotationInput();
            SyncFoldProgressWithSlider();
        }
    }

    // 清理无效关节（核心修复：基于实例ID判断）
    private void FilterInvalidJoints()
    {
        int invalidCount = hinges.RemoveAll(joint => !IsJointValid(joint));
        if (invalidCount > 0)
        {
            Debug.Log($"清理了 {invalidCount} 个无效铰链");
        }
    }

    // 处理折叠输入
    private void HandleFoldInput()
    {
        if (Input.GetKey(KeyCode.UpArrow))
            AdjustFold(+foldSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.DownArrow))
            AdjustFold(-foldSpeed * Time.deltaTime);
    }

    // 处理旋转输入
    private void HandleRotationInput()
    {
        if (!enableKeyboardRotation)
            return;

        if (Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    // 同步滑动条和折叠进度
    private void SyncFoldProgressWithSlider()
    {
        if (Mathf.Abs(foldProgress - lastFoldProgress) > 0.001f)
        {
            SetFoldProgress(foldProgress);
            lastFoldProgress = foldProgress;
        }
    }

    // 调整折叠角度
    void AdjustFold(float delta)
    {
        foreach (var hinge in hinges)
        {
            JointSpring spring = hinge.spring;
            spring.targetPosition = Mathf.Clamp(spring.targetPosition + delta, hinge.limits.min, hinge.limits.max);
            hinge.spring = spring;
        }

        UpdateFoldProgress();
    }

    // 设置折叠进度 (0-1)
    public void SetFoldProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        foreach (var hinge in hinges)
        {
            if (!IsJointValid(hinge)) continue; // 双重保险

            JointSpring spring = hinge.spring;
            float angleRange = hinge.limits.max - hinge.limits.min;
            if (angleRange < 0.001f)
            {
                spring.targetPosition = hinge.limits.min;
            }
            else
            {
                spring.targetPosition = hinge.limits.min + angleRange * progress;
            }
            hinge.spring = spring;
        }
    }

    // 更新折叠进度值
    private void UpdateFoldProgress()
    {
        if (hinges.Count == 0) return;

        float totalProgress = 0f;
        int validJointCount = 0;

        foreach (var hinge in hinges)
        {
            if (!IsJointValid(hinge)) continue;

            float angleRange = hinge.limits.max - hinge.limits.min;
            if (angleRange > 0.001f)
            {
                float jointProgress = (hinge.spring.targetPosition - hinge.limits.min) / angleRange;
                totalProgress += Mathf.Clamp01(jointProgress);
                validJointCount++;
            }
        }

        foldProgress = validJointCount > 0 ? totalProgress / validJointCount : 0f;
        lastFoldProgress = foldProgress;
    }

    // 供OrigamiLoader调用，清空所有铰链引用
    public void ClearAllHinges()
    {
        hinges.Clear();
        Debug.Log("OrigamiController：已清空所有铰链引用");
    }

    // 获取当前有效铰链数量（供调试）
    public int GetValidHingeCount()
    {
        FilterInvalidJoints();
        return hinges.Count;
    }
}
