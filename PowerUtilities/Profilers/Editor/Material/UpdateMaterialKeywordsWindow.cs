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
    public class UpdateMaterialKeywordsWindow : EditorWindow
    {

        Shader shaderObj;
        string toggleTypeString = "GroupToggle";
        

        [MenuItem(MaterialAnalysisWindow.MENU_PATH+"/CheckMaterialKeywords")]
        static void Init()
        {
            var win = GetWindow<UpdateMaterialKeywordsWindow>();
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


            if (GUILayout.Button("Check keywords"))
            {
                var result = CheckKeywords(shaderObj, toggleTypeString);
                Debug.Log(result);
            }
            GUILayout.EndVertical();
        }



        private static string CheckKeywords(Shader shader,string toggleTypeString)
        {
            var propKeywordList = shader.GetShaderPropsHasKeyword(toggleTypeString);


            var paths = AssetDatabaseTools.GetSelectedFolders();
            var mats = AssetDatabaseTools.FindAssetsInProject<Material>("t:Material", paths);
            mats = mats.Where(mat => mat.shader == shader).ToArray();
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
            return result.ToString();
        }


    }
}
#endif