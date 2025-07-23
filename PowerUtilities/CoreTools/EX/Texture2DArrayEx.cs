using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    public static class Texture2DArrayEx
    {
        /// <summary>
        /// Fill texture2dArray with textures,format need same
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="textures"></param>
        public static void Fill<T>(this Texture2DArray arr,List<T> textures) where T : Texture
        {
            textures.ForEach((tex, id) =>
            {
                Graphics.CopyTexture(tex, 0, arr, id);
                //for (int mipId = 0; mipId < tex.mipmapCount; mipId++)
                //{
                //    Graphics.CopyTexture(tex, 0, mipId, arr, id, mipId);
                //}

            });
        }
    }
}