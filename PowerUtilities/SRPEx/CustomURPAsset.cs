namespace PowerUtilities
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
    using UnityEditor.Rendering.Universal;
    using UnityEngine.Rendering;
    using System.Reflection;
    using System.Linq;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(CustomURPAsset)), CanEditMultipleObjects]
    public class UniversalRenderPipelineAssetExEditor : UniversalRenderPipelineAssetEditor
    {

        [MenuItem("PowerUtilities/SRP/ChangeURPAssetScript")]
        static void ChangeURPAssetScript()
        {
            var t = GraphicsSettings.currentRenderPipeline.GetType();
            var so = new SerializedObject(GraphicsSettings.currentRenderPipeline);
            var script = so.FindProperty("m_Script");

            var urpAssetExScript = AssetDatabase.FindAssets("CustomURPAsset")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<MonoScript>(path))
                .FirstOrDefault();

            if (urpAssetExScript == null)
                return;

            script.objectReferenceValue =urpAssetExScript;
            so.ApplyModifiedProperties();
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