using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Draw MonoBehaviour or RenderFeature 's settingSO(ScriptableObject)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EditorSettingSOAttribute : PropertyAttribute
    {
        public string settingFieldName = "settingSO";
        public Type settingType = typeof(ScriptableObject);

        public EditorSettingSOAttribute(Type settingType ,string settingFieldName= "settingSO")
        {
            this.settingType = settingType;
            this.settingFieldName = settingFieldName;
        }
    }
}
