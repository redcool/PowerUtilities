using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace PowerUtilities.Timeline
{
    [Serializable]
    public class VolumeControlClip : PlayableAsset
    {
        [Header("--- 0.0.2")]
        //will clone this
        public VolumeControlBehaviour template;

        public string guid;


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var sp = ScriptPlayable<VolumeControlBehaviour>.Create(graph, template);
            var b = sp.GetBehaviour();
            b.TrySetup(owner,sp,GUIDTools.GetGUID(guid));

            //instance = b;

            //template.TrySetup(owner);
            return sp;
        }
        
    }
}
