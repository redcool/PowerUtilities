using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// urp renderPass controlled by SRPFeatureControl
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SRPPass<T> : ScriptableRenderPass
        where T : SRPFeature
    {
        public T Feature { get; private set; }

        protected Camera camera;
        protected ScriptableRenderContext context;

        public SRPPass(T feature)
        {
            Feature = feature;
        }

        /// <summary>
        /// This pass can execute
        /// 1 check Feature 
        /// 2 check cameraType,
        ///     2.1 check gameCameraTag when cameraType is Game
        /// </summary>
        /// <returns></returns>
        public virtual bool CanExecute()
        { 
            if(Feature == null || !Feature.enabled)
                return false;

            if (camera.cameraType == CameraType.Game &&!string.IsNullOrEmpty(Feature.gameCameraTag))
                return camera.CompareTag(Feature.gameCameraTag);

            return true;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            this.camera = cameraData.camera;
            this.context = context;
            

            if (! CanExecute())
                return;

            var cmd = CommandBufferPool.Get(Feature.name);
            cmd.Execute(ref context);

            OnExecute(context, ref renderingData,cmd);

            cmd.Execute(ref context);
        }

        public abstract void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd);
    }
}
