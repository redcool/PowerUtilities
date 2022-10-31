using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerUtilities
{
    public static class MaterialPropCodeGenTools 
    {

        public static void UpdateComponentsMaterial<T>(T[] comps, Action<T,int> onAction) where T : Component
        {
            if (onAction == null || comps == null || comps.Length == 0)
                return;

            for (int i = 0; i < comps.Length; i++)
            {
                onAction(comps[i],i);
            }
        }

    }
}
