using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// PostProcessing Volume Tools
    /// </summary>
    public static class PostProcessVolumeTools
    {
        public static Volume[] GetGlobalVolumes(LayerMask layerMask)
        => VolumeManager.instance.GetVolumes(layerMask)
            .Where(v => v.isActiveAndEnabled && v.isGlobal)
            .ToArray();
        
        public static Volume GetFirstGlobalVolume(LayerMask layerMask)
            => VolumeManager.instance.GetVolumes(layerMask)
            .Where(v => v.isActiveAndEnabled && v.isGlobal)
            .FirstOrDefault();

    }
}
