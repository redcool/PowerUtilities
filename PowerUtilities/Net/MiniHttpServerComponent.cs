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
    public class MiniHttpServerComponent : MonoBehaviour
    {
        public int port = 8000;
        public MiniHttpServer httpServer;

        private void Awake()
        {
            httpServer = new MiniHttpServer(port);
            httpServer.isShowDebugInfo = true;

            httpServer.OnAccept += HttpServer_OnAccept;
        }

        private void HttpServer_OnAccept(HttpListenerContext context)
        {
            Debug.Log("get connect");
            var req = context.Request;
            var resp = context.Response;

            var bytes = Encoding.UTF8.GetBytes("dont");
            resp.OutputStream.WriteAsync(bytes).AsTask().ContinueWith( (obj) => {
                resp.Close();
            });
        }

        void Update()
        {
            if (httpServer != null)
                httpServer.TryAccept();
        }
    }
}
