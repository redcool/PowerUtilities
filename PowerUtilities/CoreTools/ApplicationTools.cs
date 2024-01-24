using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace PowerUtilities
{
    public static class ApplicationTools
    {
        public static event Action OnDomainUnload;

        static ApplicationTools()
        {

            AppDomain.CurrentDomain.DomainUnload -= CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
#if UNITY_EDITOR
            Debug.Log($"[] domain unload");
#endif
            if(OnDomainUnload != null) 
                OnDomainUnload();
            OnDomainUnload = null;
        }


    }
}
