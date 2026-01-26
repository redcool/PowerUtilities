using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class RendererEx
    {
        /// <summary>
        /// usage : go.GetComponent<Renderer>()?.SetEnable()
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="isEnable"></param>
        /// <returns></returns>
        public static bool SetEnable(this Renderer renderer, bool isEnable)
        {
            if (!renderer) 
                return false;
            return renderer.enabled = isEnable;
        }
        /// <summary>
        /// Get float property list from all sharedMaterials
        /// </summary>
        /// <param name="r"></param>
        /// <param name="propName"></param>
        /// <param name="list"></param>
        public static void GetSharedMatsFloat(this Renderer r,string propName,ref List<float> list)
        {
            if(list == null)
                list = new List<float>();

            list.Clear();
            foreach(var m in r.sharedMaterials)
            {
                if(m&&m.HasProperty(propName))
                {
                    list.Add(m.GetFloat(propName));
                }
            }
        }
        public static float[] GetSharedMatsFloat(this Renderer r, string propName)
        {
            return r?.sharedMaterials?.Select(m => m.GetFloat(propName)).ToArray();
        }

        public static void SetSharedMatsFloat(this Renderer r, string propName, float value)
        {
            foreach (var m in r.sharedMaterials)
            {
                if (m && m.HasProperty(propName))
                {
                    m.SetFloat(propName, value);
                }
            }
        }

        public static void SetSharedMatsFloat(this Renderer r, string propName, IList<float> list)
        {
            if (list == null)
                return;

            for (int i = 0; i < r.sharedMaterials.Length; i++)
            {
                var m = r.sharedMaterials[i];
                if (m && m.HasProperty(propName))
                {
                    if (i >= list.Count)
                        break;
                    m.SetFloat(propName, list[i]);
                }
            }
        }

        /// <summary>
        /// Get vector property list from all sharedMaterials
        /// </summary>
        /// <param name="r"></param>
        /// <param name="propName"></param>
        /// <param name="list"></param>
        public static void GetSharedMatsVector(this Renderer r, string propName, ref List<float> list)
        {
            if (list == null)
                list = new List<float>();

            list.Clear();
            foreach (var m in r.sharedMaterials)
            {
                if (m && m.HasProperty(propName))
                {
                    list.Add(m.GetFloat(propName));
                }
            }
        }
    }
}
