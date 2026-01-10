using System.Reflection.Metadata;
using EligibilityPlatform.Domain.Entities;
using Parameter = EligibilityPlatform.Domain.Entities.Parameter;

namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing parameter entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IParameterRepository : IRepository<Parameter>
    {
    }
}
