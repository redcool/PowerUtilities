using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PowerUtilities
{
    public static class AnimationClipPlayableEx
    {

        public static double GetProgress(this AnimationClipPlayable a)
        => a.GetTime() / a.GetAnimationClip().length;

        public static void SetProgress(this AnimationClipPlayable a, float progress)
        {
            var time = a.GetAnimationClip().length * progress;
            a.SetTime(time);
        }
    }
}