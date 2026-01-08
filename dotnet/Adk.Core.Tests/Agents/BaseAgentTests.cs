using System.Collections.Generic;
using System.Threading.Tasks;
using Adk.Core.Agents;
using Adk.Core.Events;
using AdkEvent = Adk.Core.Events.Event;

namespace Adk.Core.Tests.Agents
{
    public class TestAgent : BaseAgent
    {
        public TestAgent(BaseAgentConfig config) : base(config)
        {
        }

        protected override async IAsyncEnumerable<AdkEvent> RunAsyncImpl(InvocationContext context)
        {
            yield return EventUtils.CreateEvent(
                invocationId: context.InvocationId,
                author: Name,
                content: new Google.Cloud.AIPlatform.V1.Content
                {
                    Parts = { new Google.Cloud.AIPlatform.V1.Part { Text = "Hello from TestAgent" } }
                }
            );
            await Task.CompletedTask;
        }
    }

    public class BaseAgentTests
    {
        [Fact]
        public void Constructor_ValidatesName()
        {
            var config = new BaseAgentConfig("valid_name");
            var agent = new TestAgent(config);
            Assert.Equal("valid_name", agent.Name);
        }

        [Fact]
        public void Constructor_ThrowsOnInvalidName()
        {
            var config = new BaseAgentConfig("invalid name");
            Assert.Throws<System.Exception>(() => new TestAgent(config));
        }

        [Fact]
        public void Constructor_ThrowsOnReservedNameUser()
        {
            var config = new BaseAgentConfig("user");
            Assert.Throws<System.Exception>(() => new TestAgent(config));
        }
    }
}
