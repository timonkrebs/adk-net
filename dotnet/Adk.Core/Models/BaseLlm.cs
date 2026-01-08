using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Adk.Core.Models
{
    public abstract class BaseLlm
    {
        public string Model { get; }

        // This is not easily enforceable as static in abstract class in C# in the same way (can't be abstract static)
        // But we can have a property.
        // Or just omit if it's for registry lookup.
        // static readonly List<Regex> SupportedModels = new List<Regex>();

        protected BaseLlm(string model)
        {
            Model = model;
        }

        public abstract IAsyncEnumerable<LlmResponse> GenerateContentAsync(LlmRequest llmRequest, bool stream = false);

        public abstract Task<BaseLlmConnection> Connect(LlmRequest llmRequest);

        protected virtual Dictionary<string, string> TrackingHeaders
        {
            get
            {
                // Placeholder for client labels
                string headerValue = "adk-dotnet-client";
                return new Dictionary<string, string>
                {
                    { "x-goog-api-client", headerValue },
                    { "user-agent", headerValue }
                };
            }
        }

        public void MaybeAppendUserContent(LlmRequest llmRequest)
        {
            if (llmRequest.Contents.Count == 0)
            {
                llmRequest.Contents.Add(new Google.Cloud.AIPlatform.V1.Content
                {
                    Role = "user",
                    Parts = { new Google.Cloud.AIPlatform.V1.Part { Text = "Handle the requests as specified in the System Instruction." } }
                });
            }

            // Check last content role
             if (llmRequest.Contents.Count > 0 && llmRequest.Contents[llmRequest.Contents.Count - 1].Role != "user")
            {
                llmRequest.Contents.Add(new Google.Cloud.AIPlatform.V1.Content
                {
                    Role = "user",
                    Parts = { new Google.Cloud.AIPlatform.V1.Part { Text = "Continue processing previous requests as instructed. Exit or provide a summary if no more outputs are needed." } }
                });
            }
        }
    }
}
