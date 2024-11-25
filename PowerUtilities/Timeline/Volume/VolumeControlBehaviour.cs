using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.Timeline
{

    [Serializable]
    public class VolumeControlBehaviour : PlayableBehaviour
    {
        public Bloom_Data bloomData;
        public ColorAdjustments_Data ColorAdjustments_Data;

        [Header("Volume")]
        [Tooltip("Is valumeRef empty will use temporary Volume")]
        public ExposedReference<Volume> volumeRef;
        public float volumeWeight = 1;

        [Header("Template Profile")]
        [Tooltip("Bake settings from this profile")]
        public VolumeProfile profile;

        [Header("Clip info")]
        [Tooltip("this clip's volume,when empty use volumeRef or ower(root gameObject)'s volume")]

        public Volume clipVolume;
        [Tooltip("save clip 's volume profile")]
        //public List<VolumeComponent> components = new List<VolumeComponent>();
        public VolumeProfile clipVolumeProfile;

        Playable playable;
        bool isRefVolume;

        public string clipVolumeProfilePath = "";

        public override void OnPlayableCreate(Playable playable)
        {
            this.playable = playable;
        }

        public void TrySetup(GameObject owner)
        {
            var v = volumeRef.Resolve(playable.GetGraph().GetResolver());
            isRefVolume = v;

            v = v ?? clipVolume ?? owner.GetOrAddComponent<Volume>(); ;
            if (v)
            {
                v.isGlobal = true;
                v.profile = ScriptableObject.CreateInstance<VolumeProfile>();
                v.priority = 1;
                v.weight = 1;
            }
            clipVolume = v;
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            if (!isRefVolume)
                clipVolume?.Destroy();
        }

    }
}
