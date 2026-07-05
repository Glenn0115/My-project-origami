using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GridCameraLink : MonoBehaviour
{
    public Material gridMaterial; // 拖入你创建的 LargeGridMat 材质

    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (gridMaterial == null)
        {
            Debug.LogError("Grid Material is not assigned in GridCameraLink!");
            enabled = false;
        }
    }

    void Update()
    {
        if (gridMaterial != null)
        {
            // 将相机的投影矩阵和世界矩阵传递给Shader
            gridMaterial.SetMatrix("_CamProj", mainCamera.projectionMatrix);
            gridMaterial.SetMatrix("_CamWorld", mainCamera.worldToCameraMatrix);
        }
    }
}