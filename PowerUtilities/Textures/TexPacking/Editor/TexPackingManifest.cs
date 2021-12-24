#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEngine;
    using System.Collections;
    using System;

    public class TexPackingManifest : ScriptableObject
    {
        /// <summary>
        /// packing rects
        /// </summary>
        public Rect[] rects;
        /// <summary>
        /// packing texture's instanceID
        /// </summary>
        public int[] instanceIds;

        public Texture atlas;

        public Rect GetRect(int instanceId)
        {
            var index = Array.FindIndex(instanceIds, id => id == instanceId);
            if (index > -1)
                return rects[index];
            return default(Rect);
        }
    }
}
#endif