namespace PowerUtilities.Test
{
    using PowerUtilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
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

        [EditorButton(onClickCall = nameof(StartAsk))]
        public bool isStartAsk;

        StringBuilder chatTextSB = new StringBuilder();


        public bool isFinishAsk;

        readonly string[] modeStrings = { "/api/chat", "/api/generate" };
        public void StartAsk()
        {
            var url = ollamaUrl + modeStrings[(int)askMode];
            StartCoroutine(WaitForAsk(url, modelName, promptText));
        }
        void ClearResponseText()
        {
            chatText = "";
            chatTextSB.Clear();
        }
        IEnumerator WaitForAsk(string ollamaUrl, string modelName, string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                chatText = "prompt is empty";
                yield break;
            }

            chatTextSB.AppendLine(prompt);

            var json = GetOllamaReqJson(modelName, prompt, isStream);

            StartCoroutine(WaitForResponseStream(ollamaUrl, json));
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

        IEnumerator WaitForResponseStream(string ollamaUrl, string json)
        {
            using (var request = new UnityWebRequest(ollamaUrl, "POST"))
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new OllamaStreamHandler((chunkStr) =>
                {
                    chatTextSB.Append(chunkStr);
                    chatText = chatTextSB.ToString();
                });
                request.SetRequestHeader("Content-Type", "application/json");

                isFinishAsk = false;
                yield return request.SendWebRequest();
                isFinishAsk = true;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(request.error);
                }
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

                isFinishAsk = false;
                chatText = "*";

                yield return request.SendWebRequest();
                isFinishAsk = true;

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