namespace PowerUtilities.RenderFeatures
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.SceneManagement;
    using Object = UnityEngine.Object;

    [Tooltip("save (color,depth) to disk")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/BakeTarget")]
    public class BakeTarget : SRPFeature
    {
        [Header("")]
        public int width = 1920;
        public int height = 1080;
        public Material blitMat;
        public RenderTexture tempRT;

        public bool isSave;

        public override ScriptableRenderPass GetPass() => new BakeTargetPass(this);
    }

    public class BakeTargetPass : SRPPass<BakeTarget>
    {
        public BakeTargetPass(BakeTarget feature) : base(feature)
        {
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var width = Feature.width;
            var height = Feature.height;

            if (RenderingTools.IsNeedCreateTexture(Feature.tempRT, width, height))
                Feature.tempRT = new RenderTexture(width, height, 0,RenderTextureFormat.DefaultHDR);

            //var tempID = Shader.PropertyToID("_tempId");
            //cmd.GetTemporaryRT(tempID, cameraData.cameraTargetDescriptor);
            cmd.Blit(BuiltinRenderTextureType.CurrentActive, Feature.tempRT);

            if (!Feature.isSave)
                return;

            if (Feature.isSave)
                Feature.isSave = false;
            
            var scene = SceneManager.GetActiveScene();
            var path = $"{Path.GetDirectoryName(scene.path)}/{scene.name}.png";


            //if (SystemInfo.supportsAsyncGPUReadback)
            //{
            //    GPUTools.AsyncGPUReadRenderTexture(tempRT, 4, bytes =>
            //    {
            //        File.Delete(path);
            //        File.WriteAllBytes(path, bytes);
            //    });
            //}
            //else
            {
                var tex = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
                GPUTools.ReadRenderTexture(Feature.tempRT, ref tex);

                File.Delete(path);
                File.WriteAllBytes(path, tex.EncodeToPNG());

                Object.DestroyImmediate(tex);
            }
        }
    }
}
