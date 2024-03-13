namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// Draw MonoBehaviour or RenderFeature 's settingSO(ScriptableObject)
    /// <br/>
    /// ! need a CustomEditor which can be empty  otherwise unity IMGUI will error
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EditorSettingSOAttribute : PropertyAttribute
    {
        public Type settingType;
        public EditorSettingSOAttribute(Type settingType)
        {
            this.settingType = settingType;
        }
    }
}
