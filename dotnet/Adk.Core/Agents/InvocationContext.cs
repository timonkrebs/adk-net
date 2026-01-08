using Google.Cloud.AIPlatform.V1;
using Adk.Core.Artifacts;
using Adk.Core.Sessions;
using Adk.Core.Memory;
using Adk.Core.Auth;
using Adk.Core.Plugins;
using System;
using System.Collections.Generic;

namespace Adk.Core.Agents
{
    public class InvocationContext
    {
        public BaseArtifactService? ArtifactService { get; }
        public BaseSessionService? SessionService { get; }
        public BaseMemoryService? MemoryService { get; }
        public BaseCredentialService? CredentialService { get; }
        public string InvocationId { get; }
        public string? Branch { get; }
        public BaseAgent Agent { get; set; }
        public Content? UserContent { get; }
        public Session Session { get; }
        public bool EndInvocation { get; set; }
        public List<TranscriptionEntry>? TranscriptionCache { get; }
        public RunConfig? RunConfig { get; }
        public LiveRequestQueue? LiveRequestQueue { get; }
        public Dictionary<string, ActiveStreamingTool>? ActiveStreamingTools { get; }
        public PluginManager PluginManager { get; }

        private readonly InvocationCostManager _invocationCostManager = new InvocationCostManager();

        public InvocationContext(
            BaseAgent agent,
            string invocationId,
            Session session,
            PluginManager pluginManager,
            BaseArtifactService? artifactService = null,
            BaseSessionService? sessionService = null,
            BaseMemoryService? memoryService = null,
            BaseCredentialService? credentialService = null,
            string? branch = null,
            Content? userContent = null,
            bool endInvocation = false,
            List<TranscriptionEntry>? transcriptionCache = null,
            RunConfig? runConfig = null,
            LiveRequestQueue? liveRequestQueue = null,
            Dictionary<string, ActiveStreamingTool>? activeStreamingTools = null)
        {
            Agent = agent;
            InvocationId = invocationId;
            Session = session;
            PluginManager = pluginManager;
            ArtifactService = artifactService;
            SessionService = sessionService;
            MemoryService = memoryService;
            CredentialService = credentialService;
            Branch = branch;
            UserContent = userContent;
            EndInvocation = endInvocation;
            TranscriptionCache = transcriptionCache;
            RunConfig = runConfig;
            LiveRequestQueue = liveRequestQueue;
            ActiveStreamingTools = activeStreamingTools;
        }

        public string AppName => Session.AppName;
        public string UserId => Session.UserId;

        public void IncrementLlmCallCount()
        {
            _invocationCostManager.IncrementAndEnforceLlmCallsLimit(RunConfig);
        }
    }

    internal class InvocationCostManager
    {
        private int _numberOfLlmCalls = 0;

        public void IncrementAndEnforceLlmCallsLimit(RunConfig? runConfig)
        {
            _numberOfLlmCalls++;

            if (runConfig != null && runConfig.MaxLlmCalls > 0 &&
                _numberOfLlmCalls > runConfig.MaxLlmCalls)
            {
                throw new Exception($"Max number of llm calls limit of {runConfig.MaxLlmCalls} exceeded");
            }
        }
    }

    public static class InvocationContextUtils
    {
        public static string NewInvocationContextId()
        {
            return $"e-{Guid.NewGuid()}";
        }
    }
}
