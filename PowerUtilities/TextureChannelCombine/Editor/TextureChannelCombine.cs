#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class TextureChannelCombine : EditorWindow
    {

        enum TextureChannel
        {
            R,G,B,A
        }

        enum TextureSize : int
        {
            size512 = 512,size1024=1024,size2048=2048
        }

        class TextureInfo
        {
            public string label;
            public Texture2D tex;
            public TextureChannel channel;
            // shader props
            public string texShaderPropName;
        }


        [MenuItem("PowerUtilities/TextureChannelCombine/Open")]
        static void Init()
        {
            GetWindow<TextureChannelCombine>().Show();
        }

        string helpStr = 
@"图片通道组合
    1 指定图片
    2 指定使用该图片的通道
";

        TextureInfo[] texInfos = new TextureInfo[] {
            new TextureInfo{label="R : " ,channel = TextureChannel.R,texShaderPropName = "_RTex"},
            new TextureInfo{label="G : " ,channel = TextureChannel.G,texShaderPropName = "_GTex"},
            new TextureInfo{label="B : " ,channel = TextureChannel.B,texShaderPropName = "_BTex"},
            new TextureInfo{label="A : " ,channel = TextureChannel.A,texShaderPropName = "_ATex"},
        };

        Vector4[] channelMasks = new Vector4[]
        {
            new Vector4(1,0,0,0),
            new Vector4(0,1,0,0),
            new Vector4(0,0,1,0),
            new Vector4(0,0,0,1),
        };

        DefaultAsset folderAsset;
        TextureSize texSize = TextureSize.size512;
        private void OnGUI()
        {
            EditorGUILayout.HelpBox(helpStr,MessageType.Info);

            //Texture
            EditorGUILayout.BeginVertical("Box");
            foreach (var item in texInfos)
            {
                EditorGUILayout.BeginVertical("Box");
                DrawTexInfo(item);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            // output file path
            EditorGUILayout.BeginHorizontal("Box");
            folderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Output folder:", folderAsset, typeof(DefaultAsset), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("Box");
            texSize = (TextureSize)EditorGUILayout.EnumPopup("output texture size:", texSize);
            EditorGUILayout.EndVertical();

            // buttons
            if (folderAsset && GUILayout.Button("Save"))
            {
                Save();
            }
        }

        void DrawTexInfo(TextureInfo info)
        {
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField(info.label,GUILayout.Width(20));
            info.tex = (Texture2D)EditorGUILayout.ObjectField(info.tex, typeof(Texture2D),false);

            EditorGUILayout.LabelField("Channel Used:",GUILayout.Width(100));
            info.channel = (TextureChannel)EditorGUILayout.EnumPopup(info.channel);

            EditorGUILayout.EndHorizontal();
        }

        void Save()
        {
            var folderPath = AssetDatabase.GetAssetPath(folderAsset);
            if (Path.HasExtension(folderPath))
            {
                folderPath = Path.GetDirectoryName(folderPath);
            }

            // set material;
            int size = (int)texSize;

            var mat = new Material(Shader.Find("Hidden/TextureChannelCombine"));

            foreach (var item in texInfos)
            {
                var tex = item.tex ? item.tex : Texture2D.blackTexture;
                mat.SetTexture(item.texShaderPropName, tex);
                mat.SetVector(item.texShaderPropName + "Mask", channelMasks[(int)item.channel]);
            }

            // render
            var rt = RenderTexture.GetTemporary(1, 1);
            var resultTex = GPUTools.Render(size,size,rt,mat);
            RenderTexture.ReleaseTemporary(rt);

            // save
            folderPath = PathTools.GetAssetAbsPath(folderPath);
            File.WriteAllBytes(folderPath+"/combine.png", resultTex.EncodeToPNG());

            AssetDatabase.Refresh();
        }

    }
}
#endif