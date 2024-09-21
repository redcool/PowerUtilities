namespace PowerUtilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using Object = UnityEngine.Object;
#if UNITY_EDITOR
    using UnityEditor;
#endif

#if UNITY_EDITOR
    [CustomEditor(typeof(MiniHttpClientComponent))]
    public class MiniHttpClientComponentEditor : PowerEditor<MiniHttpClientComponent>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override void DrawInspectorUI(MiniHttpClientComponent inst)
        {
            if (GUILayout.Button("Test Post"))
            {
                //PostFile(inst.fileObj, inst);
                //PostFile(inst.shaderObj, inst);

                inst.PostFile();
            }

        }
        public static void PostFile(Object obj, MiniHttpClientComponent inst)
        {
            if (obj == null)
                return;

            var assetPath = AssetDatabase.GetAssetPath(obj);
            var assetName = Path.GetFileName(assetPath);
            var bytes = File.ReadAllBytes(PathTools.GetAssetAbsPath(assetPath));
            var fileType = obj.GetType().Name;
            MiniHttpClient.PostFile(inst.url, assetName, fileType, bytes);
        }
    }

#endif

    public class MiniHttpClientComponent : MonoBehaviour
    {
        [Tooltip("server url")]
        public string url;
        //public Texture fileObj;
        //public Shader shaderObj;
        public bool isShowDebugInfo;

        [Tooltip("AssetBundle,or use file extension name")]
        public MiniHttpFileType fileType = MiniHttpFileType.AssetBundle;

        [Tooltip("http headers additional ")]
        [ListItemDraw("key:,key,value:,value", "50,.4,50,.3")]
        public List<MiniHttpKeyValuePair> headerPairList = new List<MiniHttpKeyValuePair>();

        [Tooltip("read file from")]
        public string bundleAbsPath;

        public void PostFile()
        {
            var assetName = Path.GetFileName(bundleAbsPath);
            var assetExtName = Path.GetExtension(bundleAbsPath);

            var bytes = File.ReadAllBytes(bundleAbsPath);
            var fileTypeStr = fileType == MiniHttpFileType.ByExtensionName ? assetExtName : typeof(AssetBundle).Name;

            MiniHttpClient.PostFile(url, assetName, fileTypeStr, bytes, isShowDebugInfo, headerPairList);
        }

    }
}
