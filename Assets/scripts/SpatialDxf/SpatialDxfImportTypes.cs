using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Settings for importing Rhino-style 3D DXF crease curves.
/// They deliberately live outside the 2D converter so its established behaviour stays unchanged.
/// </summary>
[Serializable]
public sealed class SpatialDxfImportOptions
{
    [Min(0.000001f)] public float vertexMergeTolerance = 0.01f;
    [Min(0.0000001f)] public float minimumTriangleArea = 0.0001f;

    [Tooltip("Move the imported model's bounding-box centre to Unity's origin.")]
    public bool centerAtOrigin = true;

    [Tooltip("Convert Rhino/DXF Z-up coordinates into Unity's Y-up coordinates without flattening them.")]
    public bool rhinoZUpToUnityYUp = true;

    [Tooltip("Keep imported M/V joints passive. Turn this off to let the existing global fold slider drive them.")]
    public bool markImportedCreasesPassive = true;

    [Range(0f, 180f)]
    [Tooltip("When imported M/V joints use the existing fold slider, this is the target angle at slider value 1. Slider value 0 always keeps the imported pose.")]
    public float sliderDriveTravelAngle = 180f;
}

/// <summary>
/// A compact result object used by the UI to tell the user what was imported and why it failed.
/// </summary>
public sealed class SpatialDxfImportResult
{
    public bool success;
    public string jsonPath;
    public string error;
    public int parsedEdgeCount;
    public int vertexCount;
    public int faceCount;
    public int mountainCount;
    public int valleyCount;
    public int boundaryCount;
    public readonly List<string> warnings = new List<string>();

    public string Summary
    {
        get
        {
            return $"vertices={vertexCount}, faces={faceCount}, " +
                   $"M={mountainCount}, V={valleyCount}, B={boundaryCount}";
        }
    }
}

/// <summary>
/// The route used to turn a DXF curve network into an OrigamiModel JSON file.
/// Auto examines the curve coordinates after DXF OCS coordinates have been
/// converted to WCS, so an inclined planar drawing still goes through Flat2D.
/// </summary>
public enum DxfImportMode
{
    Auto,
    Flat2D,
    Spatial3D
}

/// <summary>
/// One curve segment expressed in DXF WCS coordinates.  This is deliberately
/// shared by the source analyser and the spatial converter, so both routes
/// classify the exact same LINE / POLYLINE / LWPOLYLINE data.
/// </summary>
public struct DxfCurveSegment
{
    public Vector3 start;
    public Vector3 end;
    public uint rgbColor;

    public DxfCurveSegment(Vector3 start, Vector3 end, uint rgbColor)
    {
        this.start = start;
        this.end = end;
        this.rgbColor = rgbColor;
    }
}

/// <summary>
/// Diagnostics from reading a DXF curve network.  Plane values are expressed
/// in DXF WCS, before the optional Rhino Z-up to Unity Y-up rotation.
/// </summary>
public sealed class DxfImportAnalysis
{
    public bool sourceExists;
    public bool hasSupportedCurves;
    public bool isCoplanar;
    public DxfImportMode recommendedMode;
    public int curveCount;
    public int uniquePointCount;
    public Vector3 planeOrigin;
    public Vector3 planeNormal;
    public float maxDistanceFromPlane;
    public string error;
    public readonly List<string> warnings = new List<string>();

    public string Summary
    {
        get
        {
            if (!string.IsNullOrEmpty(error))
                return error;

            string kind = isCoplanar ? "coplanar" : "spatial";
            return $"{curveCount} curve segments, {uniquePointCount} points, {kind}, " +
                   $"recommended={recommendedMode}";
        }
    }
}

/// <summary>
/// Settings shared by the simulation importer and the crease-pattern editor.
/// Each route deliberately writes into its own directory so the generated
/// JSON files remain identifiable and do not overwrite each other.
/// </summary>
[Serializable]
public sealed class DxfImportOptions
{
    public DxfImportMode mode = DxfImportMode.Auto;

    [Tooltip("Generated JSON directory for the legacy planar DXF route. Empty uses Assets/Models/Dxf2D.")]
    public string flatOutputDirectory;

    [Tooltip("Generated JSON directory for the spatial DXF route. Empty uses Assets/Models/Spatial.")]
    public string spatialOutputDirectory;

    [Tooltip("Optional JSON base name. Empty uses <source>_2d or <source>_spatial.")]
    public string outputFileName;

    [Min(0.000001f)]
    [Tooltip("Maximum WCS distance from the fitted source plane before Auto treats the DXF as spatial.")]
    public float coplanarityTolerance = 0.001f;

    [Tooltip("Options passed unchanged to the spatial importer when Spatial3D is selected.")]
    public SpatialDxfImportOptions spatialOptions = new SpatialDxfImportOptions();
}

/// <summary>
/// Unified result returned by DxfImportRouter.  jsonPath can be passed to the
/// existing JSON loader in either UI once success is true.
/// </summary>
public sealed class DxfImportResult
{
    public bool success;
    public DxfImportMode requestedMode;
    public DxfImportMode resolvedMode;
    public string jsonPath;
    public string error;
    public DxfImportAnalysis analysis;
    public readonly List<string> warnings = new List<string>();

    public string Summary
    {
        get
        {
            if (!string.IsNullOrEmpty(error))
                return error;

            return $"{resolvedMode}: {jsonPath}";
        }
    }
}

/// <summary>
/// One deduplicated DXF edge, using zero-based indices while topology is generated.
/// </summary>
public struct SpatialDxfIndexedEdge
{
    public int v1;
    public int v2;
    public OrigamiCrease.Type type;

    public SpatialDxfIndexedEdge(int v1, int v2, OrigamiCrease.Type type)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.type = type;
    }
}
