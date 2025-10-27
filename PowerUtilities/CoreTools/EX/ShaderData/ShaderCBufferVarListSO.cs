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
    public class ShaderCBufferVarListSO : ScriptableObject
    {
        public List<ShaderCBufferVar> shaderCBufferVarList = new List<ShaderCBufferVar>();
    }
}
