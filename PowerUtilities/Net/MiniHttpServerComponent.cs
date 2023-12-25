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

        //public event Action<string,byte[]> OnFileReceived;

        private void Awake()
        {
            httpServer = new MiniHttpServer(port);
            httpServer.isShowDebugInfo = true;

            httpServer.OnReceived += OnHandleFile;

            //OnFileReceived += MiniHttpServerComponent_OnFileReceived;
        }

        //private void MiniHttpServerComponent_OnFileReceived(string filetype,byte[] bytes)
        //{
        //    Debug.Log(filetype);
        //    if(filetype == typeof(Shader).Name)
        //    {
        //        var content = Encoding.UTF8.GetString(bytes);
        //        var path = 
        //        File.WriteAllText(path, content);
        //    }

        //}

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
            var readCount = req.InputStream.Read(bytes);

            var folder = Application.temporaryCachePath;
            var outputPath = $"{folder}/{fileName}";
            //MiniHttpServerComponent_OnFileReceived(fileType, outputPath);

            //-------------- save file
            //if (!isWriteFiletemporaryCachePath)
            //    return;

            if (File.Exists(outputPath))
                File.Delete(outputPath);

            //File.WriteAllText($"{folder}/req.txt", Encoding.UTF8.GetString(bytes));
            File.WriteAllBytes(outputPath, bytes);

            bytes = Encoding.UTF8.GetBytes($"{outputPath}");
            resp.OutputStream.WriteAsync(bytes).AsTask().ContinueWith((obj) =>
            {
                resp.Close();
            });

            var sb = new StringBuilder();
            sb.AppendLine("handle file request:");
            sb.AppendLine(req.ContentType);
            sb.AppendLine(outputPath);
            Debug.Log(sb);
        }

        public static List<List<byte>> MultipartFormDataParse(HttpListenerRequest req)
        {
            var list = new List<List<byte>>();

            var reader = new StreamReader(req.InputStream);
            var boundary = reader.ReadLine();

            List<byte> byteList = null;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line.Contains(boundary))
                {

                    continue;
                }
                // new part
                if (line.StartsWith("\n"))
                {
                    byteList = new List<byte>();
                    list.Add(byteList);
                    continue;
                }
                if(byteList != null)
                    byteList.AddRange(Encoding.UTF8.GetBytes(line));


            }


            return list;
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
