using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Shared entry point for DXF imports in both the simulation UI and the
/// crease-pattern editor.  Auto sends a truly coplanar curve network through
/// the established 2D converter, and sends a non-coplanar one through the new
/// spatial converter.
/// </summary>
public static class DxfImportRouter
{
    /// <summary>
    /// Reads DXF curve entities and reports whether their WCS endpoints occupy
    /// one plane.  This is intentionally not a z==0 check: elevated and
    /// inclined 2D drawings are still recognised as Flat2D.
    /// </summary>
    public static DxfImportAnalysis Analyze(
        string dxfPath,
        float coplanarityTolerance = Dxf3DToOrigamiConverter.DefaultCoplanarityTolerance)
    {
        return Dxf3DToOrigamiConverter.AnalyzeSource(dxfPath, coplanarityTolerance);
    }

    /// <summary>
    /// Converts a DXF using Auto or an explicitly requested route.  On success
    /// the returned jsonPath can be supplied directly to either existing JSON
    /// loader; the UI does not need to know which converter was selected.
    /// </summary>
    public static DxfImportResult Convert(string dxfPath, DxfImportOptions options = null)
    {
        DxfImportOptions effectiveOptions = options ?? new DxfImportOptions();
        DxfImportResult result = new DxfImportResult
        {
            requestedMode = effectiveOptions.mode
        };

        List<DxfCurveSegment> curves;
        DxfImportAnalysis analysis;
        bool sourceReady = Dxf3DToOrigamiConverter.TryReadSourceCurves(
            dxfPath,
            effectiveOptions.coplanarityTolerance,
            out curves,
            out analysis);

        result.analysis = analysis;
        result.warnings.AddRange(analysis.warnings);
        if (!sourceReady)
        {
            result.error = analysis.error;
            return result;
        }

        DxfImportMode resolvedMode = effectiveOptions.mode == DxfImportMode.Auto
            ? analysis.recommendedMode
            : effectiveOptions.mode;
        result.resolvedMode = resolvedMode;

        if (resolvedMode == DxfImportMode.Flat2D)
        {
            if (!analysis.isCoplanar)
            {
                result.error =
                    "Flat2D was requested, but the DXF curve endpoints are non-coplanar. " +
                    "Use Auto or Spatial3D so the folded geometry is not flattened incorrectly.";
                return result;
            }

            ConvertFlat2D(dxfPath, curves, analysis, effectiveOptions, result);
            return result;
        }

        ConvertSpatial3D(dxfPath, effectiveOptions, result);
        return result;
    }

    /// <summary>
    /// Convenience overload for a UI button that explicitly says "2D DXF" or
    /// "3D DXF" without constructing an options object first.
    /// </summary>
    public static DxfImportResult Convert(string dxfPath, DxfImportMode mode)
    {
        return Convert(dxfPath, new DxfImportOptions { mode = mode });
    }

    private static void ConvertFlat2D(
        string sourcePath,
        List<DxfCurveSegment> curves,
        DxfImportAnalysis analysis,
        DxfImportOptions options,
        DxfImportResult result)
    {
        string outputDirectory = string.IsNullOrWhiteSpace(options.flatOutputDirectory)
            ? Path.Combine(Application.dataPath, "Models", "Dxf2D")
            : options.flatOutputDirectory;
        string outputName = ResolveOutputName(sourcePath, options.outputFileName, "_2d");
        string temporaryDxfPath = null;

        try
        {
            temporaryDxfPath = WritePlanarTemporaryDxf(curves, analysis);
            bool converted = DxfToJsonConverter.Convert(temporaryDxfPath, outputDirectory, outputName);
            if (!converted)
            {
                result.error = "The legacy Flat2D DXF converter could not create JSON from the projected curve network.";
                return;
            }

            result.success = true;
            result.jsonPath = Path.Combine(outputDirectory, outputName + ".json");
            result.warnings.Add(
                "Flat2D used the legacy DxfToJsonConverter after projecting the source plane to a temporary LINE-only DXF.");
        }
        catch (Exception exception)
        {
            result.error = $"Unable to prepare planar DXF import: {exception.Message}";
        }
        finally
        {
            DeleteTemporaryFile(temporaryDxfPath, result.warnings);
        }
    }

    private static void ConvertSpatial3D(
        string sourcePath,
        DxfImportOptions options,
        DxfImportResult result)
    {
        string outputDirectory = string.IsNullOrWhiteSpace(options.spatialOutputDirectory)
            ? Path.Combine(Application.dataPath, "Models", "Spatial")
            : options.spatialOutputDirectory;
        string outputName = ResolveOutputName(sourcePath, options.outputFileName, "_spatial");
        SpatialDxfImportResult spatialResult = Dxf3DToOrigamiConverter.Convert(
            sourcePath,
            outputDirectory,
            outputName,
            options.spatialOptions ?? new SpatialDxfImportOptions());

        result.success = spatialResult.success;
        result.jsonPath = spatialResult.jsonPath;
        result.error = spatialResult.error;
        result.warnings.AddRange(spatialResult.warnings);
    }

    private static string ResolveOutputName(string sourcePath, string requestedName, string suffix)
    {
        string baseName = string.IsNullOrWhiteSpace(requestedName)
            ? Path.GetFileNameWithoutExtension(sourcePath)
            : Path.GetFileNameWithoutExtension(Path.GetFileName(requestedName));
        if (string.IsNullOrWhiteSpace(baseName))
            baseName = "ImportedDxf";

        return string.IsNullOrWhiteSpace(requestedName) ? baseName + suffix : baseName;
    }

    /// <summary>
    /// The old converter accepts only planar LINE / POLYLINE data.  Write a
    /// compact temporary LINE-only DXF after an isometric plane projection so
    /// it can remain unchanged while still accepting LWPOLYLINE and any plane
    /// orientation from Rhino.
    /// </summary>
    private static string WritePlanarTemporaryDxf(
        List<DxfCurveSegment> curves,
        DxfImportAnalysis analysis)
    {
        Vector3 normal = analysis.planeNormal.sqrMagnitude > 0.000001f
            ? analysis.planeNormal.normalized
            : Vector3.forward;
        Vector3 reference = Mathf.Abs(normal.z) < 0.9f ? Vector3.forward : Vector3.up;
        Vector3 xAxis = Vector3.Cross(reference, normal).normalized;
        if (xAxis.sqrMagnitude < 0.000001f)
            xAxis = Vector3.right;
        Vector3 yAxis = Vector3.Cross(normal, xAxis).normalized;
        Vector3 origin = analysis.planeOrigin;

        string temporaryDirectory = Path.Combine(Path.GetTempPath(), "KreslingDxfImport");
        Directory.CreateDirectory(temporaryDirectory);
        string temporaryPath = Path.Combine(temporaryDirectory, Path.GetRandomFileName() + ".dxf");

        StringBuilder dxf = new StringBuilder(Mathf.Max(1024, curves.Count * 100));
        AppendPair(dxf, 0, "SECTION");
        AppendPair(dxf, 2, "ENTITIES");
        for (int i = 0; i < curves.Count; i++)
        {
            DxfCurveSegment curve = curves[i];
            Vector3 start = curve.start - origin;
            Vector3 end = curve.end - origin;
            float x1 = Vector3.Dot(start, xAxis);
            float y1 = Vector3.Dot(start, yAxis);
            float x2 = Vector3.Dot(end, xAxis);
            float y2 = Vector3.Dot(end, yAxis);

            AppendPair(dxf, 0, "LINE");
            AppendPair(dxf, 8, "0");
            AppendPair(dxf, 420, (curve.rgbColor & 0xFFFFFF).ToString(CultureInfo.InvariantCulture));
            AppendPair(dxf, 10, FormatFloat(x1));
            AppendPair(dxf, 20, FormatFloat(y1));
            AppendPair(dxf, 30, "0");
            AppendPair(dxf, 11, FormatFloat(x2));
            AppendPair(dxf, 21, FormatFloat(y2));
            AppendPair(dxf, 31, "0");
        }

        AppendPair(dxf, 0, "ENDSEC");
        AppendPair(dxf, 0, "EOF");
        try
        {
            File.WriteAllText(temporaryPath, dxf.ToString(), Encoding.ASCII);
            return temporaryPath;
        }
        catch
        {
            // The caller has not received the path yet when WriteAllText
            // throws, so clean it here as well as in the caller's finally.
            try
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
            catch
            {
                // Preserve the original write exception; the caller will show it.
            }
            throw;
        }
    }

    private static void AppendPair(StringBuilder dxf, int code, string value)
    {
        dxf.Append(code.ToString(CultureInfo.InvariantCulture));
        dxf.Append('\n');
        dxf.Append(value);
        dxf.Append('\n');
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("G9", CultureInfo.InvariantCulture);
    }

    private static void DeleteTemporaryFile(string temporaryPath, List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(temporaryPath) || !File.Exists(temporaryPath))
            return;

        try
        {
            File.Delete(temporaryPath);
        }
        catch (Exception exception)
        {
            warnings.Add($"Could not remove temporary planar DXF '{temporaryPath}': {exception.Message}");
        }
    }
}
