using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    public static class UniversalAdditionalCameraDataEx
    {
        /// <summary>
        /// Get current Renderer's index
        /// </summary>
        /// <param name="cameraData"></param>
        /// <returns></returns>
        public static int GetRendererIndex(this UniversalAdditionalCameraData cameraData)
        {
            var cr = cameraData.scriptableRenderer;
            var rs = UniversalRenderPipeline.asset.GetRenderers();
            return rs.FindIndex(r => r == cr);
        }

        /// <summary>
        /// Get current RendererData
        /// </summary>
        /// <param name="cameraData"></param>
        /// <returns></returns>
        public static T GetRendererData<T>(this UniversalAdditionalCameraData cameraData) where T: ScriptableRendererData
        {
            var id = GetRendererIndex(cameraData);
            var datas = UniversalRenderPipeline.asset.GetRendererDatas();
            return (T)datas[id];
        }
        /// <summary>
        /// Camera use default renderer?
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsDefaultScriptableRenderer(this UniversalAdditionalCameraData data)
            => UniversalRenderPipeline.asset.scriptableRenderer == data.scriptableRenderer;

    }
}
