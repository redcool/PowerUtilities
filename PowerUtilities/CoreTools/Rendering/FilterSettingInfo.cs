﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{

    [Serializable]
    public struct RangeInfo
    {
        public int min, max;
        public RangeInfo(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
    }

    /// <summary>
    /// like FilteringSettings,can serialized
    /// </summary>
    [Serializable]
    public class FilteringSettingsInfo
    {
        public bool enabled=true;
        public LayerMask layers;
        public RangeInfo renderQueueRangeInfo = new RangeInfo { min=0, max=5000 };

        public RangeInfo sortingLayerRangeInfo = new RangeInfo { min=short.MinValue, max=short.MaxValue };
        [RenderingLayerMask] public uint renderingLayerMasks = 1;
        public bool excludeMotionVectors;

        public static implicit operator FilteringSettings(FilteringSettingsInfo info)
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
        public static implicit operator FilteringSettingsInfo(FilteringSettings settings)
        {
            return new FilteringSettingsInfo
            {
                excludeMotionVectors =  settings.excludeMotionVectorObjects,
                layers = settings.layerMask,
                renderingLayerMasks = settings.renderingLayerMask,
                renderQueueRangeInfo= new RangeInfo(settings.renderQueueRange.lowerBound, settings.renderQueueRange.upperBound),
                sortingLayerRangeInfo = new RangeInfo(settings.sortingLayerRange.lowerBound, settings.sortingLayerRange.upperBound)
            };
        }

        public static void SetupFilterSettingss(ref FilteringSettings settings, FilteringSettingsInfo info)
        {
            settings.excludeMotionVectorObjects = info.excludeMotionVectors;
            settings.layerMask = info.layers;
            settings.renderingLayerMask = info.renderingLayerMasks;
            settings.sortingLayerRange = new SortingLayerRange((short)info.sortingLayerRangeInfo.min, (short)info.sortingLayerRangeInfo.max);
            settings.renderQueueRange = new RenderQueueRange(info.renderQueueRangeInfo.min, info.renderQueueRangeInfo.max);
        }

    }
}