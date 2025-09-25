namespace PowerUtilities.SSPR
{
    using System.Threading;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public enum RunMode
    {
        Hash, // cspass(Hash,Hash Resolve)
        CS_PASS_2,// cspass(csMain,csMain)
                  //CS_PASS_1 // cspass(csMain)
    }

    public enum BlurPassMode
    {
        OffsetHalfPixel,
        SinglePass,
        TwoPasses,
    }

    /// <summary>
    /// implements Screen Space Planar Reflection
    /// 
    /// references: 
    /// https://remi-genin.github.io/posts/screen-space-planar-reflections-in-ghost-recon-wildlands/
    /// https://github.com/Steven-Cannavan/URP_ScreenSpacePlanarReflections
    /// https://www.cnblogs.com/idovelemon/p/13184970.html
    /// 
    /// usage:
    /// 1 add SSPRFeature to UniversalRenderPipelineAsset_Renderer
    ///     1.1 ssprFeature add ssprCore
    ///     1.2 change params
    /// 2 add 3D plane to scene
    ///     2.1 assign SSPRFeature/Shaders/ShowReflectionTexture.mat to plane
    ///     2.2 chang plane'mat renderqueue > 2500
    ///     
    /// 
    /// </summary>
    public class SSPRFeature : ScriptableRendererFeature
    {

        [HelpBox(lineCount =7)]
        [SerializeField]
        string helpStr = @"
    version : 0.2
    Usage:
    1 add SSPRFeature to UniversalRenderPipelineAsset_Renderer
        1.1 create Setting SO
        1.2 change params
    2 add 3D plane to scene
        2.1 assign SSPRFeature/Shaders/ShowReflectionTexture.mat to plane
        2.2 chang plane'mat renderqueue > 2500
";

        SSPRPass ssprPass;

        [EditorSettingSO(typeof(SSPRSettingSO))]
        public SSPRSettingSO settingSO;

        /// <inheritdoc/>
        public override void Create()
        {
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when settingSO up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settingSO == null)
                return;

            ref var cameraData = ref renderingData.cameraData;
            var enabled = string.IsNullOrEmpty(settingSO.gameCameraTag) ? true:cameraData.camera.CompareTag(settingSO.gameCameraTag) ;
            enabled = enabled || cameraData.cameraType != CameraType.Game;
            if (!enabled)
                return;

            // close msaa
            if (UniversalRenderPipeline.asset && UniversalRenderPipeline.asset.msaaSampleCount > 1)
            {
                UniversalRenderPipeline.asset.msaaSampleCount = 1;
            }

            if (ssprPass == null)
                ssprPass = new SSPRPass();

            ssprPass.settings = settingSO;
            ssprPass.renderPassEvent = settingSO.renderEvent + settingSO.renderEventOffset;

            renderer.EnqueuePass(ssprPass);
        }
    }


}