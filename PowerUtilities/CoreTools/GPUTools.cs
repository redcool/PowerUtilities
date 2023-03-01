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

        public static void AsyncGPUReadRenderTexture(RenderTexture rt, int pixelByteCount = 4, Action<byte[]> onDone = null)
        {
            if (onDone == null || !rt)
                return;

            var buffer = new NativeArray<byte>(rt.width * rt.height * pixelByteCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            AsyncGPUReadback.RequestIntoNativeArray(ref buffer, rt, 0, request =>
            {
                if (request.hasError)
                {
                    buffer.Dispose();
                    return;
                }
                using var enc = ImageConversion.EncodeNativeArrayToPNG(buffer, rt.graphicsFormat, (uint)rt.width, (uint)rt.height);
                onDone(enc.ToArray());
                buffer.Dispose();
            });
        }

        public static void ReadRenderTexture(RenderTexture sourceTex,ref Texture2D targetTex)
        {
            if (!sourceTex)
                return;
            
            if(!targetTex)
                targetTex = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.ARGB32, false, true);

            RenderTexture.active = sourceTex;
            targetTex.ReadPixels(new Rect(0, 0, sourceTex.width, sourceTex.height), 0, 0, false);
            RenderTexture.active = null;
        }
    }
}
//#endif