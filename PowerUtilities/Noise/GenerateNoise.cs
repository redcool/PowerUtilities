namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public static class GenerateNoise
    {
#if UNITY_EDITOR
        [MenuItem("PowerUtilities/Noise/Generate Noise Textures")]
        static void Init()
        {
            const int size = 512;
            CreateNoiseTex(size, 1, "tex1.png");
            CreateNoiseTex(size, 5, "tex2.png");
            CreateNoiseTex(size, 10, "tex3.png");
            CreateNoiseTex(size, 20, "tex4.png");
            AssetDatabase.Refresh();
        }
#endif

        static void CreateNoiseTex(int size, float scale, string fileName)
        {
            var tex = SimplexNoise.GenerateNoiseTexture(size, size, scale, scale);
            var bytes = tex.EncodeToPNG();

            var dir = Application.dataPath + "/NoiseTex/";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var path = Path.Combine(dir, fileName);
            File.WriteAllBytes(path, bytes);
        }
    }
}