using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing Log operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="LogService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class LogService(IUnitOfWork uow, IMapper mapper) : ILogService
    {
        /// <summary>
        /// The unit of work instance for database operations.
        /// </summary>
        private readonly IUnitOfWork _uow = uow;

        /// <summary>
        /// The AutoMapper instance for object mapping.
        /// </summary>
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Gets all log records with pagination.
        /// </summary>
        /// <param name="pageIndex">The index of the page to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>A PaginationModel containing the paged log records.</returns>
        public async Task<PaginationModel<LogModel>> GetAll(int pageIndex, int pageSize)
        {
            // Creates a query for log entities with no tracking for read-only operations
            var logsQuery = _uow.LogRepository.Query().AsNoTracking();

            // Projects the query to select specific log model properties
            var projectedQuery = logsQuery.Select(l => new LogModel
            {
                Id = l.Id,
                Message = l.Message,
                Level = l.Level,
                TimeStamp = l.TimeStamp,
                Exception = l.Exception
            });

            // Gets the total count of log records
            var totalCount = await projectedQuery.CountAsync();

            // Retrieves the paged log records ordered by ID descending
            var logs = await projectedQuery
                .OrderByDescending(l => l.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Returns the pagination model with log data and total count
            return new PaginationModel<LogModel>
            {
                IsSuccess = true,
                Data = logs,
                TotalCount = totalCount
            };
        }
    }
}
