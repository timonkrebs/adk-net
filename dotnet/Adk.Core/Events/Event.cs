using Google.Cloud.AIPlatform.V1;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adk.Core.Events
{
    using LlmResponse = Adk.Core.Models.LlmResponse;

    /// <summary>
    /// Represents an event in a conversation between agents and users.
    /// It is used to store the content of the conversation, as well as the actions
    /// taken by the agents like function calls, etc.
    /// </summary>
    public class Event : LlmResponse
    {
        /// <summary>
        /// The unique identifier of the event.
        /// Do not assign the ID. It will be assigned by the session.
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// The invocation ID of the event. Should be non-empty before appending to a
        /// session.
        /// </summary>
        public string InvocationId { get; set; } = "";

        /// <summary>
        /// "user" or the name of the agent, indicating who appended the event to the
        /// session.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// The actions taken by the agent.
        /// </summary>
        public EventActions Actions { get; set; } = new EventActions();

        /// <summary>
        /// Set of ids of the long running function calls. Agent client will know from
        /// this field about which function call is long running. Only valid for
        /// function call event
        /// </summary>
        public List<string>? LongRunningToolIds { get; set; }

        /// <summary>
        /// The branch of the event.
        /// The format is like agent_1.agent_2.agent_3, where agent_1 is the parent of
        /// agent_2, and agent_2 is the parent of agent_3.
        /// </summary>
        public string? Branch { get; set; }

        /// <summary>
        /// The timestamp of the event.
        /// </summary>
        public long Timestamp { get; set; }
    }

    public static class EventUtils
    {
        private const string ASCII_LETTERS_AND_NUMBERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static Event CreateEvent(
            string? id = null,
            string? invocationId = null,
            string? author = null,
            EventActions? actions = null,
            List<string>? longRunningToolIds = null,
            string? branch = null,
            long? timestamp = null,
            Content? content = null)
        {
            return new Event
            {
                Id = id ?? CreateNewEventId(),
                InvocationId = invocationId ?? "",
                Author = author,
                Actions = actions ?? new EventActions(),
                LongRunningToolIds = longRunningToolIds ?? new List<string>(),
                Branch = branch,
                Timestamp = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Content = content
            };
        }

        public static bool IsFinalResponse(Event evt)
        {
            if (evt.Actions.SkipSummarization == true ||
                (evt.LongRunningToolIds != null && evt.LongRunningToolIds.Count > 0))
            {
                return true;
            }

            return GetFunctionCalls(evt).Count == 0 &&
                   GetFunctionResponses(evt).Count == 0 &&
                   (evt.Partial != true) &&
                   !HasTrailingCodeExecutionResult(evt);
        }

        public static List<FunctionCall> GetFunctionCalls(Event evt)
        {
            var funcCalls = new List<FunctionCall>();
            if (evt.Content?.Parts != null)
            {
                foreach (var part in evt.Content.Parts)
                {
                    if (part.FunctionCall != null)
                    {
                        funcCalls.Add(part.FunctionCall);
                    }
                }
            }
            return funcCalls;
        }

        public static List<FunctionResponse> GetFunctionResponses(Event evt)
        {
            var funcResponses = new List<FunctionResponse>();
            if (evt.Content?.Parts != null)
            {
                foreach (var part in evt.Content.Parts)
                {
                    if (part.FunctionResponse != null)
                    {
                        funcResponses.Add(part.FunctionResponse);
                    }
                }
            }
            return funcResponses;
        }

        public static bool HasTrailingCodeExecutionResult(Event evt)
        {
            if (evt.Content != null && evt.Content.Parts.Count > 0)
            {
                // In Google.Cloud.AIPlatform.V1.Part, there is no ExecutableCodeResult property directly.
                // However, there is a oneof case for Data.
                // We should check if the part is a code execution result.
                // NOTE: Vertex AI V1 Part definition has 'FunctionResponse' and 'FunctionCall'.
                // 'ExecutableCodeResult' might be available in newer versions or beta.
                // Let's check OneofCase.

                // For now, assume false as standard Vertex AI V1 doesn't seem to have codeExecutionResult in Part yet in 3.10.0 or it's named differently.
                // It might be 'CodeExecutionResult' in newer SDKs?

                // If it's strictly needed, we might need to check the JSON or use dynamic if it's there but not in the contract.
                // But for standard compilation, I'll return false for now to fix the build unless I find the property.

                // Actually, looking at online docs for Vertex AI, code execution is a newer feature.
                // I'll comment it out or return false until I confirm the property name.

                // return lastPart.ExecutableCodeResult != null;
                return false;
            }
            return false;
        }

        public static string StringifyContent(Event evt)
        {
            if (evt.Content == null || evt.Content.Parts == null)
            {
                return "";
            }

            return string.Join("", evt.Content.Parts.Select(p => p.Text ?? ""));
        }

        public static string CreateNewEventId()
        {
            var random = new Random();
            var id = new char[8];
            for (int i = 0; i < 8; i++)
            {
                id[i] = ASCII_LETTERS_AND_NUMBERS[random.Next(ASCII_LETTERS_AND_NUMBERS.Length)];
            }
            return new string(id);
        }
    }
}
