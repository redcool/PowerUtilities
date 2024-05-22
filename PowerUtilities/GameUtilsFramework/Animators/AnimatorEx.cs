using PowerUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameUtilsFramework
{
    public static class AnimatorEx
    {
        static Dictionary<string, int> animatorLayerIdDict = new Dictionary<string, int>();
        static Dictionary<string, int> animatorParamaterIdDict = new Dictionary<string, int>();

        public static int GetLayerIndex(this Animator anim, string layerName)
        {
            return DictionaryTools.Get(animatorLayerIdDict, layerName, layerName => anim.GetLayerIndex(layerName));
        }

        public static int GetParameterId(string paramName)
        {
            return DictionaryTools.Get(animatorParamaterIdDict, paramName, paramName => Animator.StringToHash(paramName));
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
            return anim.GetCurrentAnimatorStateInfo(GetLayerIndex(anim, layerName));
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
        /// <summary>
        /// move around target
        /// </summary>
        /// <param name="anim"></param>
        /// <param name="idX"></param>
        /// <param name="idZ"></param>
        /// <param name="inputDir"></param>
        /// <param name="damplingTime"></param>
        public static void PlayBlendMove(this Animator anim, int idX, int idZ, Vector3 inputDir, float damplingTime = 0.1f, float xOffset = 0, float zOffset = 0)
        {
            float speedX = Vector3.Dot(anim.transform.right, inputDir.normalized) + xOffset;
            float speedZ = Vector3.Dot(anim.transform.forward, inputDir.normalized) + zOffset;

            //if (Mathf.Abs(speedX) < 0.001f)
            //    speedX = 0;

            //if (Mathf.Abs(speedZ) < 0.001f)
            //    speedZ = 0;

            anim.SetFloat(idX, speedX, damplingTime, Time.deltaTime);
            anim.SetFloat(idZ, speedZ, damplingTime, Time.deltaTime);
        }

        public static void PlayBlendMove(this Animator anim, string nameX, string nameZ, Vector3 inputDir, float damplingTime = 0.1f, float xOffset = 0, float zOffset = 0)
        {
            int idX = GetParameterId(nameX);
            var idZ = GetParameterId(nameZ);
            PlayBlendMove(anim, idX, idZ, inputDir, damplingTime,xOffset,zOffset);
        }

        /// <summary>
        /// Set all layers
        /// </summary>
        /// <param name="anim"></param>
        /// <param name="weight"></param>
        public static void SetLayersWeight(this Animator anim, float weight)
        {
            for (int i = 0; i < anim.layerCount; i++)
            {
                anim.SetLayerWeight(i, weight);
            }

        }

        public static void SetLayersWeight(this Animator anim,int[] layers, float[] weights)
        {
            if (layers.Length != weights.Length)
                return;

            for (int i = 0; i < layers.Length; i++)
            {
                anim.SetLayerWeight(layers[i], weights[i]);
            }

        }
    }
}
