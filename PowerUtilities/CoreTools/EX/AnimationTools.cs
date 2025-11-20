using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
        /// <summary>
        /// Get AnimationClips from Animation
        /// in EditorMode , iterator m_Animations
        /// in Player solo, iterator AnimationState
        /// </summary>
        /// <param name="anim"></param>
        /// <returns></returns>
        public static AnimationClip[] GetAnimationClips(this Animation anim)
        {
            var list = new List<AnimationClip>();
#if UNITY_EDITOR
            var animSO = new SerializedObject(anim);
            var arrayProp = animSO.FindProperty("m_Animations");
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                var clipProp = arrayProp.GetArrayElementAtIndex(i);
                if (clipProp.objectReferenceValue is AnimationClip clip)
                    list.Add(clip);
            }
#else
            foreach (AnimationState state in anim)
            {
                list.Add(state.clip);
            }
#endif

            return list.ToArray();

        }
    }
}
