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

        public SRPPass(T feature)
        {
            Feature = feature;
        }

        public virtual bool IsValid(Camera cam) => !(
            Feature == null || !Feature.enabled 
            || (!string.IsNullOrEmpty(Feature.cameraTag) && !cam.CompareTag(Feature.cameraTag))
            );

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            if (! IsValid(cameraData.camera))
                return;

            var cmd = CommandBufferPool.Get(nameof(T));
            cmd.Execute(ref context);
            cmd.Clear();

            OnExecute(context, ref renderingData,cmd);

            cmd.Execute(ref context);
            cmd.Clear();
        }

        public abstract void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd);
    }
}
