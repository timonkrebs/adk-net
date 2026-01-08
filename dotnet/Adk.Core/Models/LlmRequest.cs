using Google.Cloud.AIPlatform.V1;
using Adk.Core.Agents;
using System.Collections.Generic;

namespace Adk.Core.Models
{
    // Need to find LiveConnectConfig and SchemaUnion, or equivalents.
    // LiveConnectConfig seems to be specific to real-time API.
    // SchemaUnion is likely for JSON Schema response.
    // In Vertex AI V1, `GenerationConfig` has `ResponseSchema`.

    /// <summary>
    /// LLM request class that allows passing in tools, output schema and system
    /// instructions to the model.
    /// </summary>
    public class LlmRequest
    {
        /// <summary>
        /// The model name.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// The contents to send to the model.
        /// </summary>
        public List<Content> Contents { get; set; } = new List<Content>();

        /// <summary>
        /// Additional config for the generate content request.
        /// Tools in generateContentConfig should not be set directly; use appendTools.
        /// </summary>
        public GenerationConfig? Config { get; set; } // Renamed to Config to match TS type name 'config', but type is GenerationConfig usually? TS says GenerateContentConfig.
        // Actually, TS GenerateContentConfig likely includes tools, systemInstruction etc.
        // In Vertex AI V1, we pass tools and systemInstruction separately to GenerateContent, or part of the request.
        // But here it seems to try to bundle them.

        // Use a custom class or Dictionary if strict mapping is hard, but let's try to stick to Vertex AI concepts if possible.
        // However, `GenerateContentConfig` in JS SDK wraps everything.
        // I will define a wrapper or use the request object builder pattern.
        // For now, I'll use a placeholder or dynamic config to allow flexibility.

        public object? LiveConnectConfig { get; set; }

        /// <summary>
        /// The tools dictionary. Excluded from JSON serialization.
        /// </summary>
        public Dictionary<string, Adk.Core.Tools.BaseTool> ToolsDict { get; set; } = new Dictionary<string, Adk.Core.Tools.BaseTool>();

        // We might need to store SystemInstruction separately if not in Config
        public List<Part> SystemInstructions { get; set; } = new List<Part>();
        public List<Tool> Tools { get; set; } = new List<Tool>();
    }

    public static class LlmRequestUtils
    {
        public static void AppendInstructions(LlmRequest llmRequest, IEnumerable<string> instructions)
        {
            foreach (var instruction in instructions)
            {
                 llmRequest.SystemInstructions.Add(new Part { Text = instruction });
            }
        }

        // TODO: Implement AppendTools and SetOutputSchema
    }
}
