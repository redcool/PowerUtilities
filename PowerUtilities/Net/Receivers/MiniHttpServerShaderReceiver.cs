using UnityEngine;

namespace PowerUtilities.Net
{
    public static class MiniHttpServerShaderReceiver
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            MiniHttpServerComponent.OnFileReceived -= OnReceiveShaderBundle;
            MiniHttpServerComponent.OnFileReceived += OnReceiveShaderBundle;
        }
        /// <summary>
        /// error in unity editor, device is ok
        /// </summary>
        /// <param name="shaderObjs"></param>
        public static void ReplaceShaderExisted(Shader[] shaderObjs)
        {
            //var renderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            var renderers = Resources.FindObjectsOfTypeAll<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];

                foreach (var shaderObj in shaderObjs)
                {
                    Debug.Log(renderer.sharedMaterial.shader?.name + " -> " + shaderObj.name);
                    if (renderer.sharedMaterial.shader?.name == shaderObj.name)
                    {
                        renderer.sharedMaterial.shader = shaderObj;
                    }
                }
            }
        }

        public static void OnReceiveShaderBundle(string fileName, string fileType, string filePath)
        {
            if (fileType == typeof(AssetBundle).Name)
            {
                var req = AssetBundle.LoadFromFileAsync(filePath);
                req.completed += (op) =>
                {
                    var ab = req.assetBundle;
                    var shaderObjs = ab.LoadAllAssets<Shader>();
                    ReplaceShaderExisted(shaderObjs);

                    ab.Unload(false);
                };

            }
        }
    }
}