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

        public virtual string SettingsSOFieldName => "settings";
        /// <summary>
        /// Field Type
        /// </summary>
        public virtual Type SettingSOType => typeof(ScriptableObject);

        Editor targetEditor;
        bool isTargetEditorFolded = true;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var settingProp = serializedObject.FindProperty(SettingSOFieldName);
            EditorGUITools.DrawSettingSO(settingProp, ref targetEditor, ref isTargetEditorFolded, SettingSOType);

            serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif