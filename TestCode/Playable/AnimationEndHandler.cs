using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PowerUtilities.Test
{
    public class AnimationEndHandler : PlayableBehaviour
    {
        public Action OnDone;
        bool isDone;
        
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (isDone)
                return;

            var clipPlay = (AnimationClipPlayable)playable.GetInput(0);
            if (clipPlay.IsValid() && clipPlay.GetProgress() >= 1f)
            {
                isDone = true;
                OnDone?.Invoke();
            }
        }

    }
}
