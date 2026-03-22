#if UNITASK
namespace PowerUtilities.Test
{
    using Cysharp.Threading.Tasks;
    using PowerUtilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;


    public class OllamaClient : MonoBehaviour
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
        public bool isShowThinking;

        [EditorButton(onClickCall = nameof(StartAsk))]
        public bool isStartAsk;

        StringBuilder chatTextSB = new StringBuilder();
        StringBuilder historySB = new StringBuilder();

        readonly string[] modeStrings = { "/api/chat", "/api/generate" };
        public async void StartAsk()
        {
            var url = ollamaUrl + modeStrings[(int)askMode];
            await WaitForAsk(url, modelName, promptText);
        }
        void ClearResponseText()
        {
            chatText = "";
            chatTextSB.Clear();
            historySB.Clear();
        }
        async Task WaitForAsk(string ollamaUrl, string modelName, string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                chatText = "prompt is empty";
                return;
            }

            chatTextSB.AppendLine($"user : {prompt}");
            historySB.AppendLine($"user : {prompt}");

            var json = GetOllamaReqJson(modelName, historySB.ToString(), isStream);

            chatTextSB.AppendLine("assistant:");
            await WaitForResponseStream(ollamaUrl, json);
            chatTextSB.AppendLine();
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

        async Task WaitForResponseStream(string ollamaUrl, string json)
        {
            using (var request = new UnityWebRequest(ollamaUrl, "POST"))
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new OllamaStreamHandler((respChunk) =>
                {
                    if (respChunk.isThinkingDone)
                    {
                        chatTextSB.AppendLine();
                    }
                    // check content
                    if (!string.IsNullOrEmpty(respChunk.message.content))
                    {
                        chatTextSB.Append(respChunk.message.content);
                        chatText = chatTextSB.ToString();
                        historySB.Append(chatText);
                    }

                    //check thinking
                    else if (!string.IsNullOrEmpty(respChunk.message.thinking))
                    {
                        chatTextSB.Append(respChunk.message.thinking);
                        chatText = chatTextSB.ToString();
                    }
                }
                );

                request.SetRequestHeader("Content-Type", "application/json");

                var asyncOp = await request.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    await UniTask.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(request.error);
                }

                chatTextSB.AppendLine();
            }
        }

        IEnumerator WaitForResponseNoStream(string ollamaUrl, string json)
        {
            using (var request = new UnityWebRequest(ollamaUrl, "POST"))
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                chatText = "*";

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    //chatText = request.downloadHandler.text;
                    var ollamaResponse = JsonUtility.FromJson<OllamaResponse>(request.downloadHandler.text);
                    chatText = ollamaResponse.message.content;
                }
                else
                {
                    Debug.LogError(request.error);
                }
            }
        }
    }
}
#endif