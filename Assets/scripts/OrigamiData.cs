using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrigamiVertex
{
    public float x, y, z;
}

[System.Serializable]
public class OrigamiFace
{
    public int id;
    public List<int> vertices;
    public bool rigid;
    
    // 新增颜色属性
    public string color = "#FFFFFF";
    
    // 获取Unity颜色
    public Color GetUnityColor()
    {
        if (ColorUtility.TryParseHtmlString(color, out Color result))
            return result;
        return Color.white;
    }
}

[System.Serializable]
public class OrigamiCrease
{
    public int id;
    public int v1;
    public int v2;
    public enum Type
    {
        Valley,     // 谷折（凹）
        Mountain,   // 山折（凸）
        Boundary,   // 边界（不可折叠）

    }
    public Type type;
    public float restAngle;
    public float minAngle;
    public float maxAngle;
    public float stiffness;
    
    // 新增折痕宽度
    public float width = 0.02f;
}

[System.Serializable]
public class OrigamiConnection
{
    public int crease_id;
    public int faceA;
    public int faceB;
}

[System.Serializable]
public class OrigamiMaterial
{
    public string name = "Default";
    public float metallic = 0f;
    public float smoothness = 0.5f;
    public bool doubleSided = true;
}

[System.Serializable]
public class OrigamiModel
{
    public string name = "Origami Model";
    public string description = "";
    public List<OrigamiVertex> vertices;
    public List<OrigamiFace> faces;
    public List<OrigamiCrease> creases;
    public List<OrigamiConnection> connections;
    public OrigamiMaterial material = new OrigamiMaterial();
    
    // 全局设置
    public float defaultCreaseWidth = 0.02f;
    public string mountainCreaseColor = "#FF0000";
    public string valleyCreaseColor = "#0000FF";
    public string boundaryCreaseColor = "#000000";
}
