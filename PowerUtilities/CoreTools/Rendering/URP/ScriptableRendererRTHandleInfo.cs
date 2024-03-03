using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// ScriptableRenderer's RTHandle 
    /// </summary>
    public class ScriptableRendererRTHandleInfo
    {
        public Dictionary<URPRTHandleNames, RTHandle> rtHandleDict = new Dictionary<URPRTHandleNames, RTHandle>();
    }
}
