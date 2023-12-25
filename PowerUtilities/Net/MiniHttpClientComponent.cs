namespace PowerUtilities.Net
{
using System;
using System.Collections.Generic;
    using System.IO;
    using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [CustomEditor(typeof(MiniHttpClientComponent))]
    public class MiniHttpClientComponentEditor : PowerEditor<MiniHttpClientComponent>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override void DrawInspectorUI(MiniHttpClientComponent inst)
        {
            if(GUILayout.Button("Test Post"))
            {
                PostFile(inst.fileObj,inst);
                PostFile(inst.shaderObj, inst);
            }

            static void PostFile(Object obj, MiniHttpClientComponent inst )
            {
                if (obj == null)
                    return;

                var assetPath = AssetDatabase.GetAssetPath(obj);
                var assetName = Path.GetFileName(assetPath);
                var bytes = File.ReadAllBytes(PathTools.GetAssetAbsPath(assetPath));
                var fileType = obj.GetType().Name;
                MiniHttpClient.PostFile(inst.url, assetName,fileType, bytes);
            }
        }
    }
#endif
    public class MiniHttpClientComponent : MonoBehaviour
    {
        public string url;
        public Texture fileObj;
        public Shader shaderObj;

    }
}
