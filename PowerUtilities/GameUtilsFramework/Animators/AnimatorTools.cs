using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameUtilsFramework
{
    public static class AnimatorTools
    {
        static Dictionary<string, int> animatorLayerIdDict = new Dictionary<string, int>();

        public static int GetLayerIndexFromCache(this Animator anim, string layerName)
        {
           if(!animatorLayerIdDict.TryGetValue(layerName,out var id))
            {
                id = animatorLayerIdDict[layerName] = anim.GetLayerIndex(layerName);
            }
            return id;
        }

        public static float QuantifyInputValue(float v)
        {
            if (v > 0.02f && v < 0.55f)
                return 0.5f;
            if (v > 0.55f)
                return 1;
            if (v < -0.02f && v > -0.55f)
                return -0.5f;
            if (v < -0.55f)
                return -1;
            return v;
        }

        public static AnimatorStateInfo GetCurrentStateInfo(this Animator anim,string layerName)
        {
            return anim.GetCurrentAnimatorStateInfo(anim.GetLayerIndexFromCache(layerName));
        }

        public static bool IsInState(this Animator anim,string layerName,string stateName)
        {
            return GetCurrentStateInfo(anim,layerName).IsName(stateName);
        }

        public static void PlayAnimSetBool(this Animator anim,string stateName, string varName, bool varValue,float transitionDuraion=0.2f)
        {
            anim.SetBool(varName, varValue);
            anim.CrossFade(stateName, transitionDuraion);
        }
    }
}
