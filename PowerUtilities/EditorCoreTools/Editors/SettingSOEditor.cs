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
        public virtual string SettingSOFieldName =>"settingSO";
        public virtual Type SettingSOType => typeof(ScriptableObject);

        Editor targetEditor;
        bool isTargetEditorFolded = true;

        bool IsValid()
        {
            if (!SettingSOType.IsSubclassOf(typeof(ScriptableObject)))
            {
                Debug.LogError($"{SettingSOType} need extends ScriptableObject");
                return false;
            }

            return true;
        }

        public override void OnInspectorGUI()
        {
            if (!IsValid())
                return;

            var settingSOProp = serializedObject.FindProperty(SettingSOFieldName);

            //========================================  gammaUISetting header
            EditorGUILayout.BeginHorizontal();
            //1 exist
            EditorGUILayout.PrefixLabel("SettingSO:");
            settingSOProp.objectReferenceValue = EditorGUILayout.ObjectField(settingSOProp.objectReferenceValue, SettingSOType, false);
            //2 create new
            if (GUILayout.Button("Create New"))
            {
                var so = CreateInstance(SettingSOType);
                var nextId = Resources.FindObjectsOfTypeAll(SettingSOType).Length;

                var settingsFolder = AssetDatabaseTools.CreateFolder("Assets/PowerUtilities", SettingSOType.Name, true);
                var soPath = $"{settingsFolder}/_{SettingSOType.Name}_{nextId}.asset";
                AssetDatabase.CreateAsset(so, soPath);
                AssetDatabaseTools.SaveRefresh();

                //
                var newAsset = AssetDatabase.LoadAssetAtPath(soPath, SettingSOType);
                EditorGUIUtility.PingObject(newAsset);

                settingSOProp.objectReferenceValue = newAsset;
            }
            EditorGUILayout.EndHorizontal();

            //========================================  splitter line 
            var rect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUITools.DrawColorLine(rect);

            //========================================  draw gammaUISetting 
            if (settingSOProp.objectReferenceValue != null)
            {
                EditorTools.CreateEditor(settingSOProp.objectReferenceValue, ref targetEditor);

                isTargetEditorFolded = EditorGUILayout.Foldout(isTargetEditorFolded, "Settings",true);
                if (isTargetEditorFolded)
                    targetEditor.DrawDefaultInspector();

            }
            else
            {
                EditorGUILayout.HelpBox("No Details", MessageType.Info);
            }
        }
    }
}
#endif