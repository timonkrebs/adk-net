using System.Collections.Generic;
using System.Threading.Tasks;
using Adk.Core.Agents;
using Adk.Core.Events;

namespace Adk.Core.Tools
{
    // Need dummy classes for AuthConfig, AuthCredential, AuthHandler, SearchMemoryResponse
    // to match TS code structure.
    public class AuthConfig { }
    public class AuthCredential { }
    public class SearchMemoryResponse { }

    public class ToolContext : CallbackContext
    {
        public string? FunctionCallId { get; }
        public ToolConfirmation? ToolConfirmation { get; set; }

        public ToolContext(
            InvocationContext invocationContext,
            EventActions? eventActions = null,
            string? functionCallId = null,
            ToolConfirmation? toolConfirmation = null)
            : base(invocationContext, eventActions)
        {
            FunctionCallId = functionCallId;
            ToolConfirmation = toolConfirmation;
        }

        public EventActions Actions => EventActions;

        public void RequestCredential(AuthConfig authConfig)
        {
            if (FunctionCallId == null)
            {
                throw new System.Exception("functionCallId is not set.");
            }

            // Mock AuthHandler
            EventActions.RequestedAuthConfigs[FunctionCallId] = new object(); // Placeholder
        }

        public AuthCredential? GetAuthResponse(AuthConfig authConfig)
        {
            // Mock AuthHandler
            return null;
        }

        public Task<List<string>> ListArtifacts()
        {
            if (InvocationContext.ArtifactService == null)
            {
                throw new System.Exception("Artifact service is not initialized.");
            }

            // Placeholder call
            // return InvocationContext.ArtifactService.ListArtifactKeys(...)
            return Task.FromResult(new List<string>());
        }

        public Task<SearchMemoryResponse> SearchMemory(string query)
        {
             if (InvocationContext.MemoryService == null)
            {
                throw new System.Exception("Memory service is not initialized.");
            }
             return Task.FromResult(new SearchMemoryResponse());
        }

        public void RequestConfirmation(string hint = "", object? payload = null)
        {
            if (FunctionCallId == null)
            {
                throw new System.Exception("functionCallId is not set.");
            }

            EventActions.RequestedToolConfirmations[FunctionCallId] = new ToolConfirmation(false, hint, payload);
        }
    }
}
