using System.ComponentModel.DataAnnotations;
using EligibilityPlatform.Domain.Entities;

namespace EligibilityPlatform.Application.Repository
{
    namespace EligibilityPlatform.Application.Repository
    {
        /// <summary>
        /// Repository interface for managing data type entities.
        /// Extends the base repository interface with default CRUD operations.
        /// </summary>
        public interface IDataTypeRepository : IRepository<Domain.Entities.DataType>
        {
        }
    }
}
