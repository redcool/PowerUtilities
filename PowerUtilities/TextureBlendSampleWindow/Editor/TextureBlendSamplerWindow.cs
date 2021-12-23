using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureBlendSamplerWindow : EditorWindow
{
    Material mat;
    DefaultAsset folderAsset;
    int outputTextureSize = 2048;


    [MenuItem("PowerUtilities/TextureBlendSamplerWindow/Open")]
    static void Open()
    {
        var win = GetWindow<TextureBlendSamplerWindow>();
        win.Show();
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginVertical("Box");
        mat = (Material)EditorGUILayout.ObjectField(mat, typeof(Material), false);
        outputTextureSize = EditorGUILayout.IntSlider(outputTextureSize, 512, 4096);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Box");
        folderAsset = (DefaultAsset)EditorGUILayout.ObjectField(folderAsset,typeof(DefaultAsset),false);
        EditorGUILayout.EndVertical();

        if (mat && folderAsset)
        {
            if(GUILayout.Button("Save"))
                Save();
        }
        else
        {
            EditorGUILayout.HelpBox("need assign a material.", MessageType.Warning);
        }
    }

    private void Save()
    {
        var folderPath = PathTools.GetAssetDir( AssetDatabase.GetAssetPath(folderAsset) );

        // render
        var rt = RenderTexture.GetTemporary(1, 1);
        var resultTex = GPUTools.Render(outputTextureSize, outputTextureSize, rt, mat);
        RenderTexture.ReleaseTemporary(rt);

        // save
        folderPath = PathTools.GetAssetAbsPath(folderPath);
        File.WriteAllBytes(folderPath + "/BlendTexs.png", resultTex.EncodeToPNG());

        AssetDatabase.Refresh();
    }
}
