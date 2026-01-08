using Google.Cloud.AIPlatform.V1;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdkEvent = Adk.Core.Events.Event;

namespace Adk.Core.Agents
{
    public delegate Task<Content?> SingleAgentCallback(CallbackContext context);

    public class BaseAgentConfig
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public BaseAgent? ParentAgent { get; set; }
        public List<BaseAgent>? SubAgents { get; set; }
        public List<SingleAgentCallback>? BeforeAgentCallback { get; set; }
        public List<SingleAgentCallback>? AfterAgentCallback { get; set; }

        public BaseAgentConfig(string name)
        {
            Name = name;
        }
    }

    public abstract class BaseAgent
    {
        public string Name { get; }
        public string? Description { get; }
        public BaseAgent RootAgent { get; }

        private BaseAgent? _parentAgent;
        public BaseAgent? ParentAgent
        {
            get => _parentAgent;
            set => _parentAgent = value;
        }

        public List<BaseAgent> SubAgents { get; }
        public List<SingleAgentCallback> BeforeAgentCallback { get; }
        public List<SingleAgentCallback> AfterAgentCallback { get; }

        // OpenTelemetry Tracer
        // In .NET we usually use ActivitySource
        private static readonly System.Diagnostics.ActivitySource ActivitySource = new System.Diagnostics.ActivitySource("gcp.vertex.agent");

        protected BaseAgent(BaseAgentConfig config)
        {
            Name = ValidateAgentName(config.Name);
            Description = config.Description;
            _parentAgent = config.ParentAgent;
            SubAgents = config.SubAgents ?? new List<BaseAgent>();
            RootAgent = GetRootAgent(this);
            BeforeAgentCallback = config.BeforeAgentCallback ?? new List<SingleAgentCallback>();
            AfterAgentCallback = config.AfterAgentCallback ?? new List<SingleAgentCallback>();

            SetParentAgentForSubAgents();
        }

        public async IAsyncEnumerable<AdkEvent> RunAsync(InvocationContext parentContext)
        {
            using var activity = ActivitySource.StartActivity($"agent_run [{Name}]");

            var context = CreateInvocationContext(parentContext);

            var beforeAgentCallbackEvent = await HandleBeforeAgentCallback(context);
            if (beforeAgentCallbackEvent != null)
            {
                yield return beforeAgentCallbackEvent;
            }

            if (context.EndInvocation)
            {
                yield break;
            }

            await foreach (var evt in RunAsyncImpl(context))
            {
                yield return evt;
            }

            if (context.EndInvocation)
            {
                yield break;
            }

            var afterAgentCallbackEvent = await HandleAfterAgentCallback(context);
            if (afterAgentCallbackEvent != null)
            {
                yield return afterAgentCallbackEvent;
            }
        }

        // RunLive implementation omitted for now as per TODO in TS code

        protected abstract IAsyncEnumerable<AdkEvent> RunAsyncImpl(InvocationContext context);
        // protected abstract IAsyncEnumerable<AdkEvent> RunLiveImpl(InvocationContext context);

        public BaseAgent? FindAgent(string name)
        {
            if (Name == name)
            {
                return this;
            }
            return FindSubAgent(name);
        }

        public BaseAgent? FindSubAgent(string name)
        {
            foreach (var subAgent in SubAgents)
            {
                var result = subAgent.FindAgent(name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        protected virtual InvocationContext CreateInvocationContext(InvocationContext parentContext)
        {
            return new InvocationContext(
                this,
                parentContext.InvocationId,
                parentContext.Session,
                parentContext.PluginManager,
                parentContext.ArtifactService,
                parentContext.SessionService,
                parentContext.MemoryService,
                parentContext.CredentialService,
                parentContext.Branch,
                parentContext.UserContent,
                parentContext.EndInvocation,
                parentContext.TranscriptionCache,
                parentContext.RunConfig,
                parentContext.LiveRequestQueue,
                parentContext.ActiveStreamingTools
            );
        }

        protected async Task<AdkEvent?> HandleBeforeAgentCallback(InvocationContext invocationContext)
        {
            if (BeforeAgentCallback.Count == 0)
            {
                return null;
            }

            var callbackContext = new CallbackContext(invocationContext);
            foreach (var callback in BeforeAgentCallback)
            {
                var content = await callback(callbackContext);

                if (content != null)
                {
                    invocationContext.EndInvocation = true;

                    return Adk.Core.Events.EventUtils.CreateEvent(
                        invocationId: invocationContext.InvocationId,
                        author: Name,
                        branch: invocationContext.Branch,
                        content: content,
                        actions: callbackContext.EventActions
                    );
                }
            }

            if (callbackContext.EventActions.StateDelta.Count > 0)
            {
                return Adk.Core.Events.EventUtils.CreateEvent(
                    invocationId: invocationContext.InvocationId,
                    author: Name,
                    branch: invocationContext.Branch,
                    actions: callbackContext.EventActions
                );
            }

            return null;
        }

        protected async Task<AdkEvent?> HandleAfterAgentCallback(InvocationContext invocationContext)
        {
            if (AfterAgentCallback.Count == 0)
            {
                return null;
            }

            var callbackContext = new CallbackContext(invocationContext);
            foreach (var callback in AfterAgentCallback)
            {
                var content = await callback(callbackContext);

                if (content != null)
                {
                    return Adk.Core.Events.EventUtils.CreateEvent(
                        invocationId: invocationContext.InvocationId,
                        author: Name,
                        branch: invocationContext.Branch,
                        content: content,
                        actions: callbackContext.EventActions
                    );
                }
            }

             if (callbackContext.EventActions.StateDelta.Count > 0)
            {
                return Adk.Core.Events.EventUtils.CreateEvent(
                    invocationId: invocationContext.InvocationId,
                    author: Name,
                    branch: invocationContext.Branch,
                    actions: callbackContext.EventActions
                );
            }

            return null;
        }

        private void SetParentAgentForSubAgents()
        {
            foreach (var subAgent in SubAgents)
            {
                if (subAgent.ParentAgent != null)
                {
                    throw new Exception($"Agent \"{subAgent.Name}\" already has a parent agent, current parent: \"{subAgent.ParentAgent.Name}\", trying to add: \"{Name}\"");
                }
                subAgent.ParentAgent = this;
            }
        }

        private static string ValidateAgentName(string name)
        {
            if (!IsIdentifier(name))
            {
                throw new Exception($"Found invalid agent name: \"{name}\". Agent name must be a valid identifier.");
            }

            if (name == "user")
            {
                throw new Exception("Agent name cannot be 'user'. 'user' is reserved for end-user's input.");
            }

            return name;
        }

        private static bool IsIdentifier(string str)
        {
            // C# regex for identifiers is slightly different but this matches the intent
            return Regex.IsMatch(str, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }

        private static BaseAgent GetRootAgent(BaseAgent rootAgent)
        {
            while (rootAgent.ParentAgent != null)
            {
                rootAgent = rootAgent.ParentAgent;
            }
            return rootAgent;
        }
    }
}
