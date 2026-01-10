using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services.Inteface
{
    /// <summary>
    /// Service interface for log management operations.
    /// Provides methods for retrieving log records with pagination support.
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Retrieves log records with pagination support.
        /// </summary>
        /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
        /// <param name="pageSize">The number of records to include per page.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="PaginationModel{LogModel}"/> with the paginated log records.</returns>
        Task<PaginationModel<LogModel>> GetAll(int pageIndex, int pageSize);
    }
}
