using Google.Cloud.AIPlatform.V1;
using System;

namespace Adk.Core.Agents
{
    // These types need to be found in Google.Cloud.AIPlatform.V1 or mocked/created if they are from the JS SDK.
    // AudioTranscriptionConfig, ProactivityConfig, RealtimeInputConfig, SpeechConfig are likely JS SDK or specific newer Vertex AI types.
    // I'll create placeholders or use 'object' for now if I can't find them, but check first.

    // Google.Cloud.AIPlatform.V1 has Modality.

    public enum StreamingMode
    {
        None,
        Sse,
        Bidi
    }

    /// <summary>
    /// Configs for runtime behavior of agents.
    /// </summary>
    public class RunConfig
    {
        // Placeholders for complex types not immediately obvious in Vertex AI V1 .NET SDK
        public object? SpeechConfig { get; set; }

        // Modality is in Google.Cloud.AIPlatform.V1.Modality if using new GenAI specific types, but standard V1 uses GenerationConfig which has different params.
        // Assuming we are just passing strings or enums for now, I'll change to string to fix build or remove generic Modality if not found.
        // Actually, looking at docs, Modality might be in a different namespace or not exposed directly as enum in V1.
        // I will use `object` or `string` for now as placeholder.
        public List<string>? ResponseModalities { get; set; }

        public bool SaveInputBlobsAsArtifacts { get; set; }

        public bool SupportCfc { get; set; }

        public StreamingMode StreamingMode { get; set; }

        public object? OutputAudioTranscription { get; set; }

        public object? InputAudioTranscription { get; set; }

        public bool EnableAffectiveDialog { get; set; }

        public object? Proactivity { get; set; }

        public object? RealtimeInputConfig { get; set; }

        public int MaxLlmCalls { get; set; }

        public static RunConfig Create(
            bool saveInputBlobsAsArtifacts = false,
            bool supportCfc = false,
            bool enableAffectiveDialog = false,
            StreamingMode streamingMode = StreamingMode.None,
            int maxLlmCalls = 500)
        {
            return new RunConfig
            {
                SaveInputBlobsAsArtifacts = saveInputBlobsAsArtifacts,
                SupportCfc = supportCfc,
                EnableAffectiveDialog = enableAffectiveDialog,
                StreamingMode = streamingMode,
                MaxLlmCalls = ValidateMaxLlmCalls(maxLlmCalls)
            };
        }

        private static int ValidateMaxLlmCalls(int value)
        {
            if (value <= 0)
            {
                // In C#, standard Console.WriteLine or a proper logger would be used.
                Console.WriteLine("Warning: maxLlmCalls is less than or equal to 0. This will result in no enforcement on total number of llm calls.");
            }
            return value;
        }
    }
}
