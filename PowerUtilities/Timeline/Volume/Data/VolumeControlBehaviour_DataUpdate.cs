using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace PowerUtilities.Timeline
{
    public partial class VolumeControlBehaviour
    {
        public Bloom_Data bloomData;


        public void UpdateVolumeSettings()
        {
            if (!clipVolume)
                return;

            VolumeControlClipPostUpdate.Update(clipVolume, bloomData);
        }

        public void ReadSettingsFrom(VolumeProfile vp)
        {

        }
    }
}
