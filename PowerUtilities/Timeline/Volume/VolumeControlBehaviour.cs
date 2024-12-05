using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.Timeline
{

    [Serializable]
    public partial class VolumeControlBehaviour : BaseVolumeControlBehaviour
    {


        bool isRefVolume;

        /// <summary>
        /// volumeRef is empty, will create temporary volumeGO with a profile
        /// 
        /// when clip create, call this
        /// 
        /// </summary>
        /// <param name="owner"></param>
        public void TrySetup(GameObject owner,Playable playable,string guid)
        {
            //Debug.Log("TrySetup :" + playable);
            var v = volumeRef.Resolve(playable.GetGraph().GetResolver());
            isRefVolume = v;

            v = v ?? clipVolume;
            if (!v)
            {
                var tr = owner.transform.Find(guid) ?? new GameObject(guid).transform;
                v = tr.gameObject.AddComponent<Volume>();
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
            //Debug.Log("OnPlayableDestroy");
            if (!isRefVolume)
                clipVolume?.gameObject.Destroy();
        }

    }
}
