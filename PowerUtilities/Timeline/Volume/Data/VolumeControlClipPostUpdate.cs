using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace PowerUtilities.Timeline
{
    public static class VolumeControlClipPostUpdate
    {
        public static void Update(Volume clipVolume,Bloom_Data data)
        {
            var setting = clipVolume.GetOrAddTemporaryProfile().GetOrAddSetting(data.ComponentType);
            data.UpdateSetting(setting);
        }
    }
}
