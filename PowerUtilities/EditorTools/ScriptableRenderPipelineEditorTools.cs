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
public static class ScriptableRenderPipelineEditorTools
{
    /// <summary>
    /// Find UniversalRenderPipelineAssets and current use URPAsset in Assets folder
    /// 
    /// </summary>
    /// <param name="urpAssets"></param>
    /// <param name="defaultAsset"></param>
    /// <returns></returns>
    public static int GetDefaultPipelineIndex(out UniversalRenderPipelineAsset[] urpAssets, out UniversalRenderPipelineAsset defaultAsset)
    {
        defaultAsset = null;
        urpAssets = AssetDatabaseTools.FindAssetsInProject<UniversalRenderPipelineAsset>();
        var selectedId = urpAssets.FindIndex(asset => asset == (QualitySettings.renderPipeline ?? GraphicsSettings.defaultRenderPipeline));
        if (selectedId == -1)
        {
            return -1;
        }
        defaultAsset = urpAssets[selectedId];
        return selectedId;
    }
}
#endif