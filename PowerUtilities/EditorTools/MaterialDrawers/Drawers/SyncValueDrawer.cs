#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// material value sync, 
    /// 1 not show,
    /// 2 set currrent material value from other mat's propName
    /// </summary>
    public class SyncValueDrawer : MaterialPropertyDrawer
    {
        string propName;
        string keyword;
        public SyncValueDrawer(string propName)
        {
            this.propName = propName;
        }
        public SyncValueDrawer(string propName,string keyword) : this(propName)
        {
            this.keyword = keyword;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (string.IsNullOrEmpty(propName))
                return;

            var mat = (Material)editor.target;

            switch (prop.GetPropertyType())
            {
                case (int)ShaderPropertyType.Vector:
                    prop.vectorValue = mat.GetVector(propName);
                    break;
                case (int)ShaderPropertyType.Color:
                    prop.colorValue = mat.GetColor(propName);
                    break;
                case (int)ShaderPropertyType.Texture:
                    prop.textureValue = mat.GetTexture(propName);
                    var targetTexST = (propName + "_ST");
                    if (mat.HasProperty(targetTexST))
                        prop.textureScaleAndOffset = mat.GetVector(targetTexST);
                    break;
                default:
                    prop.floatValue = mat.GetFloat(propName);
                    if(!string.IsNullOrEmpty(keyword))
                    {
                        mat.SetKeyword(keyword, prop.floatValue > 0f);
                    }
                    break;
            }

        }
    }
}
#endif