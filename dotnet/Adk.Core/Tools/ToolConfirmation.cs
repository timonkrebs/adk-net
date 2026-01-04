using Google.Cloud.AIPlatform.V1;
using System.Text.Json.Serialization;

namespace Adk.Core.Tools
{
    /// <summary>
    /// Represents a tool confirmation configuration.
    /// Experimental, subject to change.
    /// </summary>
    public class ToolConfirmation
    {
        /// <summary>
        /// The hint text for why the input is needed.
        /// </summary>
        [JsonPropertyName("hint")]
        public string Hint { get; set; }

        /// <summary>
        /// Whether the tool execution is confirmed.
        /// </summary>
        [JsonPropertyName("confirmed")]
        public bool Confirmed { get; set; }

        /// <summary>
        /// The custom data payload needed from the user to continue the flow.
        /// It should be JSON serializable.
        /// </summary>
        [JsonPropertyName("payload")]
        public object? Payload { get; set; }

        public ToolConfirmation(bool confirmed, string hint = "", object? payload = null)
        {
            Hint = hint;
            Confirmed = confirmed;
            Payload = payload;
        }
    }
}
