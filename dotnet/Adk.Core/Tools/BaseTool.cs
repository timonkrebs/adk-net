using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.AIPlatform.V1;
using Adk.Core.Agents;
using Adk.Core.Models;

namespace Adk.Core.Tools
{
    public class RunAsyncToolRequest
    {
        public Dictionary<string, object> Args { get; set; } = new Dictionary<string, object>();
        public ToolContext ToolContext { get; set; }

        public RunAsyncToolRequest(Dictionary<string, object> args, ToolContext toolContext)
        {
            Args = args;
            ToolContext = toolContext;
        }
    }

    public class ToolProcessLlmRequest
    {
        public ToolContext ToolContext { get; set; }
        public LlmRequest LlmRequest { get; set; }

        public ToolProcessLlmRequest(ToolContext toolContext, LlmRequest llmRequest)
        {
            ToolContext = toolContext;
            LlmRequest = llmRequest;
        }
    }

    public class BaseToolParams
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsLongRunning { get; set; }

        public BaseToolParams(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    public abstract class BaseTool
    {
        public string Name { get; }
        public string Description { get; }
        public bool IsLongRunning { get; }

        protected BaseTool(BaseToolParams parameters)
        {
            Name = parameters.Name;
            Description = parameters.Description;
            IsLongRunning = parameters.IsLongRunning;
        }

        public virtual FunctionDeclaration? GetDeclaration()
        {
            return null;
        }

        public abstract Task<object?> RunAsync(RunAsyncToolRequest request);

        public virtual Task ProcessLlmRequest(ToolProcessLlmRequest request)
        {
            var functionDeclaration = GetDeclaration();
            if (functionDeclaration == null)
            {
                return Task.CompletedTask;
            }

            request.LlmRequest.ToolsDict[Name] = this;

            // In .NET Vertex AI, we usually create a Tool object containing FunctionDeclarations
            // and add it to the request.
            // Simplified logic here:
            var tool = FindToolWithFunctionDeclarations(request.LlmRequest);
            if (tool != null)
            {
                tool.FunctionDeclarations.Add(functionDeclaration);
            }
            else
            {
                var newTool = new Tool();
                newTool.FunctionDeclarations.Add(functionDeclaration);
                request.LlmRequest.Tools.Add(newTool);
            }

            return Task.CompletedTask;
        }

        private Tool? FindToolWithFunctionDeclarations(LlmRequest llmRequest)
        {
            foreach (var tool in llmRequest.Tools)
            {
                if (tool.FunctionDeclarations.Count > 0) return tool;
            }
            return null;
        }
    }
}
