#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
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

            EditorApplication.contextualPropertyMenu -= OnContextMenu;
            EditorApplication.contextualPropertyMenu += OnContextMenu;
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
            var buildTargetName = Enum.GetName(typeof(BuildTarget), EditorUserBuildSettings.activeBuildTarget);

            var buildTarget = Application.platform;
            var asset = platformQualityPipelineAssets.Where(info => Enum.GetName(typeof(RuntimePlatform), info.platform) == buildTargetName).FirstOrDefault();
            if (asset == null)
            {
                EditorUtility.DisplayDialog("Warning", $"profile not found,current platform:{buildTarget}", "ok");
                return;
            }

            ApplyPipelineAssets(asset);
        }
    }
}

#endif