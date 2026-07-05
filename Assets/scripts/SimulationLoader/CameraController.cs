using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float fastMoveMultiplier = 2f;
    public KeyCode fastMoveKey = KeyCode.LeftShift;

    [Header("旋转设置")]
    public float rotationSpeed = 3f;
    public MouseButton rotateButton = MouseButton.Right; // 右键旋转

    [Header("缩放设置")]
    public float zoomSpeed = 10f;
    public float minZoomDistance = 1f;
    public float maxZoomDistance = 20f;

    [Header("平移设置")]
    public float panSpeed = 5f;
    public MouseButton panButton = MouseButton.Middle; // 中键平移（移除panKey，直接中键拖动）

    [Header("重置设置")]
    public KeyCode resetKey = KeyCode.Home;
    public Button resetButton;
    public Vector3 defaultPosition = new Vector3(0, 5, -10);
    public Vector3 defaultRotation = new Vector3(30, 0, 0);

    // 枚举鼠标按键
    public enum MouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2
    }

    // 内部变量
    private Vector3 lastMousePosition;
    private Transform cameraTransform;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float currentDistance;
    private Vector3 pivotPoint = Vector3.zero;

    void Start()
    {
        cameraTransform = transform;
        targetPosition = cameraTransform.position;
        targetRotation = cameraTransform.rotation;
        currentDistance = Vector3.Distance(cameraTransform.position, pivotPoint);

        // 禁用光标锁定，以便用户可以与UI交互
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 关键修改：绑定UI按钮点击事件
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetCamera);
        }

    }

    void Update()
    {
        // 获取鼠标位置
        Vector3 mousePosition = Input.mousePosition;

        // 处理键盘移动
        HandleKeyboardMovement();

        // 处理旋转（右键拖动）
        HandleRotation(mousePosition);

        // 处理缩放（鼠标滚轮）
        HandleZoom();

        // 处理平移（中键拖动）
        HandlePanning(mousePosition);

        // 处理重置
        //HandleReset();

        // 保存上一帧的鼠标位置
        lastMousePosition = mousePosition;
    }

    void HandleKeyboardMovement()
    {
        // 检查是否按下快速移动键
        float speedMultiplier = Input.GetKey(fastMoveKey) ? fastMoveMultiplier : 1f;
        float finalSpeed = moveSpeed * speedMultiplier * Time.deltaTime;

        // WASD键移动
        if (Input.GetKey(KeyCode.W))
            targetPosition += cameraTransform.forward * finalSpeed;
        if (Input.GetKey(KeyCode.S))
            targetPosition -= cameraTransform.forward * finalSpeed;
        if (Input.GetKey(KeyCode.A))
            targetPosition -= cameraTransform.right * finalSpeed;
        if (Input.GetKey(KeyCode.D))
            targetPosition += cameraTransform.right * finalSpeed;

        // QE键上下移动
        if (Input.GetKey(KeyCode.Q))
            targetPosition -= Vector3.up * finalSpeed;
        if (Input.GetKey(KeyCode.E))
            targetPosition += Vector3.up * finalSpeed;

        // 平滑移动到目标位置
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.deltaTime * 10f);
    }

    void HandleRotation(Vector3 mousePosition)
    {
        // 检查是否按下右键（rotateButton）
        if (Input.GetMouseButton((int)rotateButton))
        {
            // 计算鼠标移动差值
            float deltaX = (mousePosition.x - lastMousePosition.x) * rotationSpeed * 0.1f;
            float deltaY = (mousePosition.y - lastMousePosition.y) * rotationSpeed * 0.1f;

            // 更新目标旋转
            Vector3 eulerAngles = targetRotation.eulerAngles;
            eulerAngles.y += deltaX;
            eulerAngles.x -= deltaY;

            // 限制垂直旋转角度
            if (eulerAngles.x > 180f)
                eulerAngles.x = Mathf.Clamp(eulerAngles.x, 270f, 360f);
            else
                eulerAngles.x = Mathf.Clamp(eulerAngles.x, 0f, 90f);

            targetRotation = Quaternion.Euler(eulerAngles);
        }

        // 平滑旋转到目标方向
        cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    void HandleZoom()
    {
        // 获取鼠标滚轮输入（中键滚轮）
        float scrollDelta = Input.mouseScrollDelta.y;

        if (scrollDelta != 0)
        {
            // 更新当前距离
            currentDistance -= scrollDelta * zoomSpeed * Time.deltaTime * 10f;
            currentDistance = Mathf.Clamp(currentDistance, minZoomDistance, maxZoomDistance);

            // 更新目标位置
            targetPosition = pivotPoint - cameraTransform.forward * currentDistance;
        }
    }

    void HandlePanning(Vector3 mousePosition)
    {
        // 检查是否按下中键（panButton）
        bool isPanning = Input.GetMouseButton((int)panButton);

        if (isPanning)
        {
            // 计算鼠标移动差值
            float deltaX = (mousePosition.x - lastMousePosition.x) * panSpeed * 0.003f;
            float deltaY = (mousePosition.y - lastMousePosition.y) * panSpeed * 0.003f;

            // 更新目标位置
            targetPosition -= cameraTransform.right * deltaX * currentDistance;
            targetPosition -= cameraTransform.up * deltaY * currentDistance;

            // 更新枢轴点
            pivotPoint -= cameraTransform.right * deltaX * currentDistance;
            pivotPoint -= cameraTransform.up * deltaY * currentDistance;
        }
    }

    void ResetCamera()
    {
        // 1. 重置到默认位置
        targetPosition = defaultPosition;
        targetRotation = Quaternion.Euler(defaultRotation);

        // 2. 重置缩放距离
        currentDistance = Vector3.Distance(defaultPosition, pivotPoint);

        // 3. 重置枢轴点（保持在原点）
        pivotPoint = Vector3.zero;

        // 4. 立即应用（避免等待帧）
        cameraTransform.position = targetPosition;
        cameraTransform.rotation = targetRotation;

        Debug.Log("[相机] 重置成功！回到默认视角");
    }
}