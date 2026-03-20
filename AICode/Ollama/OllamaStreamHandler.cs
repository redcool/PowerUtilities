namespace PowerUtilities.Test
{

    using System;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Networking;

    [Serializable]
    public class OllamaMessage
    {
        public string role;
        public string content;
        public string thinking;
    }
    [Serializable]
    public class OllamaRequest
    {
        public string model;
        public OllamaMessage[] messages;
        public bool stream;
    }
    /**
     {"model":"qwen3.5",
"created_at":"2026-03-09T10:26:30.4748491Z",
"message":{"role":"assistant",
"content":"你好！我是通义千问（Qwen）3.5，是阿里巴巴最新推出的超大规模语言模型。我可以帮助你完成各种任务，比如：\n\n- **复杂推理**：解决数学问题、逻辑谜题；\n- **代码开发**：写代码、调试、生成完整项目；\n- **多语言支持**：流畅切换全球 100+ 种语言；\n- **内容创作**：写故事、公文、邮件等；\n- **专业分析**：解析图表、长文档或科学数据；\n- **超长上下文**：处理数十万字的文件或视频内容。\n\n如果你有具体需求（比如“帮我写一个 Python 爬虫”或“总结这篇报告”），随时告诉我！ □",
"thinking":"好的，用户问“你是”，看起来问题不完整。我需要先确认他们的意图。可能他们想问我是什么，比如我的身份或功能。因为我是 Qwen3.5，我应该介绍自己的主要能力和特点。根据提供的知识库，我需要列出核心升级点，但要注意不要堆砌参数。\n\n首先，我应该明确回答用户的问题，说明我是通义千问系列中最新推出的 Qwen3.5 模型。然后简要说明我的能力和优势，比如逻辑推理、视觉解析、多语言支持等。需要避免使用技术术语，保持口语化。同时，要确保信息准确，不夸大。\n\n可能用户想知道我能做什么，或者有什么限制。我应该突出实用场景，比如处理复杂任务、生成代码、分析图表等。还要注意回复简洁，因为用户的问题很简短，可能需要直接回答，而不是冗长的介绍。\n\n检查是否有遗漏的关键点，比如是否提到支持 256K 上下文窗口，或者多语言能力。需要涵盖这些，但用更自然的方式表达。同时，避免使用 markdown，保持纯文本。最后，邀请用户提问或给出具体任务，促进进一步互动。"
},
"done":true,
"done_reason":"stop",
"total_duration":8113117600,
"load_duration":5174090800,
"prompt_eval_count":11,
"prompt_eval_duration":29974700,
"eval_count":392,
"eval_duration":2390777300
}
     */
    public class OllamaResponse
    {
        public string model;
        public string created_at;
        public OllamaMessage message;
        public string done;
        public string done_reason;
        public int total_duration;
        public int load_duration;
        public int prompt_eval_count;
        public int prompt_eval_duration;
        public int eval_count;
        public int eval_duration;
    }
    [Serializable]
    public class OllamaResponseChunk
    {
        public bool done;
        public OllamaMessage message;
    }


    public class OllamaStreamHandler : DownloadHandlerScript
    {
        public Action<string> onChunkReceived;

        public OllamaStreamHandler(Action<string> callback)
        {
            onChunkReceived = callback;
        }
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || data.Length == 0)
                return false;
            var chunk = Encoding.UTF8.GetString(data, 0, dataLength);
            //Debug.Log(chunk);
            // { "model":"qwen3.5","created_at":"2026-03-18T06:32:32.8335017Z","message":{ "role":"assistant","content":"处理"},"done":false}
            var lines = chunk.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                try
                {
                    var responseJson = JsonUtility.FromJson<OllamaResponseChunk>(line);
                    if (responseJson != null && responseJson.message != null)
                    {
                        onChunkReceived?.Invoke(responseJson.message.content);
                    }
                }
                catch (Exception e)
                {
                    // Handle JSON parsing errors or other exceptions
                    Debug.Log(e);
                }
            }
            return true;
        }
    }
}