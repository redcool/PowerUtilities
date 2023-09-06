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
    /// material vector, 
    /// 1 not show,
    /// 2 values read from 4 properties
    /// </summary>
    public class VectorValuesDrawer : MaterialPropertyDrawer
    {
        string[] props;
        public VectorValuesDrawer(string propNames)
        {
            if(!string.IsNullOrEmpty(propNames))
                props = propNames.Split(' ').Where(item=> !string.IsNullOrEmpty(item)).ToArray();
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (props == null || prop.type != MaterialProperty.PropType.Vector)
                return;
            var mat = (Material)editor.target;

            var v = new Vector4();
            props.ForEach((propName, id) =>
            {
                v[id] = mat.GetFloat(propName);
            });
            prop.vectorValue = v;
        }
    }
}
#endif