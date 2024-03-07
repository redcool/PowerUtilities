using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PowerUtilities
{
    public static class ScreenTools
    {
        /// <summary>
        /// Call this when Screen size changed
        /// </summary>
        public static event Action OnScreenSizeChanged;

        static int2 lastCameraRes;
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod()]
#endif
        public static void Start()
        {
            RenderPipelineManager.beginFrameRendering -= CheckScreenSize;
            RenderPipelineManager.beginFrameRendering += CheckScreenSize;
        }

        private static void CheckScreenSize(ScriptableRenderContext ctx, Camera[] cameras)
        {
            if (OnScreenSizeChanged == null)
                return;

            if (lastCameraRes.x != Screen.width || lastCameraRes.y != Screen.height)
            {
                OnScreenSizeChanged();

                lastCameraRes.x = Screen.width;
                lastCameraRes.y = Screen.height;
            }

        }
    }
}
