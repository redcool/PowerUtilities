using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace PowerUtilities.Timeline
{
    public static class VolumeDataTools
    {
        public static void Update(Volume clipVolume,IvolumeData data)
        {
            var setting = clipVolume.GetOrAddTemporaryProfile().GetOrAddSetting(data.ComponentType);
            data.UpdateSetting(setting);
        }
    }
}
