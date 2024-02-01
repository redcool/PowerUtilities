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
        public virtual Type SettingSOType => typeof(ScriptableObject);

        Editor targetEditor;
        bool isTargetEditorFolded = true;

        public override void OnInspectorGUI()
        {
            EditorGUITools.DrawSettingSOProperty(serializedObject, ref targetEditor, ref isTargetEditorFolded, SettingSOFieldName, SettingSOType);
        }

    }
}
#endif