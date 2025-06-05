//#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Rendering;

    public static class GPUTools
    {
        /// <summary>
        /// use gpu render a texture
        /// </summary>
        /// <param name="info"></param>
        /// <param name="source"></param>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static Texture2D Render(int width,int height,Texture source,Material mat)
        {
            var rt = RenderTexture.GetTemporary(width,height);
            Graphics.Blit(source,rt, mat);

            var lastRT = RenderTexture.active;
            var tex = new Texture2D(rt.width,rt.height);

            RenderTexture.active = rt;

            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = lastRT;
            RenderTexture.ReleaseTemporary(rt);
            return tex;
        }


    }
}
//#endif