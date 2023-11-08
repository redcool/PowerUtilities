#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    [ProjectSettingGroup("PowerUtils/PowerUtilSettings")]
    [SOAssetPath("Assets/PowerUtilities/PowerUtilSettings.asset")]
    public class PowerUtilSettings :ScriptableObject
    {
        [Tooltip("check this,when need click LightmapPreviewWindow select lightmap object")]
        public bool isCheckLightmapPreviewWin = false;
    }
}
#endif