using PowerUtilities.RenderFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace PowerUtilities.RenderFeatures
{
    [Tooltip("urp's volume profile ")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU + "/SceneVolumeControl")]
    public class SceneVolumeControl : SRPFeature
    {
        [Header("Default scene volume profile")]
        public VolumeProfile profile;
        [Range(0, 1)]
        public float volumeWeight;

        [Header("Quality's volume profile")]
        [ListItemDraw("qualityLevel:,qualityLevel,profile:,profile", "100,100,50,")]
        public List<SceneVolumeControlQualityVolume> volumeList = new List<SceneVolumeControlQualityVolume>();

        [Tooltip("Update interval time(frame count)")]
        [Min(1)]
        public int framesInterval = 1;

        [Header("Other Volumes")]
        [Tooltip("Update others global volume")]
        public bool isOverrideGlobalVolumeWeight;
        [Range(0,1)]
        public float otherGlobalVolumeWeight = 0.3f;

        [Header("Debug")]
        [EditorDisableGroup]
        public GameObject sceneVolumeControlGo;

        public override ScriptableRenderPass GetPass()
        {
            return new SceneVolumeControlPass(this);
        }

    }

    [Serializable]
    public class SceneVolumeControlQualityVolume
    {
        [EnumFlags(isFlags = false, type = typeof(QualitySettings), memberName = "names")]
        public int qualityLevel = 0;
        public VolumeProfile profile;
    }

    public class SceneVolumeControlPass : SRPPass<SceneVolumeControl>
    {
        //Scene lastScene;
        float lastOtherVolumeWeight;
        Volume volume;
        int lastFrameCount;

        public override bool CanExecute()
        {
            var isTimeValid = CompareTools.CompareAndSet(ref lastFrameCount, Time.frameCount, (a, b) => b - a > Feature.framesInterval);

            return isTimeValid && base.CanExecute() && Feature.profile;
        }

        public SceneVolumeControlPass(SceneVolumeControl feature) : base(feature)
        {
        }

        void UpdateVolume()
        {
            if(!volume && Feature.sceneVolumeControlGo)
                volume = Feature.sceneVolumeControlGo.GetComponent<Volume>();

            if (!volume)
                return;

            volume.weight = Feature.volumeWeight;
        }
        /// <summary>
        /// Create sceneVolumeControlGo, only once(or onEnable)
        /// </summary>
        void TrySetupVolumeGo()
        {
            if (Feature.sceneVolumeControlGo)
                return;

            var volumeGo = Feature.sceneVolumeControlGo = new GameObject(featureName);
            volumeGo.hideFlags = HideFlags.DontSave;
            volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;

            //use default profile
            volume.profile = Feature.profile;

            // get item by quality level
            var qVolume = Feature.volumeList.Find(item => item.qualityLevel == QualitySettings.GetQualityLevel());
            if (qVolume != null && qVolume.profile)
                volume.profile = qVolume.profile;
        }

        public override void OnDisable()
        {
            if (Feature.sceneVolumeControlGo)
                Feature.sceneVolumeControlGo.Destroy();
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            // only once(or onEnable)
            TrySetupVolumeGo();

            UpdateVolume();
            UpdateOtherGlobalVolumes();
        }

        private void UpdateOtherGlobalVolumes()
        {
            if (!Feature.isOverrideGlobalVolumeWeight)
                return;

            if(CompareTools.CompareAndSet(ref lastOtherVolumeWeight,ref Feature.otherGlobalVolumeWeight))
            {
                var volumes = GameObject.FindObjectsByType<Volume>(FindObjectsSortMode.None);
                foreach (var v in volumes)
                {
                    if (!v.isGlobal)
                        continue;

                    v.weight = Feature.otherGlobalVolumeWeight;
                }
            }
        }

        /// <summary>
        /// when scene is changed Feature.sceneVolumeControlGo need reCreate
        /// </summary>
        //private void CheckSceneChanged()
        //{
        //    var scene = SceneManager.GetActiveScene();

        //    if(CompareTools.CompareAndSet(ref lastScene,ref scene))
        //    {
        //        OnDisable();
        //        Feature.sceneVolumeControlGo = null;
        //    }
        //}

        public override void OnSceneChanged()
        {
            OnDisable();
        }
    }
}
