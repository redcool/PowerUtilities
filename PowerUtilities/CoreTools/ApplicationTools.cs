using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace PowerUtilities
{
    public static class ApplicationTools
    {
        [RuntimeInitializeOnLoadMethod]
        static void OnLoad()
        {
            Application_quitting();

            Application.wantsToQuit -= Application_quitting;
            Application.wantsToQuit += Application_quitting;

        }

        private static bool Application_quitting()
        {
            var methods = ReflectionTools.GetMethodsHasAttribute<ApplicationExitAttribute>("Power");

#if UNITY_EDITOR
            Debug.Log("quit, dispose methods:"+methods.Length);
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
