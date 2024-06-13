#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public static class MaterialPropertyHandlerTools
    {
        public static Type GetMaterialPropertyHandlerType()
        {
            var coreModule = MaterialEditorEx.lazyCoreModule.Value;
            var handlerType = coreModule.GetType("UnityEditor.MaterialPropertyHandler");
            return handlerType;
        }


        public static bool TryGetMaterialPropertyHandler(ref Type handlerType,ref object handlerInst, Shader shader,string propName)
        {
            /** ( draw material attribute ) original version
            MaterialPropertyHandler handler = MaterialPropertyHandler.GetHandler(((Material)base.target).shader, prop.name);
            if (handler != null)
            {
                handler.OnGUI(ref position, prop, (label.text != null) ? label : new GUIContent(prop.displayName), this);
                if (handler.propertyDrawer != null)
                {
                    return;
                }
            }
            */

            var coreModule = MaterialEditorEx.lazyCoreModule.Value;
            if (coreModule == null)
                return false;

            handlerType = GetMaterialPropertyHandlerType();
            // get handler
            var GetHandleFunc = handlerType.GetMethod("GetHandler", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Shader), typeof(string) }, null);
            handlerInst = GetHandleFunc.Invoke(null, new object[] { shader,propName});
            return handlerInst != null;
        }
        /// <summary>
        /// MaterialPropertyHandler's m_DecoratorDrawers
        /// </summary>
        /// <param name="handlerInst"></param>
        /// <returns></returns>
        public static List<MaterialPropertyDrawer> GetMaterialDecoratorDrawers(object handlerInst)
        {
            var type = handlerInst.GetType();
            var targetField = type.GetMemberValue<List<MaterialPropertyDrawer>>("m_DecoratorDrawers", handlerInst, default);
            return targetField;
        }

    }
}
#endif