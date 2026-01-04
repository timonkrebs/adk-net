using Google.Cloud.AIPlatform.V1;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adk.Core.Models
{
    // Need Blob type from GenAI or placeholder.
    // Vertex AI uses raw bytes or specific types.
    // I'll use a placeholder for now.
    public class Blob { }

    public interface BaseLlmConnection
    {
        Task SendHistory(List<Content> history);
        Task SendContent(Content content);
        Task SendRealtime(Blob blob);
        IAsyncEnumerable<LlmResponse> Receive();
        Task Close();
    }
}
