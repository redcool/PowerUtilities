using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities.Net
{
    public static class MiniHttpClient
    {
        static HttpClient client = new HttpClient();

        public static void PostFile(string url, string fileName, string fileType, byte[] bytes,bool isShowDebugInfo=false,List<MiniHttpKeyValuePair> headersList=null)
        {
            var bytesContent = new ByteArrayContent(bytes);
            bytesContent.Headers.Add("filename", fileName);
            bytesContent.Headers.Add("filetype", fileType);
            bytesContent.Headers.Add("Content-Type", "application/file");

            // add others header kv
            if(headersList != null ) { 
                foreach(MiniHttpKeyValuePair pair in headersList)
                {
                    if(pair.IsValid())
                        bytesContent.Headers.Add(pair.key, pair.value);
                }
            }

            var task = client.PostAsync(url, bytesContent);
            if (isShowDebugInfo)
            {
                task.ContinueWith(t =>
                {
                    Debug.Log($"[client] task status: {t.Status}");
                    Debug.Log("[client], resp:" + t.Result.Content.ReadAsStringAsync().Result);
                });
            }
        }

        static void PostMultipart(string url, string fileName, byte[] bytes, bool isShowDebugInfo = false)
        {
            //client.BaseAddress
            var bytesContent = new ByteArrayContent(bytes);
            //bytesContent.Headers.ContentType
            bytesContent.Headers.Add("filename", fileName);

            var httpContent = new MultipartFormDataContent()
            {
                {bytesContent,"texture",fileName },
                {new StringContent("test123"),"string" }
            };

            //httpContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
            httpContent.Headers.Add("filename", fileName);

            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));

            var task = client.PostAsync(url, httpContent);
            if (isShowDebugInfo)
            {
                task.ContinueWith(t =>
                {
                    Debug.Log($"[client] task status: {t.Status}");
                    Debug.Log("[client], resp:" + t.Result.Content.ReadAsStringAsync().Result);
                });
            }
        }
    }
}
