#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace PowerUtilities
{
    /// <summary>
    /// check material keyworks
    /// </summary>
    public class SyncMaterialKeywordsWindow : EditorWindow
    {

        Shader shaderObj;
        string toggleTypeString = "GroupToggle";
        

        [MenuItem(MaterialAnalysisWindow.MENU_PATH+"/SyncMaterialKeywords")]
        static void Init()
        {
            var win = GetWindow<SyncMaterialKeywordsWindow>();
            win.Show();

        }

        private void OnGUI()
        {
            GUILayout.BeginVertical("Box");

            EditorGUITools.BeginHorizontalBox(() =>
            {
                EditorGUILayout.PrefixLabel("Shader:");
                shaderObj = (Shader)EditorGUILayout.ObjectField(shaderObj, typeof(Shader), false);
            });


            EditorGUITools.BeginHorizontalBox(() =>
            {
                EditorGUILayout.PrefixLabel("Toggle Type");
                toggleTypeString = EditorGUILayout.TextField(toggleTypeString);

            });

            EditorGUI.BeginDisabledGroup(!shaderObj);
            {
                EditorGUILayout.LabelField("Update materials keywords by material toggle.");
                if (GUILayout.Button("Sync Prop Keywords"))
                {
                    var mats = shaderObj.GetMaterialsRefShader();
                    var result = shaderObj.SyncMaterialKeywords(toggleTypeString, mats);
                    Debug.Log(result);
                }

                EditorGUILayout.LabelField("Clear keywords reference target shader.");
                if (GUILayout.Button("Clear Materials Keywords"))
                {
                    shaderObj.ClearMaterialKeywords();
                }
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
        }


    }
}
#endif