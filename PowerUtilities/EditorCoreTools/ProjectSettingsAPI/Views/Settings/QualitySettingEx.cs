using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace PowerUtilities
{
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Project/QualitySettingEx")]
    [SOAssetPath("Assets/PowerUtilities/QualitySettingEx.asset")]
    public class QualitySettingEx : ScriptableObject
    {
        /// <summary>
        /// control gameobject children count
        /// </summary>
        [Serializable]
        public class QualityInfo
        {
            [EnumFlags(isFlags = false, type = typeof(QualitySettings), memberName = "names")]
            [Tooltip("quality level")]
            public int qualityLevel = 3;

            [Tooltip("component count(like :particle System Count),exceed is disabled from top to down")]
            [Min(0)]
            public int componentCount=3;
        }

        /// <summary>
        /// control platform quality renderPipeline
        /// </summary>
        [Serializable]
        public class QualityPipelineAsset
        {
            [EnumFlags(isFlags = false, type = typeof(QualitySettings), memberName = "names")]
            [Tooltip("quality level")]
            public int qualityLevel = 3;

            public RenderPipelineAsset pipelineAsset;
        }

        [Serializable]
        public class PlatformQualityPipelineAsset
        {
            public RuntimePlatform platform;
            public RenderPipelineAsset defaultAsset;

            [ListItemDraw("qLV:,qualityLevel,assets:,pipelineAsset","50,100,50,250")]
            public List<QualityPipelineAsset> qualityPipelineAssets = new();

        }

        [HelpBox]
        public string helpBox = "control gameobject's max children count";
        
        [ListItemDraw("qLv:,qualityLevel,CompCount:,componentCount", "100,100,100,50")]
        public List<QualityInfo> infos = new();

        [ListItemDraw("platform:,platform,default:,defaultAsset,assets:,qualityPipelineAssets", "80,150,70,.2,70,", rowCountArrayPropName = "qualityPipelineAssets")]
        public List<PlatformQualityPipelineAsset> platformQualityPipelineAssets = new();

        [EditorButton(onClickCall ="AutoRunProfile")]
        [Tooltip("use profiles match current build target platform")]
        public bool isAutoRunAssets;

        [EditorButton(onClickCall = "CleanPipelineAssets")]
        [Tooltip("use profiles match current build target platform")]
        public bool isCleanPipelineAssets;

        private void Awake()
        {
            if (infos.Count > 0)
                return;

            for (var i = 0; i < QualitySettings.count; i++)
            {
                infos.Add(new QualityInfo { qualityLevel = i });
            }

        }

#if UNITY_EDITOR
        private void OnEnable()
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
                ApplyProfile(platformQualityPipelineAssets[propertyCopy.GetArrayIndex()]);
            });
            
        }

        private void OnDisable()
        {
            EditorApplication.contextualPropertyMenu -= OnContextMenu;
        }
#endif
        public QualityInfo GetInfo(int qLevel) 
        {
            return infos.Find((QualityInfo info )=> info.qualityLevel == qLevel);
        }

        public void AutoRunProfile()
        {
#if UNITY_EDITOR
            var buildTargetName = Enum.GetName(typeof(BuildTarget),EditorUserBuildSettings.activeBuildTarget);
            
            var buildTarget = Application.platform;
            var asset = platformQualityPipelineAssets.Where(info => Enum.GetName(typeof(RuntimePlatform),info.platform) == buildTargetName).FirstOrDefault();
            if (asset == null)
            {
                EditorUtility.DisplayDialog("Warning", $"profile not found,current platform:{buildTarget}", "ok");
                return;
            }

            ApplyProfile(asset);
#endif
        }

        public void ApplyProfile(PlatformQualityPipelineAsset asset)
        {
            if(asset.defaultAsset)
                GraphicsSettings.defaultRenderPipeline = asset.defaultAsset;

            for (int i = 0; i < asset.qualityPipelineAssets.Count; i++)
            {
                var setting = asset.qualityPipelineAssets[i];
                var qualityLevel = setting.qualityLevel;
                var pipelineAsset = setting.pipelineAsset;

                if (pipelineAsset)
                    QualitySettingsTools.SetRenderPipelineAssetAt(qualityLevel, pipelineAsset);
            }

        }

        public void CleanPipelineAssets()
        {
            QualitySettingsTools.CleanAllRenderPipelineAsset();
        }
    }
}
