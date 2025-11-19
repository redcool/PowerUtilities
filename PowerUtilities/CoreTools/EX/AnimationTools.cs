using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PowerUtilities
{
    public static class AnimationTools
    {
        public static AnimationState UseFirstState(this Animation anim)
        {
            foreach (AnimationState state in anim)
            {
                anim.clip = state.clip;
                return state;
            }
            return null;
        }

        public static AnimationClip[] GetAnimationClips(this Animation anim)
        {
            var list = new List<AnimationClip>();
            foreach (AnimationState state in anim)
            {
                list.Add(state.clip);
            }
            return list.ToArray();
        }
    }
}
