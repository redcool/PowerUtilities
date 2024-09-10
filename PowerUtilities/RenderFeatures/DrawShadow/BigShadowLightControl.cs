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
        [Tooltip("Clear and stop DrawShadow rendering")]
        public bool isShadowEnabled = true;
        bool lastIsShadowEnabled;

        [Header("Override")]
        [Tooltip("override global DrawShadowSettings")]
        public bool isOverrideSettingSOEnabled;
        bool lastIsOverrideSettingSOEnabled;

        [EditorSettingSO]
        public DrawShadowSettingSO overrideSettingSO;

        /// <summary>
        /// called when isShadowEnabled changed
        /// </summary>
        public event Action<bool> OnShadowEnableChanged;

        public event Action<bool,DrawShadowSettingSO> OnOverrideSettingSO;

        private void Update()
        {
            var isChanged = isShadowEnabled != lastIsShadowEnabled;

            if (OnShadowEnableChanged != null&& CompareTools.CompareAndSet(ref lastIsShadowEnabled, ref isShadowEnabled))
            {
                OnShadowEnableChanged(isShadowEnabled);
            }

            
            if(OnOverrideSettingSO != null && CompareTools.CompareAndSet(ref lastIsOverrideSettingSOEnabled,ref isOverrideSettingSOEnabled))
            {
                OnOverrideSettingSO(isOverrideSettingSOEnabled, overrideSettingSO);
            }
        }

        private void OnDisable()
        {
            OnShadowEnableChanged?.Invoke(false);
            OnOverrideSettingSO?.Invoke(false, null);

            OnShadowEnableChanged = null;
            OnOverrideSettingSO = null;

            //next enable will trigger again
            lastIsOverrideSettingSOEnabled = false;
            lastIsShadowEnabled = false;
        }

    }
}
