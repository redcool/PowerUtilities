using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class DelegateEx
    {
        public static Delegate[] GetInvocationListSafe(this Delegate d)
        {
            return d != null ? d.GetInvocationList() : Array.Empty<Delegate>();
        }
    }
}
