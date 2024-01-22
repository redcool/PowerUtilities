namespace PowerUtilities.RenderFeatures
{
    using System.IO;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.SceneManagement;
    using PowerUtilities;
#if UNITY_2020
    using Tooltip = PowerUtilities.TooltipAttribute;
#endif
    [Tooltip("save (color,depth) to disk")]

    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/BakeTarget")]
    public class BakeTarget : SRPFeature
    {
        [Header("BakeTarget")]
        public int width = 1920;
        public int height = 1080;
        [LoadAsset("BakeTarget_CombineTextures.mat")]
        public Material combineBlitMat;
        public RenderTexture tempRT;

        [EditorButton]
        public bool isSave;

        public BakeTarget()
        {
           isEditorOnly = true;
        }

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
            var renderer = cameraData.renderer;

            var width = Feature.width;
            var height = Feature.height;

            if (RenderingTools.IsNeedCreateTexture(Feature.tempRT, width, height))
                Feature.tempRT = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);

            RTHandle depthTexture = null, colorTexture = null;
            RTHandleTools.GetRTHandle(ref depthTexture, renderer, URPRTHandleNames.m_DepthTexture);
            RTHandleTools.GetRTHandle(ref colorTexture, renderer, URPRTHandleNames.m_OpaqueColor);

            cmd.SetGlobalTexture(ShaderPropertyIds.sourceTex2, depthTexture);
            cmd.BlitTriangle(colorTexture, Feature.tempRT, Feature.combineBlitMat, 0);

            // outpout a frame
            if (!Feature.isSave)
            {
                return;
            }

            if (Feature.isSave)
                Feature.isSave = false;

            var scene = SceneManager.GetActiveScene();
            var path = $"{Path.GetDirectoryName(scene.path)}/{scene.name}.png";

            var colorTex = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
            GPUTools.ReadRenderTexture(Feature.tempRT, ref colorTex);

            SaveTexture(path, colorTex);
        }

        private static void SaveTexture(string path, Texture2D colorTex)
        {
            File.Delete(path);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            File.WriteAllBytes(path, colorTex.EncodeToPNG());

#if UNITY_EDITOR
            AssetDatabaseTools.SaveRefresh();
            var texObj = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUIUtility.PingObject(texObj);

#endif
        }

    }
}
