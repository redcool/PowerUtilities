using Azure;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities.Test
{
    public class TestKernelOllama : MonoBehaviour
    {
        [Header("Server")]
        public string url = "http://127.0.0.1:11434";
        public string modelId = "qwen3.5";
        public string apiKey = "ollama";

        [Header("Chat")]
        [TextArea(10, 20)]
        public string chat;
        [EditorButton(onClickCall = "ClearChat")]
        public bool isClearChat;

        [TextArea(5, 10)]
        public string userInput;

        [EditorButton(onClickCall = "Start")]
        public bool isStart;
        public bool isShowThinking;

        Kernel kernel;
        IChatCompletionService chatCompletionService;
        ChatHistory chatHistory = new ChatHistory();
        StringBuilder chatSB = new StringBuilder();

        void ClearChat()
        {
            chatSB.Clear();
            chat = "";
            chatHistory.Clear();
        }

        public async void Start()
        {
            if (string.IsNullOrEmpty(userInput))
                return;

            if (kernel == null)
            {
                var handler = new HttpClientHandler();
                
                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(url),
                };

                var builder = Kernel.CreateBuilder();
                builder.Services.AddScoped<IOllamaApiClient>(_ => new OllamaApiClient(httpClient, modelId));
                builder.Services.AddScoped<IChatCompletionService, OllamaChatCompletionService>();

                kernel = builder.Build();
                //kernel.Plugins.AddFromType<GameControlPlugin>();
            }

            if (chatCompletionService == null)
            {
                chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            }
            //chatHistory.AddSystemMessage("");
            chatHistory.AddUserMessage(userInput);

            await WaitStreamingResp();
            //await WaitResp();
        }

        private async Task WaitResp()
        {
            var resp = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

            chatSB.AppendLine(resp.Content);
            chatHistory.AddMessage(resp.Role, resp.Content ?? "");

            chat = chatSB.ToString();
        }

        async Task WaitStreamingResp()
        {
            Debug.Log("WaitStreamingResp");
            var contentList = chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory);

            var sb = new StringBuilder();
            chatSB.AppendLine("assistant:");
            await foreach (var chunk in contentList)
            {
                // check Thinking
                if (isShowThinking)
                {
                    ChatResponseStream innerContent = chunk.InnerContent as ChatResponseStream;
                    if (innerContent != null)
                    {
                        var thinking = innerContent.Message.Thinking;
                        chatSB.Append(thinking);
                        chat = chatSB.ToString();

                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }
                }

                // check Content
                if (string.IsNullOrEmpty(chunk.Content))
                    continue;

                await UniTask.Yield(PlayerLoopTiming.Update);

                sb.Append(chunk.Content);
                chatSB.Append(chunk.Content);
                chat = chatSB.ToString();
            }
            chatSB.AppendLine();
            
            chatHistory.AddMessage(AuthorRole.Assistant, sb.ToString()?? "");
        }
    }

    public class GameControlPlugin
    {
        [KernelFunction,Description("change color")]
        public string ChangeColor(string colorName)
        {
            Debug.Log("ai change color "+colorName);
            return colorName;
        }
    }
}
