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
        static Type handlerType;
        public static Type GetMaterialPropertyHandlerType()
        {
            if (handlerType != null)
                return handlerType;

            var coreModule = MaterialEditorEx.lazyCoreModule.Value;
            handlerType = coreModule.GetType("UnityEditor.MaterialPropertyHandler");
            return handlerType;
        }

        /// <summary>
        /// Get shader.property 's MaterialPropertyHandler
        /// return false(handlerInst null) when no custom material attribute
        /// </summary>
        /// <param name="handlerInst"></param>
        /// <param name="shader"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static bool TryGetMaterialPropertyHandler(ref object handlerInst, Shader shader,string propName)
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

            var handlerType = GetMaterialPropertyHandlerType();
            handlerInst = handlerType.InvokeMethod("GetHandler", new[] { typeof(Shader), typeof(string) } , null, new object[] { shader, propName });
            // get handler
            //var GetHandleFunc = handlerType.GetMethod("GetHandler", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Shader), typeof(string) }, null);
            //handlerInst = GetHandleFunc.Invoke(null, new object[] { shader,propName});
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