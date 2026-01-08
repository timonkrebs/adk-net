using Google.Cloud.AIPlatform.V1;
using Adk.Core.Events;
using Adk.Core.Sessions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adk.Core.Agents
{
    public class ReadonlyContext
    {
        public InvocationContext InvocationContext { get; }

        public ReadonlyContext(InvocationContext invocationContext)
        {
            InvocationContext = invocationContext;
        }

        public Content? UserContent => InvocationContext.UserContent;
        public string InvocationId => InvocationContext.InvocationId;
        public string AgentName => InvocationContext.Agent.Name;
        public Dictionary<string, object> State => new Dictionary<string, object>(InvocationContext.Session.State); // Should be read-only logically
    }

    public class CallbackContext : ReadonlyContext
    {
        private readonly Dictionary<string, object> _state;
        public EventActions EventActions { get; }

        public CallbackContext(InvocationContext invocationContext, EventActions? eventActions = null)
            : base(invocationContext)
        {
            EventActions = eventActions ?? new EventActions();
            // In C#, there is no direct equivalent of the State helper class in TS unless I create one.
            // For now I'll just use a Dictionary copy + delta logic if needed, but the original code uses a State class wrapper.
            // I'll assume StateDelta handles the updates.
            _state = new Dictionary<string, object>(invocationContext.Session.State);
            // Apply delta if any in eventActions.StateDelta
            foreach (var kvp in EventActions.StateDelta)
            {
                _state[kvp.Key] = kvp.Value;
            }
        }

        public new Dictionary<string, object> State => _state;

        public Task<Part?> LoadArtifact(string filename, int? version = null)
        {
            if (InvocationContext.ArtifactService == null)
            {
                throw new Exception("Artifact service is not initialized.");
            }

            // Need to construct the params object or specific args
            return InvocationContext.ArtifactService.LoadArtifact(new
            {
                AppName = InvocationContext.AppName,
                UserId = InvocationContext.UserId,
                SessionId = InvocationContext.Session.Id,
                Filename = filename,
                Version = version
            });
        }

        public async Task<int> SaveArtifact(string filename, Part artifact)
        {
            if (InvocationContext.ArtifactService == null)
            {
                throw new Exception("Artifact service is not initialized.");
            }

            int version = await InvocationContext.ArtifactService.SaveArtifact(new
            {
                AppName = InvocationContext.AppName,
                UserId = InvocationContext.UserId,
                SessionId = InvocationContext.Session.Id,
                Filename = filename,
                Artifact = artifact
            });

            EventActions.ArtifactDelta[filename] = version;
            return version;
        }
    }
}
