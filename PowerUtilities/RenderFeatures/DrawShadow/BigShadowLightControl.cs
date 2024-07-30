using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// DrawShadow read this info
    /// </summary>
    [ExecuteAlways]
    public class BigShadowLightControl : MonoBehaviour
    {
        public bool isShadowEnabled = true;
        bool lastIsShadowEnabled;

        /// <summary>
        /// called when isShadowEnabled changed
        /// </summary>
        public event Action<bool> OnShadowEnableChanged;

        private void Update()
        {
            var isChanged = isShadowEnabled != lastIsShadowEnabled;

            if (CompareTools.CompareAndSet(ref lastIsShadowEnabled, ref isShadowEnabled))
            {
                OnShadowEnableChanged?.Invoke(isShadowEnabled);
            }
        }

        private void OnDestroy()
        {
            OnShadowEnableChanged = null;
        }
    }
}
