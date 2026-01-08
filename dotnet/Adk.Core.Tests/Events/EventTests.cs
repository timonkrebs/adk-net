using Xunit;
using Adk.Core.Events;
using AdkEvent = Adk.Core.Events.Event;

namespace Adk.Core.Tests.Events
{
    public class EventTests
    {
        [Fact]
        public void CreateEvent_AssignsNewId_WhenIdNotProvided()
        {
            var evt = EventUtils.CreateEvent();
            Assert.NotNull(evt.Id);
            Assert.NotEmpty(evt.Id);
            Assert.Equal(8, evt.Id.Length);
        }

        [Fact]
        public void CreateEvent_UsesProvidedId()
        {
            var evt = EventUtils.CreateEvent(id: "customId");
            Assert.Equal("customId", evt.Id);
        }

        [Fact]
        public void CreateEvent_SetsTimestamp()
        {
            var evt = EventUtils.CreateEvent();
            Assert.True(evt.Timestamp > 0);
        }

        [Fact]
        public void IsFinalResponse_ReturnsTrue_WhenSkipSummarizationIsTrue()
        {
            var actions = new EventActions { SkipSummarization = true };
            var evt = EventUtils.CreateEvent(actions: actions);
            Assert.True(EventUtils.IsFinalResponse(evt));
        }
    }
}
