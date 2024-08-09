using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class VolumeEx
    {
        /// <summary>
        /// Get v.profile or v.sharedProfile
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static VolumeProfile GetProfile(this Volume v, bool isSharedFirst=true)
        {
            var p = v.HasInstantiatedProfile() ? v.profile : v.sharedProfile;
            if (isSharedFirst && v.sharedProfile)
            {
                p = v.sharedProfile;
            }
            return p;
        }
    }
}
