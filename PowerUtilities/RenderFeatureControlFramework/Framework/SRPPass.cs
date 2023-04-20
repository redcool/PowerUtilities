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

    //public abstract class SRPPass : ScriptableRenderPass
    //{
    //    public bool enabled;
    //}

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

        public virtual bool CanExecute() => !(
            Feature == null || !Feature.enabled 
            || (!string.IsNullOrEmpty(Feature.cameraTag) && !camera.CompareTag(Feature.cameraTag))
            );

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            this.camera = cameraData.camera;
            this.context = context;
            

            if (! CanExecute())
                return;

            var cmd = CommandBufferPool.Get(Feature.name);
            cmd.Execute(ref context);
            cmd.Clear();

            OnExecute(context, ref renderingData,cmd);

            cmd.Execute(ref context);
            cmd.Clear();
        }

        public abstract void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd);
    }
}
