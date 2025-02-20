#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PowerUtilities
{
    public class EditorBuildProcessor : IPostprocessBuildWithReport,IPreprocessBuildWithReport
    {
        public static event Action OnPreBuild,OnPostBuild;
        public static event Action<BuildReport> OnPreBuildWithReport, OnPostBuildWithReport;
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            OnPostBuild?.Invoke();
            OnPostBuildWithReport?.Invoke(report);
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            OnPreBuild?.Invoke();
            OnPreBuildWithReport?.Invoke(report);
        }
    }
}
#endif