using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    /// <summary>
    /// handle ScriptableRenderer(UniversalRenderer) passes by reflections
    /// </summary>
    public static class ScriptableRendererEx
    {
        /// <summary>
        /// UniversalRenderer pass's private pass names
        /// </summary>

        public enum PassFieldNames
        {
            m_DepthPrepass,
            m_DepthNormalPrepass,
            m_PrimedDepthCopyPass,
            m_MotionVectorPass,
            m_MainLightShadowCasterPass,
            m_AdditionalLightsShadowCasterPass,
            m_GBufferPass,
            m_GBufferCopyDepthPass,
            m_DeferredPass,
            m_RenderOpaqueForwardOnlyPass,
            m_RenderOpaqueForwardPass,
            m_RenderOpaqueForwardWithRenderingLayersPass,
            m_DrawSkyboxPass,
            m_CopyDepthPass,
            m_CopyColorPass,
            m_TransparentSettingsPass,
            m_RenderTransparentForwardPass,
            m_OnRenderObjectCallbackPass,
            m_FinalBlitPass,
            m_CapturePass,

            m_XROcclusionMeshPass,
            m_XRCopyDepthPass,

            m_FinalDepthCopyPass,
            m_ProbeVolumeDebugPass,

            m_DrawOffscreenUIPass,
            m_DrawOverlayUIPass,
        }

        static Dictionary<PassFieldNames, ScriptableRenderPass> passDict = new Dictionary<PassFieldNames, ScriptableRenderPass>();
        static ScriptableRenderer lastRendererInstance;
        /// <summary>
        /// urp passes, control remove from UnviersalRenderer
        /// </summary>
        public enum UrpPassType
        {
            CopyDepthPass,
            FinalBlitPass,
        }
        public const string URP_PASS_NAMESPACE_PREFIX = "UnityEngine.Rendering.Universal.Internal.";

        static Dictionary<UrpPassType, Type> urpPassTypeDict = new Dictionary<UrpPassType, Type>();

        static ScriptableRendererEx()
        {
            SetupURPPassTypeDict();
        }

        private static void SetupURPPassTypeDict()
        {
            urpPassTypeDict.Clear();
            var passTypes = Enum.GetValues(typeof(UrpPassType));
            var dll = typeof(UniversalRenderer).Assembly;

            foreach (UrpPassType passType in passTypes)
            {
                var passName = Enum.GetName(typeof(UrpPassType), passType);
                urpPassTypeDict[passType] = dll.GetType(URP_PASS_NAMESPACE_PREFIX + passName);
            }
        }

        /// <summary>
        /// get renderPass with cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="renderer"></param>
        /// <param name="passName"></param>
        /// <returns></returns>
        public static T GetRenderPass<T>(this ScriptableRenderer renderer, PassFieldNames passName) where T : ScriptableRenderPass
        {
            CheckRendererInstance(renderer);

            var varName = Enum.GetName(typeof(PassFieldNames), passName);

            passDict.TryGetValue(passName, out var value);
            if (value == null)
            {
                value = passDict[passName] = renderer.GetType().GetFieldValue<T>(renderer, varName);
            }

            return value != null ? (T)value : default;
        }

        /// <summary>
        /// renderer vs lastRenderer, if not same instance, clear cached dict
        /// </summary>
        /// <param name="renderer"></param>
        private static void CheckRendererInstance(ScriptableRenderer renderer)
        {
            if (IsNewRendererInstance(renderer))
            {
                passDict.Clear();
            }
        }

        public static bool IsNewRendererInstance(this ScriptableRenderer renderer)
        {
            var isNew = renderer != lastRendererInstance;
            if (isNew)
            {
                lastRendererInstance = renderer;
            }
            return isNew;
        }

        /// <summary>
        /// Get UniversalRenderer's m_ActiveRenderPassQueue
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        public static List<ScriptableRenderPass> GetActiveRenderPassQueue(this ScriptableRenderer renderer)
        {
            var list = typeof(ScriptableRenderer).GetFieldValue<List<ScriptableRenderPass>>(renderer, "m_ActiveRenderPassQueue");
            return list;
        }

        /// <summary>
        /// Remove pass from UnviersalRenderer's m_ActiveRenderPassQueue
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="onPredicate"></param>
        public static void RemoveRenderPass(this ScriptableRenderer renderer,Func<ScriptableRenderPass,bool> onPredicate)
        {
            var list = renderer.GetActiveRenderPassQueue();
            if (list  == null || onPredicate == null)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item == null || !onPredicate(item))
                    continue;

                list.Remove(item);
                i--;
            }
        }

        public static void RemoveRenderPasses(this ScriptableRenderer renderer,List<Type> passTypeList)
        {
            var list = renderer.GetActiveRenderPassQueue();
            if (list == null || passTypeList==null || passTypeList.Count == 0)
                return;

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if(item == null || !passTypeList.Contains(item.GetType()))
                    continue;

                list.Remove(item);
                i--;
            }
        }

        public static Type GetPassType(UrpPassType passType)
        {
            return urpPassTypeDict[passType];
        }
    }
}