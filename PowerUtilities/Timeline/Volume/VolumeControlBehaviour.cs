using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace PowerUtilities.Timeline
{
    [Serializable]
    public class VolumeControlBehaviour : PlayableBehaviour
    {
        [Header("Volume")]
        [Tooltip("Is valumeRef empty will use temporary Volume")]
        public ExposedReference<Volume> volumeRef;
        public float volumeWeight = 1;

        [Header("Volume Profile")]
        [Tooltip("Bake settings from this profile")]
        public VolumeProfile profile;

        [Header("Clip info")]
        public Volume clipVolume;
        public List<VolumeComponent> components = new List<VolumeComponent>();

        Playable playable;
        bool isRefVolume;

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
