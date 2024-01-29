using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// override depth state 
    /// </summary>
    [Serializable]
    public class DepthStateInfo
    {
        public bool isOverrideDepthState;
        public bool isWriteDepth = true;
        public CompareFunction compareFunc = CompareFunction.LessEqual;
    }
}
