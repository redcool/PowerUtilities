#if UNITY_EDITOR
namespace PowerUtilities
{
    using PowerUtilities.Coroutine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEditor.PackageManager;
    using UnityEditor.PackageManager.Requests;
    using UnityEngine;

    //public class PowerPackageManagerEditor : PowerEditor<PowerPackageManger>
    //{
    //    public override bool NeedDrawDefaultUI() => true;
    //    public override void DrawInspectorUI(PowerPackageManger inst)
    //    {
    //    }
    //}

    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Project/PowerPackageManger", isUseUIElment = false)]
    [SOAssetPath("Assets/PowerUtilities/PowerPackageManger.asset")]
    public class PowerPackageManger : ScriptableObject
    {
        [HelpBox]
        public string helpBox = "[PowerUtilities] reference packages";

        public string[] packageNames =
        {
            "com.unity.cinemachine",
            "com.unity.textmeshpro",
            "com.unity.render-pipelines.core",
            "com.unity.render-pipelines.universal",
            "com.unity.timeline",
            "com.unity.recorder",
            "com.unity.splines",
            "com.unity.terrain-tools",
            "com.unity.modules.physics",
            "com.unity.modules.terrain",
            "com.unity.modules.terrainphysics",
            "com.unity.polybrush",
        };

        [Tooltip("install packages")]
        [EditorButton(onClickCall = "AddPackages")]
        public bool isAddPackages;

        static AddRequest[] requests;
        static List<string> packagesExists = new ();

        public void AddPackages()
        {
            CoroutineTool.StartCoroutine(WaitForQueryAndAddPackages());
        }

        IEnumerator WaitForQueryAndAddPackages()
        {
            Debug.Log("List current packages request start.");
            var listRequest = Client.List();

            while (!listRequest.IsCompleted)
                yield return 0;
            Debug.Log("List current packages request completed.");

            packagesExists.Clear();
            listRequest.Result.ForEach(package =>
                {
                    packagesExists.Add(package.name);
                });

            requests = packageNames
            .Where(pn => !string.IsNullOrEmpty(pn) && !packagesExists.Contains(pn))
            .Distinct()
            .Select(packageName => Client.Add(packageName))
            .ToArray();

            // start update check
            EditorApplicationTools.AddEditorUpdate(OnAddUpdate);

            Debug.Log($"need add count : {requests.Length}");
        }

        void OnAddUpdate()
        {
            if(requests == null)
                return;

            requests.ForEach((req ,id)=>
            {
                var packageName = packageNames[id];
                if (req.IsCompleted)
                {
                    switch (req.Status)
                    {
                        case StatusCode.Success:
                            Debug.Log($"Package {req.Result.name} added successfully.");
                            break;
                        case StatusCode.InProgress:
                            Debug.Log($"Package {packageName} is still processing.");
                            break;
                        case StatusCode.Failure:
                            Debug.LogError($"Failed to add package {req.Result?.name}: {req.Error?.message}");
                            break;
                    }
                }
            });

            var isDone = requests.All(req => req.IsCompleted);
            if (isDone)
            {
                EditorApplication.update -= OnAddUpdate;
                Debug.Log("All packages have been processed.");
            }
        }
    }
}

#endif