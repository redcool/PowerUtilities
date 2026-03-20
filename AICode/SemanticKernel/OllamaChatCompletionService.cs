using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerUtilities.Test
{
    public class OllamaChatCompletionService : IChatCompletionService
    {
        IOllamaApiClient ollamaApiClient;

        public OllamaChatCompletionService(IOllamaApiClient client)
        {
            this.ollamaApiClient = client;
        }

        public IReadOnlyDictionary<string, object> Attributes => new Dictionary<string, object>();

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory,
            PromptExecutionSettings executionSettings = null,
            Kernel kernel = null,
            CancellationToken cancellationToken = default)
        {
            var req = CreateChatRequest(chatHistory);

            var contentSB = new StringBuilder();

            List<ChatResponseStream> innerContent = new List<ChatResponseStream>();

            AuthorRole? role = null;

            await foreach (var resp in ollamaApiClient.ChatAsync(req, cancellationToken))
            {
                if (resp == null || resp.Message == null)
                    continue;
                innerContent.Add(resp);
                if (resp.Message.Content is not null)
                    contentSB.Append(resp.Message.Content);

                role = new AuthorRole(resp.Message.Role.ToString());
            }

            return new[]{
                new ChatMessageContent
                {
                    Role = role ?? AuthorRole.Assistant,
                    Content = contentSB.ToString(),
                    InnerContent = innerContent,
                    ModelId = "qwen3.5",
                }
            };
        }

        public static ChatRequest CreateChatRequest(ChatHistory history)
        {
            var messgeList = new List<Message>();
            foreach (var message in history)
            {
                messgeList.Add(new Message { 
                    Content = message.Content,
                    Role = message.Role.Label, 
                });
            }
            return new ChatRequest
            {
                Messages = messgeList,
                Stream = true,
                Model = "qwen3.5",
            };
        }

        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings executionSettings = null,
            Kernel kernel = null,
            CancellationToken cancellationToken = default)
        {
            var req = CreateChatRequest(chatHistory);

            await foreach (var resp in ollamaApiClient.ChatAsync(req, cancellationToken))
            {
                yield return new StreamingChatMessageContent(
                    role: new AuthorRole(resp.Message.Role.ToString()),
                    content: resp.Message.Content,
                    innerContent: resp,
                    modelId: resp.Model
                    );
            }
        }


    }
}