#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Editor part
    /// </summary>
    public partial class QualitySettingEx
    {

        partial void OnEditorEnable()
        {
            EditorApplicationTools.OnInspectorContextMenu -= OnContextMenu;
            EditorApplicationTools.OnInspectorContextMenu += OnContextMenu;
        }

        void OnContextMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.serializedObject.targetObject.GetType() != typeof(QualitySettingEx))
                return;

            var propertyCopy = property.Copy();
            menu.AddItem(new GUIContent("Use this profile"), false, () =>
            {
                ApplyPipelineAssets(platformQualityPipelineAssets[propertyCopy.GetArrayIndex()]);
            });

        }

        partial void OnEditorDisable()
        {
            EditorApplication.contextualPropertyMenu -= OnContextMenu;
        }
        /// <summary>
        /// EditorOnly Method
        /// Match editor platform with name of Enum(BuildTarget,RuntimePlatform)
        /// </summary>
        public void ApplyPipelineAssetsByEditorBuildTarget()
        {
            var asset = GetPipelineAsset(EditorUserBuildSettingsTools.BuildTargetToPlatform(EditorUserBuildSettings.activeBuildTarget));
            if (asset != null)
            {
                var isOk = EditorUtility.DisplayDialog("Warning", $"buildTarget: {EditorUserBuildSettings.activeBuildTarget} ,platform:{Application.platform} , => {asset}", "ok", "cancel");
                if (isOk)
                    ApplyPipelineAssets(asset);
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", $"profile not found,current platform:{Application.platform}", "ok");
            }

        }

        public void EditorCleanPipelineAssets()
        {
            var isOk = EditorUtility.DisplayDialog("Warning", "Clean Quality's RenderPipelines" ,"ok","cancel");
            if (!isOk)
                return;

            CleanPipelineAssets();
        }
    }
}

#endif