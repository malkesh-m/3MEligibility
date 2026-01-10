using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for API execution operations.
    /// Provides methods for retrieving REST API details.
    /// </summary>
    public interface IApiExcuteService
    {
        /// <summary>
        /// Retrieves REST API details for a specific node.
        /// </summary>
        /// <param name="nodeId">The ID of the node to retrieve API details for.</param>
        /// <returns>A list of <see cref="ApiParameterModel"/> objects containing the API details.</returns>
        Task<List<ApiParameterModel>> GetRestApiDetailsAsync(int nodeId);
    }
}
