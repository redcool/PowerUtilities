namespace PowerUtilities.RenderFeatures
{
    using System.IO;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.SceneManagement;

    [Tooltip("save (color,depth) to disk")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/BakeTarget")]
    public class BakeTarget : SRPFeature
    {
        [Header("")]
        public int width = 1920;
        public int height = 1080;
        public Material combineBlitMat;
        public RenderTexture tempRT;

        public bool isSave;

        public override ScriptableRenderPass GetPass() => new BakeTargetPass(this);
    }

    public class BakeTargetPass : SRPPass<BakeTarget>
    {
        public BakeTargetPass(BakeTarget feature) : base(feature)
        {
        }

        public override bool CanExecute()
        => base.CanExecute() && Feature.combineBlitMat;

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var width = Feature.width;
            var height = Feature.height;

            if (RenderingTools.IsNeedCreateTexture(Feature.tempRT, width, height))
                Feature.tempRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);

            //var tempID = Shader.PropertyToID("_tempId");
            //cmd.GetTemporaryRT(tempID, cameraData.cameraTargetDescriptor);

            //ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.LinearToSRGB);
            cmd.SetGlobalTexture(ShaderPropertyIds.sourceTex2, ShaderPropertyIds._CameraDepthTexture);
            cmd.BlitTriangle(ShaderPropertyIds._CameraOpaqueTexture, Feature.tempRT, Feature.combineBlitMat, 0);

            if (!Feature.isSave)
                return;

            if (Feature.isSave)
                Feature.isSave = false;

            var scene = SceneManager.GetActiveScene();
            var path = $"{Path.GetDirectoryName(scene.path)}/{scene.name}.png";

            var colorTex = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
            GPUTools.ReadRenderTexture(Feature.tempRT, ref colorTex);


            File.Delete(path);
            File.WriteAllBytes(path, colorTex.EncodeToPNG());
        }

        
    }
}
