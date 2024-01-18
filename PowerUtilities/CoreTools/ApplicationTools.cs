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
            Application.quitting -= Application_quitting;
            Application.quitting += Application_quitting;
        }

        private static void Application_quitting()
        {
            var methods = ReflectionTools.GetMethodsHasAttribute<ApplicationExitAttribute>();

            foreach (var method in methods)
            {
                if (!method.IsStatic)
                {
                    continue;
                }

                method.Invoke(null, null);
            }
        }
    }
}
