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
        public string resourceFolder = "resourceFolder";
        public bool isShowDebugInfo;

        [Tooltip("Only run in Development Build")]
        public bool isDebugBuildOnly = true;

        [Header("Debug")]
        public string finalSaveFolder;
        /// <summary>
        /// call ,when receive file
        /// 
        /// string fileName,string fileType, string filePath(abs)
        /// </summary>
        public static event Action<string,string,string, List<MiniHttpKeyValuePair> > OnFileReceived;

        private void Awake()
        {
            if (isDebugBuildOnly && !Debug.isDebugBuild)
                return;

            httpServer = new MiniHttpServer(port);
            httpServer.isShowDebugInfo = isShowDebugInfo;

            // handle receive a file
            httpServer.OnReceived += OnHandleFile;

            finalSaveFolder = GetFinalSaveFolder();
        }

        private void OnDestroy()
        {
            OnFileReceived = null;
        }

        private void FileReceived(string fileName,string fileType, string filePath, List<MiniHttpKeyValuePair> headers=null)
        {
            if(isShowDebugInfo)
                Debug.Log($"[server]: get file, name: {fileName}, type : {fileType} ,path :" + filePath);

            OnFileReceived?.Invoke(fileName, fileType, filePath, headers);
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

            string outputPath = SaveReceivedBytes(req, fileName);

            // get request headers
            List<MiniHttpKeyValuePair> headers = req.Headers.AllKeys
                .Select(k => new MiniHttpKeyValuePair { key = k, value = req.Headers.Get(k) })
                .ToList();

            // send response
            SendResponseBytes(resp, outputPath);

            // notice file received event
            FileReceived(fileName, fileType, outputPath, headers);

            if (isShowDebugInfo)
            {
                ShowFileReceivedLog(req, outputPath);
            }

            //============= inner methods
            static void ShowFileReceivedLog(HttpListenerRequest req, string outputPath)
            {
                var sb = new StringBuilder();
                sb.AppendLine("[server], handle file request:");
                sb.AppendLine(req.ContentType);
                sb.AppendLine(outputPath);
                Debug.Log(sb);
            }

            string SaveReceivedBytes(HttpListenerRequest req, string fileName)
            {
                var bytes = new byte[req.ContentLength64];
                var readCount = req.InputStream.Read(bytes, 0, bytes.Length);

                //-------------- save file
                string folder = GetFinalSaveFolder();
                var outputPath = $"{folder}/{fileName}";

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                if (File.Exists(outputPath))
                    File.Delete(outputPath);


                //File.WriteAllText($"{folder}/req.txt", Encoding.UTF8.GetString(bytes));
                File.WriteAllBytes(outputPath, bytes);
                return outputPath;
            }

            static void SendResponseBytes(HttpListenerResponse resp, string outputPath)
            {
                var bytesResp = Encoding.UTF8.GetBytes($"[server] File Received : {outputPath}");
#if UNITY_2020
            resp.OutputStream.Write(bytesResp, 0, bytesResp.Length);
            resp.Close();
#else
                resp.OutputStream.WriteAsync(bytesResp).AsTask().ContinueWith((obj) =>
                {
                    resp.Close();
                });
#endif
            }
        }

        public string GetFinalSaveFolder()
        {
            return $"{Application.temporaryCachePath}/{resourceFolder}";
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
