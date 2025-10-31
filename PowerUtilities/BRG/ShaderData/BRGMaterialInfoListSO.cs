using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace PowerUtilities
{
    /// <summary>
    /// ShaderCBufferVar Container
    /// </summary>
    [Serializable]
    public class BRGMaterialInfoListSO : ScriptableObject
    {
        public List<BRGMaterialInfo> brgMaterialInfoList = new List<BRGMaterialInfo>();
    }
}
