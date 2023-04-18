using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName = "SrpRenderFeatures/SRPSettingList")]
    public class SRPFeatureList : ScriptableObject
    {
        public List<SRPFeature> settingList = new List<SRPFeature>();
    }
}
