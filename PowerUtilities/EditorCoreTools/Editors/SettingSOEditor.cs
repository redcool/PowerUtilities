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
        /// Field Type
        /// </summary>

        Editor targetEditor;
        bool isTargetEditorFolded = true;
        Type settingSOType;

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 250;
            serializedObject.UpdateIfRequiredOrScript();

            var settingProp = serializedObject.FindProperty(SettingSOFieldName);

            settingProp.TryGetType(ref settingSOType);

            EditorGUITools.DrawSettingSO(settingProp, ref targetEditor, ref isTargetEditorFolded, settingSOType);

            serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif