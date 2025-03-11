namespace PowerUtilities
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Project/QualitySettingEx")]
    [SOAssetPath("Assets/PowerUtilities/Resources/QualitySettingEx.asset")]
    public partial class QualitySettingEx : ScriptableObject
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
            public RuntimePlatform[] platform;
            public RenderPipelineAsset defaultAsset;

            [ListItemDraw("qLV:,qualityLevel,assets:,pipelineAsset","50,100,50,250")]
            public List<QualityPipelineAsset> qualityPipelineAssets = new();

        }
        /// <summary>
        /// children count
        /// </summary>
        [HelpBox]
        public string helpBoxControlChildrenCount = "control gameobject's max children count";
        
        [ListItemDraw("qLv:,qualityLevel,CompCount:,componentCount", "100,100,100,50")]
        public List<QualityInfo> infos = new();

        /// <summary>
        /// manage platform quality renderPipelineAsset
        /// </summary>
        [EditorBorder(1,bottomColorStr: ColorTools.LIGHT_GREEN)]
        [HelpBox]
        public string helpBoxPlatformPipelineAsset = "manage platform quality renderPipelineAsset";

        [ListItemDraw("platform:,platform,default:,defaultAsset,assets:,qualityPipelineAssets", "80,200,70,.2,70,", rowCountArrayPropName = "qualityPipelineAssets")]
        public List<PlatformQualityPipelineAsset> platformQualityPipelineAssets = new();

        /// <summary>
        /// Gameplaying options
        /// </summary>
        [Header("GamePlaying")]
        [Tooltip("Auto apply pipelineAsset when game playing")]
        public bool isApplyWhenPlaying;

        /// <summary>
        /// Buttons( hbox)
        /// </summary>
        [EditorBox("Options", "isApplyPipelineAssetsByEditorBuildTarget,isCleanPipelineAssets",boxType = EditorBoxAttribute.BoxType.HBox)]
        [EditorButton(onClickCall = "ApplyPipelineAssetsByEditorBuildTarget")]
        [Tooltip("use profiles match current build target platform")]
        public bool isApplyPipelineAssetsByEditorBuildTarget;

        [EditorButton(onClickCall = "EditorCleanPipelineAssets")]
        [Tooltip("use profiles match current build target platform")]
        [HideInInspector]
        public bool isCleanPipelineAssets;


        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            var inst = QualitySettingEx.GetInstance();
            if (inst && inst.isApplyWhenPlaying)
            {
                Debug.Log("auto apply asset with platform : " + Application.platform);
                inst.ApplyPipelineAssetsByRuntimePlatform();
            }
        }

        private void Awake()
        {
            if (infos.Count > 0)
                return;

            for (var i = 0; i < QualitySettings.count; i++)
            {
                infos.Add(new QualityInfo { qualityLevel = i });
            }

        }
        partial void OnEditorEnable();
        partial void OnEditorDisable();
        private void OnEnable()
        {
            OnEditorEnable();
        }

        private void OnDisable()
        {
            OnEditorDisable();
        }

        public QualityInfo GetInfo(int qLevel) 
        {
            return infos.Find((QualityInfo info )=> info.qualityLevel == qLevel);
        }

        public void ApplyPipelineAssets(PlatformQualityPipelineAsset asset)
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

        public void ApplyPipelineAssets(int index)
        {
            if (index > platformQualityPipelineAssets.Count)
                return;

            var asset = platformQualityPipelineAssets[index];
            if (asset != null)
                ApplyPipelineAssets(asset);
        }

        /// <summary>
        /// Find and Apply pipelineAssets by Applycation.platform
        /// </summary>
        public void ApplyPipelineAssetsByRuntimePlatform()
        {
            var asset = platformQualityPipelineAssets.Find(item => item.platform.Contains(Application.platform));
            if (asset != null)
                ApplyPipelineAssets(asset);
        }
        /// <summary>
        /// Clean QualitySettings PipelineAsset
        /// </summary>
        public void CleanPipelineAssets()
        {
            QualitySettingsTools.CleanAllRenderPipelineAsset();
        }

        /// <summary>
        /// Get instance from Resources folder
        /// </summary>
        /// <returns></returns>
        public static QualitySettingEx GetInstance()
        => Resources.Load<QualitySettingEx>(nameof(QualitySettingEx));

        /// <summary>
        /// Get pipelineAsset by buildTargetName(EditorUserBuildSettings.activeBuildTarget
        /// </summary>
        /// <param name="buildTargetName"></param>
        /// <param name="isIOS"></param>
        /// <returns></returns>
        public PlatformQualityPipelineAsset GetPipelineAsset(RuntimePlatform platform)
        {

            foreach (var info in platformQualityPipelineAssets)
            {
                if(info.platform.Contains(platform))
                {
                    return info;
                }
            }
            return null;
        }
    }
}
