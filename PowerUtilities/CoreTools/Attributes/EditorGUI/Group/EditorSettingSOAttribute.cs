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
        /// <summary>
        /// use fieldInfo.FieldType when null
        /// </summary>
        public Type settingType;
        /// <summary>
        /// show List property(create item button)
        /// </summary>
        public string listPropName;
        public EditorSettingSOAttribute(Type settingType=null,string listPropName = "")
        {
            this.settingType = settingType;
            this.listPropName = listPropName;
        }
    }
}
