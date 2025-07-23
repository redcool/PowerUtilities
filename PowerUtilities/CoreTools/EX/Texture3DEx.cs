using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    public static class Texture3DEx
    {
        /// <summary>
        /// Fill texture2dArray with textures
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="textures"></param>
        public static void Fill<T>(this Texture3D arr,List<T> textures) where T : Texture
        {
            textures.ForEach((tex, id) =>
            {
                Graphics.CopyTexture(tex, 0, arr, id);
            });
        }
    }
}