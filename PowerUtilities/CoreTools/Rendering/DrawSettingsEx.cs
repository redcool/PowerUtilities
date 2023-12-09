using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class DrawSettingsEx
    {
        public static void SetShaderPassNames(this DrawingSettings settings, List<ShaderTagId> tags)
        {
            if (tags == null || tags.Count == 0)
                return;

            for (int i = 0; i < tags.Count; i++)
            {
                settings.SetShaderPassName(i, tags[i]);
            }
        }
    }
}
