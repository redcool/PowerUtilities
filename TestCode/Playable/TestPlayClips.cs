using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PowerUtilities.Test
{
    public class TestPlayClips : MonoBehaviour
    {
        public AnimationClip[] clips;

        PlayableGraph graph;
        AnimationClipPlayable[] clipPlays;


        // Start is called before the first frame update
        void Start()
        {

            var anim = GetComponent<Animator>();

            AnimationPlayableOutput output = default;
            PlayableGraphTools.CreateGraph(anim, ref graph,ref output);

            clipPlays = graph.CreateClips(clips);

            var weights = Enumerable.Repeat<float>(0, clips.Length).ToArray();
            weights[0] = 1;
            var sequencePlay = graph.CreateScript<AnimationSequencePlayable>(clipPlays.Length, weights);
            sequencePlay.GetBehaviour().isLoop = true;
            
            for (int i = 0; i < clipPlays.Length; i++)
            {
                graph.Connect(clipPlays[i], 0, sequencePlay, i);
                clipPlays[i].Pause();
                //clipPlays[i].ConnectOutputPort(0, sequencePlay, i);
                //sequencePlay.ConnectInputPort(i, clipPlays[i], 0);
            }

            ScriptPlayableOutput scriptOutput = ScriptPlayableOutput.Create(graph, "script playable output");
            //scriptOutput.SetUserData(clipPlays);

            output.SetSourcePlayable(sequencePlay);

            graph.Play();

        }


        public void OnDisable()
        {
            if (graph.IsValid())
                graph.Destroy();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
