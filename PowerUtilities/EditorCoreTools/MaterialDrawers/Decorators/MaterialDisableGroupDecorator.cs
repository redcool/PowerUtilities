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
    /// show DisableGroup in groupAPI
    /// </summary>
    public class MaterialDisableGroupDecorator : MaterialPropertyDrawer
    {

        public string targetPropName;
        public MaterialDisableGroupDecorator(string targetPropName)
        {
            this.targetPropName = targetPropName;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            //var targetProp = MaterialEditor.GetMaterialProperty(editor.targets, targetPropName);
            ////EditorGUI.LabelField(position, targetPropName +":"+ targetProp.floatValue);

            //EditorGUI.BeginDisabledGroup(targetProp.floatValue < 1);
            //EditorGUI.indentLevel++;
        }

        /// <summary>
        /// check properpty MaterialDisableGroupDecorator
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static bool IsPropertyDisabled(MaterialEditor editor, string propName)
        {
            //1 check attribute exists
            if (!editor.HasPropertyAttribute(propName, "DisableGroup"))
                return false;

            //2 get targetPropName is setted?
            var disableGroupAttr = editor.GetPropertyAttribute<MaterialDisableGroupDecorator>(propName);
            if (string.IsNullOrEmpty(disableGroupAttr.targetPropName))
                return false;

            var disableTargetProp = editor.GetProperty(disableGroupAttr.targetPropName);
            return disableTargetProp.floatValue == 0;
        }

    }
}
#endif