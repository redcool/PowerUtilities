using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// Corresponding UnityEngine.Rendering.FilteringSettings
    /// </summary>
    [Serializable]
    public class SimpleFilterSetting
    {
        /// <summary>
        /// culling layer
        /// </summary>
        public LayerMask layers = -1;
        /// <summary>
        /// render queue
        /// </summary>
        public RangeInfo renderQueueRangeInfo = new RangeInfo { min = 0, max = 5000 };
        /// <summary>
        /// soringLayerRange
        /// </summary>
        public RangeInfo sortingLayerRangeInfo = new RangeInfo { min = short.MinValue, max = short.MaxValue };
        /// <summary>
        /// rendering layer
        /// </summary>
        [RenderingLayerMask] public uint renderingLayerMasks = 1;
        /// <summary>
        /// need motion vector?
        /// </summary>
        public bool excludeMotionVectors;

        public static implicit operator FilteringSettings(SimpleFilterSetting info)
        {
            return new FilteringSettings
            {
                excludeMotionVectorObjects = info.excludeMotionVectors,
                layerMask = info.layers,
                renderingLayerMask = info.renderingLayerMasks,
                sortingLayerRange = new SortingLayerRange((short)info.sortingLayerRangeInfo.min, (short)info.sortingLayerRangeInfo.max),
                renderQueueRange = new RenderQueueRange(info.renderQueueRangeInfo.min, info.renderQueueRangeInfo.max)
            };
        }
        public static implicit operator SimpleFilterSetting(FilteringSettings settings)
        {
            return new SimpleFilterSetting
            {
                excludeMotionVectors = settings.excludeMotionVectorObjects,
                layers = settings.layerMask,
                renderingLayerMasks = settings.renderingLayerMask,
                renderQueueRangeInfo = new RangeInfo(settings.renderQueueRange.lowerBound, settings.renderQueueRange.upperBound),
                sortingLayerRangeInfo = new RangeInfo(settings.sortingLayerRange.lowerBound, settings.sortingLayerRange.upperBound)
            };
        }
    }
}
