using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PowerUtilities.Test
{
    public class TestAnimationClipPlayable : MonoBehaviour
    {
        public AnimationClip clip;
        public AnimationClipPlayable clipPlay;

        public PlayableGraph graph;
        public AnimationPlayableOutput output;

        public bool isManualTime;
        public float time;

        public bool isSetDuration;
        public float duration;

        public float speed=1;

        public bool isPause;
        public bool isPlay;

        // Start is called before the first frame update
        void Start()
        {
            graph = PlayableGraph.Create();
            output = AnimationPlayableOutput.Create(graph, "anim output", GetComponent<Animator>());

            clipPlay = PlayableTools.CreateClip(graph, clip,2);
            clipPlay.SetOutputCount(2);

            output.SetSourcePlayable(clipPlay);

            graph.Play();
        }

        // Update is called once per frame
        void Update()
        {
            if (isManualTime)
            {
                clipPlay.SetTime(time);
            }
            if(isSetDuration)
                clipPlay.SetDuration(duration);

            clipPlay.SetSpeed(speed);

            if(isPause)
            {
                clipPlay.Pause();
            }

            if(isPlay)
            {
                clipPlay.Play();
            }

        }
    }
}
