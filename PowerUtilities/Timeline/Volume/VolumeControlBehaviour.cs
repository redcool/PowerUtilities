using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.Timeline
{

    [Serializable]
    public partial class VolumeControlBehaviour : PlayableBehaviour
    {
        [Header("Volume")]
        [Tooltip("Is valumeRef empty will use temporary Volume")]
        public ExposedReference<Volume> volumeRef;

        [Tooltip("this clip's volume,when empty use volumeRef or ower(root gameObject)'s volume")]
        public Volume clipVolume;

        public float volumeWeight = 1;

        Playable playable;
        int inputCount;
        bool isRefVolume;

        public override void OnPlayableCreate(Playable playable)
        {
            this.playable = playable;
            inputCount = playable.GetInputCount();
        }

        /// <summary>
        /// volumeRef is empty, will create temporary volumeGO with a profile
        /// 
        /// when clip create, call this
        /// 
        /// </summary>
        /// <param name="owner"></param>
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
