using System.Reflection.Metadata;
using MEligibilityPlatform.Domain.Entities;
using Parameter = MEligibilityPlatform.Domain.Entities.Parameter;

namespace MEligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Repository interface for managing parameter entities.
    /// Extends the base repository interface with default CRUD operations.
    /// </summary>
    public interface IParameterRepository : IRepository<Parameter>
    {
    }
}
