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
    /// custom Editor with scriptableObject profile
    /// </summary>
    public class SettingSOEditor : Editor
    {
        /// <summary>
        /// FieldName of MonoBehaviour or ScriptableObject
        /// </summary>
        public virtual string SettingSOFieldName => "settingSO";
        /// <summary>
        /// Type for create instance
        /// </summary>
        public virtual Type SettingSOType { get; set; }


        Editor targetEditor;
        bool isTargetEditorFolded = true;

        public override void OnInspectorGUI()
        {
            if(SettingSOType == null)
            {
                EditorGUILayout.SelectableLabel("SettingSOType is null, need assign subclass of ScriptableObject");
                return;
            }

            var settingProp = serializedObject.FindProperty(SettingSOFieldName);
            if(settingProp == null)
            {
                EditorGUILayout.SelectableLabel($"{SettingSOFieldName} not found, is name error?");
                return;
            }

            //EditorGUIUtility.labelWidth = 250;
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUITools.DrawSettingSO(settingProp, ref targetEditor, ref isTargetEditorFolded, SettingSOType);

            serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif