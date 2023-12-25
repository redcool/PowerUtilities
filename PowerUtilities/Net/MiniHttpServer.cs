using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities.Net
{
    public class MiniHttpServer
    {
        public int port = 8000;
        public event Action<HttpListenerContext> OnReceived;

        public bool isShowDebugInfo;

        HttpListener listener = new HttpListener();
        Task<HttpListenerContext> getContextTask;

        public static string[] GetIPv4s()
        {
            var ips = Dns.GetHostAddresses(Dns.GetHostName());
            return ips.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(ip => ip.ToString())
                .ToArray();
        }

        public static void AddHttpPrefixes(HttpListener listener,int port)
        {
            var ips = GetIPv4s();
            foreach (var ip in ips)
            {
                listener.Prefixes.Add($"http://{ip}:{port}/");
            }
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        }

        public MiniHttpServer(int port)
        {
            this.port = port;

            AddHttpPrefixes(listener, port);
        }


        public void StartListen()
        {
            if (isShowDebugInfo)
            {
                ShowURLPrefixes();
            }

            listener.Start();
        }



        public void StopListen()
        {
            listener.Stop();
        }

        /// <summary>
        /// check http connect per frame,call in Update
        /// </summary>
        public void TryAccept()
        {
            if (!listener.IsListening)
            {
                listener.Start();
            }
            
            if (getContextTask == null)
            {
                getContextTask = listener.GetContextAsync();
            }

            if (getContextTask.IsCompleted)
            {
                var context = getContextTask.Result;
                OnReceived?.Invoke(context);
                getContextTask = null;

                if (isShowDebugInfo)
                {
                    ShowAcceptInfo(context);
                }
            }
        }
        private void ShowURLPrefixes()
        {
            var sb = new StringBuilder();
            foreach (var ip in listener.Prefixes)
            {
                sb.AppendLine(ip);
            }

            Debug.Log($"start listen: {sb.ToString()}");
        }
        private void ShowAcceptInfo(HttpListenerContext context)
        {
            var req = context.Request;

            var sb = new StringBuilder();
            sb.AppendLine(req.Url.ToString());
            sb.AppendLine(req.HttpMethod);
            sb.AppendLine(req.UserHostName);
            sb.AppendLine(req.UserAgent);
            Debug.Log(sb);
        }
    }
}
