using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace PowerUtilities
{
    public class CustomPlayable : PlayableBehaviour
    {
        public override void OnGraphStart(Playable playable)
        {
            Debug.Log(nameof (OnGraphStart));
        }

        public override void OnGraphStop(Playable playable)
        {
            Debug.Log(nameof(OnGraphStop));
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var time = playable.GetTime();
            var duration = playable.GetDuration();
        }
    }

    public class CustomTimelineClipPlayable : PlayableBehaviour
    {
        public Color start = Color.white, end = Color.white;
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var image = playerData as Image;
            if(image)
            {
                var duration = playable.GetDuration();
                var time = playable.GetTime();
                image.color = Color.Lerp(start, end, (float)(time/duration));
            }
        }
    }

    [Serializable]
    public class CustomTimelinePlayableAsset : PlayableAsset
    {
        public Color start, end;
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var play = ScriptPlayable<CustomTimelineClipPlayable>.Create(graph);
            var behaviour = play.GetBehaviour();
            behaviour.start = start;
            behaviour.end = end;
            return play;
        }
    }

    [TrackBindingType(typeof(Image))]
    [TrackClipType(typeof(CustomTimelinePlayableAsset))]
    public class CustomTimelineTrack : TrackAsset
    {

    }


    public class TestScriptPlayable : MonoBehaviour
    {
        PlayableGraph graph;
        //AnimationPlayableOutput output;
        ScriptPlayableOutput scriptOutput;
        private void Start()
        {
            graph = PlayableGraph.Create(name: "Test");

            var customPlay = ScriptPlayable<CustomPlayable>.Create(graph);
            

            //output = AnimationPlayableOutput.Create(graph,"anim out",GetComponent<Animator>()); 
            //output.SetSourcePlayable(customPlay);

            scriptOutput = ScriptPlayableOutput.Create(graph, "script out");
            scriptOutput.SetSourcePlayable(customPlay);

            graph.Play();
        }

        private void OnDestroy()
        {
            graph.Destroy();
        }
    }
}
