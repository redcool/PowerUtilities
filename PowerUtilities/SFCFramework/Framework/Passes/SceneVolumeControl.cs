using Jint.Parser.Ast;
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
        [Tooltip("default Volume's weight")]
        public float volumeWeight;

        [Tooltip("Defualt Volume's priority")]
        public int volumePriority;


        [Tooltip("Create global volume use layerMasks")]
        [LayerIndex]
        public List<int> volumeLayers = new() { 0 };

        [Header("Quality's volume profile")]
        [ListItemDraw("qualityLevel:,qualityLevel,profile:,profile", "100,100,50,")]
        public List<SceneVolumeControlQualityVolume> volumeList = new List<SceneVolumeControlQualityVolume>();

        [Tooltip("Update interval time(frame count),set 0 will update per frame")]
        [Min(0)]
        public int framesInterval = 0;

        [Header("Other Volumes")]
        [Tooltip("Update others global volume")]
        public bool isOverrideGlobalVolumeWeight;
        [Range(0,1)]
        public float otherGlobalVolumeWeight = 0.3f;

        [Header("Debug")]
        [EditorDisableGroup]
        public GameObject sceneVolumeControlRootGo;

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
        List<Volume> volumes = new();
        int lastFrameCount;

        public override bool CanExecute()
        {
            var isTimeValid = Feature.framesInterval <=0 ? 
                true :  CompareTools.CompareAndSet(ref lastFrameCount, Time.frameCount, (a, b) => (b - a) > Feature.framesInterval);

            return isTimeValid && base.CanExecute() && Feature.profile;
        }

        public SceneVolumeControlPass(SceneVolumeControl feature) : base(feature)
        {
        }

        void UpdateVolume(Volume volume)
        {
            if (!volume)
                return;

            volume.weight = Feature.volumeWeight;
            volume.priority = Feature.volumePriority;
        }
        /// <summary>
        /// Create sceneVolumeControlGo, only once(or onEnable)
        /// </summary>
        Volume CreateVolumeGo(LayerMask layer)
        {
            var volumeGo = new GameObject($"{featureName}_{LayerMask.LayerToName(layer)}");
            volumeGo.transform.SetParent(Feature.sceneVolumeControlRootGo.transform);
            volumeGo.hideFlags = HideFlags.DontSave;
            volumeGo.layer = layer;

            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;

            //use default profile
            volume.profile = Feature.profile;

            // get item by quality level
            var qVolume = Feature.volumeList.Find(item => item.qualityLevel == QualitySettings.GetQualityLevel());
            if (qVolume != null && qVolume.profile)
                volume.profile = qVolume.profile;

            return volume;
        }

        public override void OnDisable()
        {
            DestroyGlobalVolumes();
        }

        private void DestroyGlobalVolumes()
        {
            if (Feature.sceneVolumeControlRootGo)
                Feature.sceneVolumeControlRootGo.Destroy();

            volumes.Clear();
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            CreateVolumes();
            UpdateVolumes();
            UpdateOtherGlobalVolumes();
        }

        private void UpdateVolumes()
        {
            foreach (var volume in volumes)
            {
                UpdateVolume(volume);
            }
        }

        private void CreateVolumes()
        {
            if (!Feature.sceneVolumeControlRootGo)
            {
                Feature.sceneVolumeControlRootGo = new GameObject("sceneVolumeControlRootGo");

                volumes.Clear();
                foreach (var layer in Feature.volumeLayers)
                {
                    volumes.Add(CreateVolumeGo(layer));
                }
            }
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

        public override void OnSceneChanged()
        {
            OnDisable();
        }

    }
}
