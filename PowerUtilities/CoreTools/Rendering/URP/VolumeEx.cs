using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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

        public static T GetOrAddSetting<T>(this VolumeProfile vp) where T : VolumeComponent
        {
            if(! vp.TryGet<T>(out var setting))
            {
                setting = vp.Add<T>();
            }
            return setting;
        }

        public static VolumeComponent GetOrAddSetting(this VolumeProfile vp,Type compType)
        {
            if (!vp.TryGet(compType,out VolumeComponent setting))
            {
                setting = vp.Add(compType);
            }
            return setting;
        }

        public static VolumeProfile GetOrAddTemporaryProfile(this Volume v)
        {
            if(!v.profile)
                v.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            return v.profile;
        }
    }
}
