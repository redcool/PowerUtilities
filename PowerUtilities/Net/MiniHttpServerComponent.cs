using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities.Net
{
    public class MiniHttpServerComponent : MonoBehaviour
    {
        public int port = 8000;
        public MiniHttpServer httpServer;
        public bool isWriteFiletemporaryCachePath;

        [Tooltip("relative path : Assets")]
        public string resourceFolder = "../resourceFolder";

        //public event Action<string,byte[]> OnFileReceived;

        private void Awake()
        {
            httpServer = new MiniHttpServer(port);
            httpServer.isShowDebugInfo = true;

            httpServer.OnReceived += OnHandleFile;

            //OnFileReceived += MiniHttpServerComponent_OnFileReceived;
        }

        private void MiniHttpServerComponent_OnFileReceived(string fileName,string fileType, string filePath)
        {
            Debug.Log($"[server]: get file, name: {fileName}, type : {fileType} ,path :"+ filePath);

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
        public static void ReplaceShaderExisted(Shader[] shaderObjs)
        {
            var objs = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            for (int i = 0; i < objs.Length; i++)
            {
                var renderer = objs[i];

                foreach (var shaderObj in shaderObjs)
                {
                    //Debug.Log(renderer.sharedMaterial.shader?.name + " -> " + shaderObj.name);
                    if (renderer.sharedMaterial.shader?.name == shaderObj.name)
                        renderer.sharedMaterial.shader = Shader.Find(shaderObj.name);
                }
            }
        }

        /// <summary>
        /// handle a httpListenerRequest
        /// headers not exists filename,will skip
        /// </summary>
        /// <param name="context"></param>
        private void OnHandleFile(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            // multipart/form-data; boundary="e7274f23-5105-492a-a024-557d664ade79"

            var fileName = req.Headers.Get("filename");
            var fileType = req.Headers.Get("filetype");
            if (string.IsNullOrEmpty(fileName))
                return;

            var bytes = new byte[req.ContentLength64];
            var readCount = req.InputStream.Read(bytes,0, bytes.Length);

            var folder = $"{Application.dataPath}/{ resourceFolder}";
            var outputPath = $"{folder}/{fileName}";

            //-------------- save file
            //if (!isWriteFiletemporaryCachePath)
            //    return;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (File.Exists(outputPath))
                File.Delete(outputPath);


            //File.WriteAllText($"{folder}/req.txt", Encoding.UTF8.GetString(bytes));
            File.WriteAllBytes(outputPath, bytes);

            bytes = Encoding.UTF8.GetBytes($"{outputPath}");
#if UNITY_2020
            resp.OutputStream.Write(bytes, 0, bytes.Length);
            resp.Close();
#else
            resp.OutputStream.WriteAsync(bytes).AsTask().ContinueWith((obj) =>
            {
                resp.Close();
            });
#endif

            MiniHttpServerComponent_OnFileReceived(fileName,fileType, outputPath);

            var sb = new StringBuilder();
            sb.AppendLine("[server], handle file request:");
            sb.AppendLine(req.ContentType);
            sb.AppendLine(outputPath);
            Debug.Log(sb);
        }

        void Update()
        {
            if (httpServer != null)
                httpServer.TryAccept();

#if UNITY_EDITOR
            if(httpServer == null)
            {
                Awake();
            }
#endif
        }
    }
}
