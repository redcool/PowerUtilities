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

        [Header("Template Profile")]
        [Tooltip("Bake settings from this profile")]
        public VolumeProfile profile;

        [Header("Clip info")]
        [Tooltip("Is valumeRef empty will use temporary Volume")]
        public ExposedReference<Volume> volumeRef;
        [Tooltip("this clip's volume,when empty use volumeRef or ower(root gameObject)'s volume")]
        public Volume clipVolume;
        public VolumeProfile clipVolumeProfile;

        public float volumeWeight = 1;

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

            v = v ?? clipVolume;
            if (!v)
            {
                v = new GameObject().AddComponent<Volume>();
                v.transform.SetParent(owner.transform, false);
            }
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
                clipVolume?.gameObject.Destroy();
        }

    }
}
