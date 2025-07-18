#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Tools/ScriptableObjCreator")]
    [SOAssetPath("Assets/PowerUtilities/ScriptableObjCreator.asset")]
    public class ScriptableObjectCreator : ScriptableObject
    {
        [HelpBox]
        public string helpBox = "Create ScriptableObject";

        public MonoScript mono;

        [EditorButton(onClickCall ="CreateSO")]
        public bool isCreate;

        void CreateSO()
        {
            var isValid = mono &&  mono.GetClass().IsSubclassOf(typeof(ScriptableObject));
            if (!isValid)
            {
                Debug.Log("mono is not ScriptableObject");
                return;
            }

            
            var obj = Selection.activeObject;
            var selectedFolder = obj ? AssetDatabaseTools.GetAssetFolder(obj) : "Assets";
            var inst = ScriptableObject.CreateInstance(mono.GetClass());

            var path = $"{selectedFolder}/{mono.GetClass().Name}.asset";

            AssetDatabase.CreateAsset(inst, path);
            AssetDatabaseTools.SaveRefresh();

            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
        }
    }
}
#endif