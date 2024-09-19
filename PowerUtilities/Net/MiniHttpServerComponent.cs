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

        [Tooltip("relative path : Assets")]
        public string resourceFolder = "../resourceFolder";
        public bool isShowDebugInfo;

        [Tooltip("Only run in Development Build")]
        public bool isDebugBuildOnly = true;

        /// <summary>
        /// call ,when receive file
        /// </summary>
        public static event Action<string,string,string> OnFileReceived;

        private void Awake()
        {
            if (isDebugBuildOnly && !Debug.isDebugBuild)
                return;

            httpServer = new MiniHttpServer(port);
            httpServer.isShowDebugInfo = isShowDebugInfo;

            // handle receive a file
            httpServer.OnReceived += OnHandleFile;
        }

        private void OnDestroy()
        {
            OnFileReceived = null;
        }

        private void MiniHttpServerComponent_OnFileReceived(string fileName,string fileType, string filePath)
        {
            if(isShowDebugInfo)
                Debug.Log($"[server]: get file, name: {fileName}, type : {fileType} ,path :" + filePath);

            OnFileReceived?.Invoke(fileName, fileType, filePath);
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
            var readCount = req.InputStream.Read(bytes, 0, bytes.Length);

            //-------------- save file
            var folder = $"{Application.temporaryCachePath}/{resourceFolder}";
            var outputPath = $"{folder}/{fileName}";

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

            MiniHttpServerComponent_OnFileReceived(fileName, fileType, outputPath);

            if (isShowDebugInfo)
            {
                var sb = new StringBuilder();
                sb.AppendLine("[server], handle file request:");
                sb.AppendLine(req.ContentType);
                sb.AppendLine(outputPath);
                Debug.Log(sb);
            }
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
