using Google.Cloud.AIPlatform.V1;
using System.Threading.Tasks;

namespace Adk.Core.Artifacts
{
    public interface BaseArtifactService
    {
        Task<Part?> LoadArtifact(object paramsObj); // Placeholder params
        Task<int> SaveArtifact(object paramsObj); // Placeholder params
    }
}
