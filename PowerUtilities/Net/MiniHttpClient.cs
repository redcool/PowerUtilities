﻿using System;
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

        public static void PostFile(string url,string fileName,string fileType,byte[] bytes)
        {
            var bytesContent = new ByteArrayContent(bytes);
            bytesContent.Headers.Add("filename",fileName);
            bytesContent.Headers.Add("filetype", fileType);
            bytesContent.Headers.Add("Content-Type","application/file");

            var task = client.PostAsync(url, bytesContent);
            task.ContinueWith(t =>
            {
                Debug.Log(t.Result.Content.ReadAsStringAsync().Result);
            });
        }

         static void PostMultipart(string url, string fileName, byte[] bytes)
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
            task.ContinueWith(t =>
            {
                Debug.Log(t.Result.Content.ReadAsStringAsync().Result);
            });
        }
    }
}
