using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CreaseAttributeEditor
{
    private readonly List<CreasePatternEditor.OrigamiCrease> _creases;
    private readonly List<LineRenderer> _lineRenderers; // ← 缓存组件
    private readonly Material _mountainMat;
    private readonly Material _valleyMat;
    private readonly Material _boundaryMat;
    private readonly Material _flatMat;
    private readonly float _baseCreaseWidth;

    public CreaseAttributeEditor(
        List<CreasePatternEditor.OrigamiCrease> creases,
        List<GameObject> creaseObjects,
        Material mountainMat,
        Material valleyMat,
        Material boundaryMat,
        Material flatMat,
        float baseCreaseWidth)
    {
        _creases = creases;
        _lineRenderers = creaseObjects.Select(go => go.GetComponent<LineRenderer>()).ToList();
        _mountainMat = mountainMat;
        _valleyMat = valleyMat;
        _boundaryMat = boundaryMat;
        _flatMat = flatMat;
        _baseCreaseWidth = baseCreaseWidth;
    }

    public bool UpdateCreaseType(int creaseId, CreasePatternEditor.CreaseType type)
    {
        var crease = _creases.FirstOrDefault(c => c.id == creaseId);
        if (crease == null) return false;

        crease.creaseType = type; // ← 移除冗余 cast

        int index = _creases.IndexOf(crease);
        if (index < 0 || index >= _lineRenderers.Count) return true;

        var lr = _lineRenderers[index];
        if (lr == null) return true;

        // 设置材质
        lr.material = type switch
        {
            CreasePatternEditor.CreaseType.Mountain => _mountainMat,
            CreasePatternEditor.CreaseType.Valley => _valleyMat,
            CreasePatternEditor.CreaseType.Boundary => _boundaryMat,
            _ => _flatMat // 包括 Flat 和未知类型
        };

        // 设置线宽
        float width = _baseCreaseWidth;
        if (type == CreasePatternEditor.CreaseType.Boundary)
            width *= 1.5f;
        // 可选：else if (type == CreasePatternEditor.CreaseType.Flat) width *= 0.7f;

        lr.startWidth = lr.endWidth = width;

        return true;
    }

    public bool UpdateCreaseAngle(int creaseId, float newAngle)
    {
        var crease = _creases.FirstOrDefault(c => c.id == creaseId);
        if (crease == null) return false;
        crease.angle = newAngle;
        return true;
    }

    // 新增：支持 min/max 角度
    public bool UpdateCreaseAngles(int creaseId, float? minAngle = null, float? maxAngle = null)
    {
        var crease = _creases.FirstOrDefault(c => c.id == creaseId);
        if (crease == null) return false;
        if (minAngle.HasValue) crease.minAngle = minAngle.Value;
        if (maxAngle.HasValue) crease.maxAngle = maxAngle.Value;
        return true;
    }

    // 扩展的便捷方法
    public bool UpdateAttributes(
        int creaseId,
        CreasePatternEditor.CreaseType? type = null,
        float? angle = null,
        float? minAngle = null,
        float? maxAngle = null)
    {
        bool ok = true;
        if (type.HasValue) ok &= UpdateCreaseType(creaseId, type.Value);
        if (angle.HasValue) ok &= UpdateCreaseAngle(creaseId, angle.Value);
        if (minAngle.HasValue || maxAngle.HasValue)
            ok &= UpdateCreaseAngles(creaseId, minAngle, maxAngle);
        return ok;
    }
}