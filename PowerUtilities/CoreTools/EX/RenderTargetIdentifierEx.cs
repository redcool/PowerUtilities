using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class RenderTargetIdentifierEx
    {
        public static bool IsTargetIdEquals(this RenderTargetIdentifier a, RenderTargetIdentifier b)
        {
            var t = typeof(RenderTargetIdentifier);

            var f = t.GetField("m_NameID", BindingFlags.Instance | BindingFlags.NonPublic);
            var aId = (int)f.GetValue(a);
            var bId = (int)f.GetValue(b);
            return aId == bId;
        }
    }
}
