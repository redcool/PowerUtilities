#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

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
        public SyncValueDrawer(string propName)
        {
            this.propName = propName;
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

            switch (prop.type)
            {
                case MaterialProperty.PropType.Vector:
                    prop.vectorValue = mat.GetVector(propName);
                    break;
                case MaterialProperty.PropType.Color:
                    prop.colorValue = mat.GetColor(propName);
                    break;
                case MaterialProperty.PropType.Texture:
                    prop.textureValue = mat.GetTexture(propName);
                    var targetTexST = (propName + "_ST");
                    if (mat.HasVector(targetTexST))
                        prop.textureScaleAndOffset = mat.GetVector(targetTexST);
                    break;
                default:
                    prop.floatValue = mat.GetFloat(propName);
                    break;
            }

        }
    }
}
#endif