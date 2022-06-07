using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameUtilsFramework
{
    public static class AnimatorTools
    {
        public static float QuantifyInputValue(float v)
        {
            if (v > 0.02f && v < 0.55f)
                return 0.5f;
            if (v > 0.55f)
                return 1;
            if (v < -0.02f && v > -0.55f)
                return -0.5f;
            if (v < -0.55f)
                return -1;
            return v;
        }
    }
}
