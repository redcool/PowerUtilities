using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PowerUtilities.Test
{
    public partial class TestPlayClip : MonoBehaviour
    {
        public AnimationClip clip;

        public bool isManual;
        [EditorIntent(2)] public float progress;

        [EditorIntent(-2)]
        public float speed=1;

        public bool isPause;
        bool isPauseLast;

        [Header("Debug")]
        public float clipTime;

        PlayableGraph graph;
        AnimationClipPlayable clipPlay;

        // Start is called before the first frame update
        void Start()
        {
            if (!clip)
                return;

            var anim = GetComponent<Animator>();

            AnimationPlayableOutput output = default;
            PlayableGraphTools.CreateGraph(anim, ref graph,ref output);

            clipPlay = graph.CreateClip(clip);

            var handlerPlay = graph.CreateScript<AnimationEndHandler>(1, 1);
            handlerPlay.GetBehaviour().OnDone = () => Debug.Log("play done");

            // series conn
            graph.Connect(clipPlay, 0, handlerPlay, 0);
            output.SetSourcePlayable(handlerPlay);

            graph.Play();

            clipTime = clip.length;
        }


        public void OnDisable()
        {
            if (graph.IsValid())
                graph.Destroy();
        }

        // Update is called once per frame
        void Update()
        {
            if (!clipPlay.IsValid())
                return;
            
            if (isManual)
            {
                clipPlay.SetProgress(progress,clip.length);
            }
            else
            { 
                progress = (float)clipPlay.GetProgress(clip.length);
            }

            clipPlay.SetSpeed(speed);

            if (CompareTools.CompareAndSet(ref isPauseLast, isPause))
            {
                if (isPause)
                    clipPlay.Pause();
                else
                    clipPlay.Play();
            }

        }
    }
}
