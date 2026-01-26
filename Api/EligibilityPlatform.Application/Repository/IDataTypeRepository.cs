using System.ComponentModel.DataAnnotations;
using MEligibilityPlatform.Domain.Entities;

namespace MEligibilityPlatform.Application.Repository
{
    namespace MEligibilityPlatform.Application.Repository
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
