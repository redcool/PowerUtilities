#if UNITY_EDITOR
using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// scriptable render pipeline tools
/// </summary>
public static class SRPTools
{

    public static int GetDefaultPipelineIndex(out UniversalRenderPipelineAsset[] urpAssets, out UniversalRenderPipelineAsset defaultAsset)
    {
        urpAssets = AssetDatabaseTools.FindAssetsInProject<UniversalRenderPipelineAsset>();
        var selectedId = urpAssets.FindIndex(asset => asset == (QualitySettings.renderPipeline ?? GraphicsSettings.defaultRenderPipeline));
        defaultAsset = urpAssets[selectedId];
        return selectedId;
    }
}
#endif