namespace PowerUtilities
{
#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    /// <summary>
    /// 显示该shader被使用的材质
    /// </summary>
    public class ShaderRefWindow : EditorWindow
    {
        bool isUpdate;
        IEnumerable<Material> materials;

        Vector2 scrollPosition;

        [MenuItem(ShaderAnalysis.SHADER_ANALYSIS+"/显示Shader引用",priority =1)]
        static void Init()
        {
            var win = GetWindow<ShaderRefWindow>();
            win.Show();
        }

        private void OnSelectionChange()
        {
            Repaint();
            isUpdate = true;
        }

        public void OnGUI()
        {

            var shader = Selection.activeObject as Shader;

            if (!shader)
            {
                EditorGUILayout.HelpBox("Select a shader, show reference materilas", MessageType.Info);
                return;
            }

            if (isUpdate)
            {
                materials = ShaderAnalysis.GetShaderInfo(shader);
                isUpdate = false;
            }

            if (materials == null)
                return;
            //---------------- shader
            GUILayout.Label("Shader:");
            EditorGUILayout.ObjectField(shader, typeof(Shader), false);

            //--------- materials
            if (materials.Count() == 0)
                EditorGUILayout.LabelField("No Material used.");
            else
            {
                GUILayout.Label("Materials:");

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, "box");
                GUILayout.BeginVertical("Box");
                materials.ForEach(item =>
                {
                    EditorGUILayout.ObjectField(item, typeof(Material), false);
                });
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
        }


    }
#endif
}