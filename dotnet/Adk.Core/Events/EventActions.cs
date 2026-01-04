using System.Collections.Generic;
using Adk.Core.Tools;
using System.Linq;

namespace Adk.Core.Events
{
    // TODO: Replace 'object' with a proper AuthConfig.
    using AuthConfig = System.Object;

    /// <summary>
    /// Represents the actions attached to an event.
    /// </summary>
    public class EventActions
    {
        /// <summary>
        /// If true, it won't call model to summarize function response.
        /// Only used for function_response event.
        /// </summary>
        public bool? SkipSummarization { get; set; }

        /// <summary>
        /// Indicates that the event is updating the state with the given delta.
        /// </summary>
        public Dictionary<string, object> StateDelta { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Indicates that the event is updating an artifact. key is the filename,
        /// value is the version.
        /// </summary>
        public Dictionary<string, int> ArtifactDelta { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// If set, the event transfers to the specified agent.
        /// </summary>
        public string? TransferToAgent { get; set; }

        /// <summary>
        /// The agent is escalating to a higher level agent.
        /// </summary>
        public bool? Escalate { get; set; }

        /// <summary>
        /// Authentication configurations requested by tool responses.
        /// </summary>
        public Dictionary<string, AuthConfig> RequestedAuthConfigs { get; set; } = new Dictionary<string, AuthConfig>();

        /// <summary>
        /// A dict of tool confirmation requested by this event, keyed by the function
        /// call id.
        /// </summary>
        public Dictionary<string, ToolConfirmation> RequestedToolConfirmations { get; set; } = new Dictionary<string, ToolConfirmation>();

        /// <summary>
        /// Merges a list of EventActions objects into a single EventActions object.
        /// </summary>
        public static EventActions Merge(IEnumerable<EventActions?> sources, EventActions? target = null)
        {
            var result = new EventActions();

            if (target != null)
            {
                MergeActions(result, target);
            }

            foreach (var source in sources)
            {
                if (source == null) continue;
                MergeActions(result, source);
            }

            return result;
        }

        private static void MergeActions(EventActions result, EventActions source)
        {
            foreach (var kvp in source.StateDelta)
            {
                result.StateDelta[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in source.ArtifactDelta)
            {
                result.ArtifactDelta[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in source.RequestedAuthConfigs)
            {
                result.RequestedAuthConfigs[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in source.RequestedToolConfirmations)
            {
                result.RequestedToolConfirmations[kvp.Key] = kvp.Value;
            }

            if (source.SkipSummarization.HasValue)
            {
                result.SkipSummarization = source.SkipSummarization;
            }
            if (source.TransferToAgent != null)
            {
                result.TransferToAgent = source.TransferToAgent;
            }
            if (source.Escalate.HasValue)
            {
                result.Escalate = source.Escalate;
            }
        }
    }
}
