#if UNITY_6000_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    public partial class SRPFeature
    {
        /// <summary>
        /// Set from SRPFeatureControl when add pass
        /// </summary>
        public RenderingData renderingData;
    }
}
#endif