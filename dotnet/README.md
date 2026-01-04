# ADK for .NET (C#)

This is the .NET implementation of the Agent Development Kit (ADK).

## Project Structure

- `Adk.Core`: The core library containing the agent framework, models, and tools.
- `Adk.Core.Tests`: Unit tests for the core library.

## Prerequisites

- .NET SDK 8.0 or later.
- Google Cloud Project with Vertex AI enabled (for using `Gemini` models).

## building

To build the solution:

```bash
dotnet build
```

## Testing

To run the tests:

```bash
dotnet test
```

## Usage Example

Here is a simple example of how to define an agent and run it.

```csharp
using Adk.Core.Agents;
using Adk.Core.Events;
using Adk.Core.Models;
using Google.Cloud.AIPlatform.V1;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// 1. Define your agent by extending BaseAgent
public class MyAgent : BaseAgent
{
    public MyAgent(BaseAgentConfig config) : base(config)
    {
    }

    protected override async IAsyncEnumerable<Event> RunAsyncImpl(InvocationContext context)
    {
        // Simple echo logic for demonstration
        var userText = context.UserContent?.Parts[0]?.Text ?? "No input";

        yield return EventUtils.CreateEvent(
            invocationId: context.InvocationId,
            author: Name,
            content: new Content
            {
                Role = "model",
                Parts = { new Part { Text = $"Echo: {userText}" } }
            }
        );

        await Task.CompletedTask;
    }
}

// 2. Run the agent
public class Program
{
    public static async Task Main(string[] args)
    {
        // Configure
        var config = new BaseAgentConfig("echo_agent")
        {
            Description = "A simple echo agent"
        };
        var agent = new MyAgent(config);

        // Create invocation context (usually done by the runner)
        var session = new Adk.Core.Sessions.Session("session-1", "test-app");
        var pluginManager = new Adk.Core.Plugins.PluginManager();

        var invocationContext = new InvocationContext(
            agent,
            "invocation-1",
            session,
            pluginManager,
            userContent: new Content { Parts = { new Part { Text = "Hello ADK!" } } }
        );

        // Run
        await foreach (var evt in agent.RunAsync(invocationContext))
        {
            Console.WriteLine($"Agent says: {evt.Content?.Parts[0].Text}");
        }
    }
}
```
