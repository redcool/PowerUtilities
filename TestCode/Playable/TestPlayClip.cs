using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PowerUtilities.Test
{
    public class TestPlayClip : MonoBehaviour
    {
        public AnimationClip clip;

        public bool isManualTime;
        [EditorIntent(1)] public float time;

        public bool isSetDuration;
        public float duration;

        [EditorIntent(-1)]
        public float speed=1;

        public bool isPause;
        bool isPauseLast;

        [EditorButton(onClickCall = "Start")]
        public bool isStart;

        [Header("Debug")]
        public float clipTime;

        PlayableGraph graph;
        AnimationClipPlayable clipPlay;

        async void Test()
        {
            Start();
            while (true)
            {
                Update();
                await Task.Delay(16);
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
            //graph.Play();

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

            if (isManualTime)
            {
                clipPlay.SetTime(time);
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
