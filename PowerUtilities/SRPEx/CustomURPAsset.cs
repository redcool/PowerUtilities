namespace PowerUtilities
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor.Rendering.Universal;
    using UnityEditor;
#endif
    using UnityEngine.Rendering.Universal;
    using UnityEngine.Rendering;
    using System.Reflection;
    using System.Linq;

#if UNITY_EDITOR

    [CustomEditor(typeof(CustomURPAsset)), CanEditMultipleObjects]
    public class UniversalRenderPipelineAssetExEditor : UniversalRenderPipelineAssetEditor
    {

        [MenuItem("PowerUtilities/SRP/ChangeURPAssetScript")]
        static void ChangeURPAssetScript()
        {
            // get script CustomURPAsset.cs
            var urpAssetExScript = AssetDatabase.FindAssets("CustomURPAsset")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<MonoScript>(path))
                .FirstOrDefault();

            if (urpAssetExScript == null)
                return;

            GraphicsSettings.currentRenderPipeline.ChangeRefScript(urpAssetExScript);
        }
    }
#endif
    /// <summary>
    /// UniversalRenderPipelineAsset extends,
    /// change RenderPipelineAsset's script to CustomURPAsset
    /// 
    /// LightExplorer is urp's feature
    /// USE LightExplorerEx need use CustomURPAsset
    /// 
    /// </summary>
    public class CustomURPAsset : UniversalRenderPipelineAsset
    {

    }
}