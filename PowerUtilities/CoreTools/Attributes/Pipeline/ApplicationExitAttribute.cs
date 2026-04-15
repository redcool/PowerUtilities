using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Application exit call
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ApplicationExitAttribute : Attribute
    {
        public static bool CallMethodsHasApplicationExit()
        {
            var methods = ReflectionTools.GetMethodsHasAttribute<ApplicationExitAttribute>("Power");

#if UNITY_EDITOR
            Debug.Log("quit, dispose methods:" + methods.Length);
#endif
            foreach (var method in methods)
            {
                if (!method.IsStatic)
                {
                    continue;
                }

                method.Invoke(null, null);
            }
            return true;
        }
    }


}
