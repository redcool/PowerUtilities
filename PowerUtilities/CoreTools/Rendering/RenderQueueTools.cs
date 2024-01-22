using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public enum RenderQueueType
    {
        opaque = 0, transparent = 1, all = 2
    }
    public static class RenderQueueTools
    {
        public static RenderQueueRange ToRenderQueueRange(RenderQueueType type) => type switch
        { 
             RenderQueueType.transparent => RenderQueueRange.transparent,
             RenderQueueType.all => RenderQueueRange.all,
             _ => RenderQueueRange.opaque,
        };
            
    }
}
