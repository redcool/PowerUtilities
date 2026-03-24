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
    public class TestPlayClip : MonoBehaviour,IMono
    {
        public AnimationClip clip;

        public bool isManual;
        [EditorIntent(2)] public float progress;

        public bool isSetDuration;
        public float duration;

        [EditorIntent(-2)]
        public float speed=1;

        public bool isPause;
        bool isPauseLast;

        [Header("Debug")]
        public float clipTime;

        PlayableGraph graph;
        AnimationClipPlayable clipPlay;

        //[EditorButton(onClickCall = "Test")]public bool isTest;

        async void Test()
        {
            Start();
            while (true)
            {
                Update();
                await Task.Delay(16);
            }
        }


        public class AnimationEndHandler : PlayableBehaviour
        {
            public Action OnDone;
            public override void OnBehaviourPause(Playable playable, FrameData info)
            {
                if (playable.GetGraph().IsPlaying() && playable.IsDone())
                    OnDone?.Invoke();
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            if (!clip)
                return;
            
            var anim = GetComponent<Animator>();
            AnimationPlayableOutput output = default;
            clipPlay = PlayableTools.PlayClip(anim, clip, ref graph, ref output);

            var handlerPlay = ScriptPlayable<AnimationEndHandler>.Create(graph);
            var handler = handlerPlay.GetBehaviour();
            handler.OnDone = () => Debug.Log("play done");

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
            Debug.Log(clipPlay.IsDone());
            if (isManual)
            {
                clipPlay.SetProgress(progress,clip.length);
            }
            else
            {
                progress = (float)clipPlay.GetProgress(clip.length);
            }
            if(isSetDuration)
                clipPlay.SetDuration(duration);

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
