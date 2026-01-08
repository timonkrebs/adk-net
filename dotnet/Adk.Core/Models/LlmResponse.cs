using Google.Cloud.AIPlatform.V1;
using System.Collections.Generic;
using System.Linq;

namespace Adk.Core.Models
{
    // These types are conceptual mappings. In a real .NET implementation using Google.Cloud.AIPlatform.V1,
    // we would map them to the protobuf types.
    // However, the JS code uses @google/genai types which are slightly different from Vertex AI Protobufs.
    // For now, I will use aliases or simple wrappers where appropriate, but try to stick to Vertex AI types.

    // Vertex AI Protobuf types:
    // Content -> Google.Cloud.AIPlatform.V1.Content
    // Part -> Google.Cloud.AIPlatform.V1.Part
    // Candidate -> Google.Cloud.AIPlatform.V1.Candidate
    // GroundingMetadata -> Google.Cloud.AIPlatform.V1.GroundingMetadata
    // GenerateContentResponse -> Google.Cloud.AIPlatform.V1.GenerateContentResponse

    /// <summary>
    /// LLM response class that provides the first candidate response from the
    /// model if available. Otherwise, returns error code and message.
    /// </summary>
    public class LlmResponse
    {
        /// <summary>
        /// The content of the response.
        /// </summary>
        public Content? Content { get; set; }

        /// <summary>
        /// The grounding metadata of the response.
        /// </summary>
        public GroundingMetadata? GroundingMetadata { get; set; }

        /// <summary>
        /// Indicates whether the text content is part of a unfinished text stream.
        /// Only used for streaming mode and when the content is plain text.
        /// </summary>
        public bool? Partial { get; set; }

        /// <summary>
        /// Indicates whether the response from the model is complete.
        /// Only used for streaming mode.
        /// </summary>
        public bool? TurnComplete { get; set; }

        /// <summary>
        /// Error code if the response is an error. Code varies by model.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Error message if the response is an error.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Flag indicating that LLM was interrupted when generating the content.
        /// Usually it's due to user interruption during a bidi streaming.
        /// </summary>
        public bool? Interrupted { get; set; }

        /// <summary>
        /// The custom metadata of the LlmResponse.
        /// An optional key-value pair to label an LlmResponse.
        /// NOTE: the entire object must be JSON serializable.
        /// </summary>
        public Dictionary<string, object>? CustomMetadata { get; set; }

        /// <summary>
        /// The usage metadata of the LlmResponse.
        /// </summary>
        public GenerateContentResponse.Types.UsageMetadata? UsageMetadata { get; set; }

        /// <summary>
        /// The finish reason of the response.
        /// </summary>
        public Candidate.Types.FinishReason? FinishReason { get; set; }

        // TODO: LiveServerSessionResumptionUpdate and Transcription are not in Vertex AI V1 yet or I need to find them.
        // For now, I will leave them out or mock them if strictly needed by tests.
        // public LiveServerSessionResumptionUpdate? LiveSessionResumptionUpdate { get; set; }
        // public Transcription? InputTranscription { get; set; }
        // public Transcription? OutputTranscription { get; set; }

        public static LlmResponse Create(GenerateContentResponse response)
        {
            var usageMetadata = response.UsageMetadata;

            if (response.Candidates != null && response.Candidates.Count > 0)
            {
                var candidate = response.Candidates[0];
                if (candidate.Content?.Parts != null && candidate.Content.Parts.Count > 0)
                {
                    return new LlmResponse
                    {
                        Content = candidate.Content,
                        GroundingMetadata = candidate.GroundingMetadata,
                        UsageMetadata = usageMetadata,
                        FinishReason = candidate.FinishReason,
                    };
                }

                return new LlmResponse
                {
                    ErrorCode = candidate.FinishReason.ToString(),
                    ErrorMessage = candidate.FinishMessage,
                    UsageMetadata = usageMetadata,
                    FinishReason = candidate.FinishReason,
                };
            }

            if (response.PromptFeedback != null)
            {
                return new LlmResponse
                {
                    ErrorCode = response.PromptFeedback.BlockReason.ToString(),
                    ErrorMessage = response.PromptFeedback.BlockReasonMessage,
                    UsageMetadata = usageMetadata,
                };
            }

            return new LlmResponse
            {
                ErrorCode = "UNKNOWN_ERROR",
                ErrorMessage = "Unknown error.",
                UsageMetadata = usageMetadata,
            };
        }
    }
}
