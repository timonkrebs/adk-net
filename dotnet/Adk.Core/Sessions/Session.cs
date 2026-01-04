using System;
using System.Collections.Generic;
using Adk.Core.Events;

namespace Adk.Core.Sessions
{
    /// <summary>
    /// Represents a session in a conversation between agents and users.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// The unique identifier of the session.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the app.
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// The id of the user.
        /// </summary>
        public string UserId { get; set; } = "";

        /// <summary>
        /// The state of the session.
        /// </summary>
        public Dictionary<string, object> State { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The events of the session, e.g. user input, model response, function
        /// call/response, etc.
        /// </summary>
        public List<Event> Events { get; set; } = new List<Event>();

        /// <summary>
        /// The last update time of the session.
        /// </summary>
        public long LastUpdateTime { get; set; }

        public Session(string id, string appName)
        {
            Id = id;
            AppName = appName;
        }

        public static Session Create(string id, string appName, string? userId = null, Dictionary<string, object>? state = null, List<Event>? events = null, long? lastUpdateTime = null)
        {
            return new Session(id, appName)
            {
                UserId = userId ?? "",
                State = state ?? new Dictionary<string, object>(),
                Events = events ?? new List<Event>(),
                LastUpdateTime = lastUpdateTime ?? 0
            };
        }
    }
}
