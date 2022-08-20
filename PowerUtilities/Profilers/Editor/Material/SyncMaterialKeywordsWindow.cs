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

            EditorGUITools.BeginHorizontalBox(() => { 
                EditorGUILayout.PrefixLabel("Shader:");
                shaderObj = (Shader)EditorGUILayout.ObjectField(shaderObj, typeof(Shader),false);
            });


            EditorGUITools.BeginHorizontalBox(() => { 
                EditorGUILayout.PrefixLabel("Toggle Type");
                toggleTypeString = EditorGUILayout.TextField(toggleTypeString);
            
            });


            EditorGUILayout.LabelField("Update materials keywords by material toggle.");
            if (GUILayout.Button("Check keywords"))
            {
                var result = CheckKeywords(shaderObj, toggleTypeString);
                Debug.Log(result);
            }

            EditorGUILayout.LabelField("Clear keywords reference target shader.");
            if(GUILayout.Button("Clear Materials Keywords"))
            {
                ClearKeywords(shaderObj);
            }
            GUILayout.EndVertical();
        }



        private static string CheckKeywords(Shader shader,string toggleTypeString)
        {
            var propKeywordList = shader.GetShaderPropsHasKeyword(toggleTypeString);
            Material[] mats = GetMaterialsRefShader(shader);
            var result = new StringBuilder();

            foreach (var mat in mats)
            {
                foreach (var propKeyword in propKeywordList)
                {
                    var keywordOn = mat.GetFloat(propKeyword.propName) != 0;
                    // update keyword
                    if (keywordOn)
                        mat.EnableKeyword(propKeyword.keyword);
                    else
                        mat.DisableKeyword(propKeyword.keyword);

                    // keyword has changed
                    if (mat.IsKeywordEnabled(propKeyword.keyword) != keywordOn)
                    {
                        result.AppendLine(mat.ToString());
                    }
                }
            }
            result.Insert(0, $"len:{mats.Length}\n");
            return result.ToString();
        }

        private static Material[] GetMaterialsRefShader(Shader shader)
        {
            var paths = AssetDatabaseTools.GetSelectedFolders();
            var mats = AssetDatabaseTools.FindAssetsInProject<Material>("t:Material", paths);
            mats = mats.Where(mat => mat.shader == shader).ToArray();
            return mats;
        }

        static void ClearKeywords(Shader shader)
        {
            Material[] mats = GetMaterialsRefShader(shader);
            foreach (var mat in mats)
            {
                mat.shaderKeywords=null;
            }
        }
    }
}
#endif