#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class Material2TextureBaker : EditorWindow
    {
        Material mat;
        string outputPath = "Assets/TextureBlendFolder";
        int outputTextureSize = 2048;


        [MenuItem("PowerUtilities/Material2TextureBaker/Open")]
        static void Open()
        {
            var win = GetWindow<Material2TextureBaker>();
            win.Show();
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Blend Material:");
                mat = (Material)EditorGUILayout.ObjectField(mat, typeof(Material), false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Output Texture Resolution");
                outputTextureSize = EditorGUILayout.IntSlider(outputTextureSize, 512, 4096);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal("Box");
            {
                EditorGUILayout.LabelField("OutputPath:");
                outputPath = EditorGUILayout.TextField(outputPath);
            }
            EditorGUILayout.EndHorizontal();

            if (mat && !string.IsNullOrEmpty(outputPath))
            {
                if (GUILayout.Button("Save"))
                    Save();
            }
            else
            {
                EditorGUILayout.HelpBox("need assign a material.", MessageType.Warning);
            }
        }

        private void Save()
        {
            // render
            var rt = RenderTexture.GetTemporary(1, 1);
            var resultTex = GPUTools.Render(outputTextureSize, outputTextureSize, rt, mat);
            RenderTexture.ReleaseTemporary(rt);

            // save
            PathTools.CreateAbsFolderPath(outputPath);
            var absPath = PathTools.GetAssetAbsPath(outputPath);
            File.WriteAllBytes(absPath + "/BlendTexs.png", resultTex.EncodeToPNG());

            AssetDatabase.Refresh();
        }
    }
}
#endif