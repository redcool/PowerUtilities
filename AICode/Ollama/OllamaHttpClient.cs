namespace PowerUtilities.Test
{
    using Cysharp.Threading.Tasks;
    using PowerUtilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    public class OllamaHttpClient : MonoBehaviour
    {
        public enum AskMode
        {
            Chat = 0,
            Generate = 1
        }


        [Header("Chat")]
        [TextArea(10, 20)]
        public string chatText;
        [EditorButton(onClickCall = nameof(ClearResponseText))]
        public bool isClear;

        [Header("Promt")]
        public AskMode askMode;

        public string ollamaUrl = "http://localhost:11434";


        [TextArea(5, 20)]
        public string promptText;
        public string modelName = "qwen3.5";
        public bool isStream = true;

        [EditorButton(onClickCall = nameof(StartAsk))]
        public bool isStartAsk;

        StringBuilder chatTextSB = new StringBuilder();

        readonly string[] modeStrings = { "/api/chat", "/api/generate" };
        public async void StartAsk()
        {
            var url = ollamaUrl + modeStrings[(int)askMode];
            await WaitAsk(url, modelName, promptText);
        }
        void ClearResponseText()
        {
            chatText = "";
            chatTextSB.Clear();
        }
        async Task WaitAsk(string ollamaUrl, string modelName, string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                chatText = "prompt is empty";
                return;
            }

            chatTextSB.AppendLine($"user : {prompt}");

            var json = GetOllamaReqJson(modelName, prompt, isStream);
            await WaitResponseStream(ollamaUrl,json);
        }

        private static string GetOllamaReqJson(string modelName, string prompt, bool isStream)
        {
            //{"model":"qwen3.5","messages":[{"role":"user","content":""}],"stream":false}
            //string json = $"{{\"model\":\"{modelName}\",\"messages\":[{{\"role\":\"user\",\"content\":\"{prompt}\"}}],\"stream\":false}}";

            var ollamaRequest = new OllamaRequest
            {
                model = modelName,
                messages = new OllamaMessage[]
                {
                new OllamaMessage
                {
                    role = "user",
                    content = prompt
                }
                },
                stream = isStream
            };
            string json = JsonUtility.ToJson(ollamaRequest);
            return json;
        }

        static HttpClient client = new HttpClient();
        async Task WaitResponseStream(string ollamaUrl, string json)
        {
            Debug.Log(ollamaUrl);
            var reqMessage = new HttpRequestMessage(HttpMethod.Post,ollamaUrl);
            reqMessage.Content = new StringContent(json,Encoding.UTF8,"application/json");

            var resp = await client.SendAsync(reqMessage, HttpCompletionOption.ResponseHeadersRead);
            if (!resp.IsSuccessStatusCode)
            {
                var errorBody = await resp.Content.ReadAsStringAsync();
                Debug.Log($"code:{resp.StatusCode} , {errorBody}");
            }

            resp.EnsureSuccessStatusCode();
            
            chatTextSB.AppendLine("assistant:");

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var jsonLine = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(jsonLine))
                    continue;

                //{ "model":"qwen3.5","created_at":"2026-03-20T05:15:23.9522131Z","message":{ "role":"assistant","content":"","thinking":" chat"},"done":false}
                // { "model":"qwen3.5","created_at":"2026-03-18T06:32:32.8335017Z","message":{ "role":"assistant","content":"处理"},"done":false}
                //Debug.Log(jsonLine);
                var respChunk = JsonUtility.FromJson<OllamaResponse>(jsonLine);

                chatTextSB.Append(respChunk.message.thinking);
                chatTextSB.Append($"{respChunk.message.content}");

                if (chatTextSB.Length > 0)
                    chatText = chatTextSB.ToString();
                //await UniTask.Yield(PlayerLoopTiming.Update);
                await UniTask.Delay(10);
            }

        }

    }

}