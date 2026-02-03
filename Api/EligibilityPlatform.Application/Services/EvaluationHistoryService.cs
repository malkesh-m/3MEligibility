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
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service for managing evaluation history records.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EvaluationHistoryService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class EvaluationHistoryService(IUnitOfWork uow, IMapper mapper) : IEvaluationHistoryService
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
        /// Adds a new evaluation history record for the specified entity.
        /// </summary>
        /// <param name="model">The evaluation history model to add.</param>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(EvaluationHistoryModel model, int tenantId)
        {
            /// <summary>
            /// Maps the evaluation history model to an entity.
            /// </summary>
            var entity = _mapper.Map<EvaluationHistory>(model);

            /// <summary>
            /// Sets the evaluation timestamp to the current date and time.
            /// </summary>
            entity.EvaluationTimeStamp = DateTime.Now;

            /// <summary>
            /// Sets the entity ID for the evaluation history record.
            /// </summary>
            entity.TenantId = tenantId;

            /// <summary>
            /// Adds the entity to the repository.
            /// </summary>
            _uow.EvaluationHistoryRepository.Add(entity);

            /// <summary>
            /// Saves changes to the database.
            /// </summary>
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes an evaluation history record by its ID and entity ID.
        /// </summary>
        /// <param name="id">The evaluation history ID.</param>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int id, int tenantId)
        {
            /// <summary>
            /// Retrieves the evaluation history entity by ID and entity ID.
            /// </summary>
            var entity = await _uow.EvaluationHistoryRepository.Query()
                .FirstOrDefaultAsync(x => x.EvaluationHistoryId == id && x.TenantId == tenantId);

            /// <summary>
            /// If the entity exists, removes it from the repository.
            /// </summary>
            if (entity != null)
            {
                _uow.EvaluationHistoryRepository.Remove(entity);

                /// <summary>
                /// Saves changes to the database.
                /// </summary>
                await _uow.CompleteAsync();
            }
        }

        /// <summary>
        /// Gets all evaluation history records for a given entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of evaluation history models.</returns>
        public async Task<List<EvaluationHistoryModel>> GetAll(int tenantId)
        {
            /// <summary>
            /// Retrieves all evaluation history entities for the specified entity ID.
            /// </summary>
            var entities = await _uow.EvaluationHistoryRepository.GetAllByTenantId(tenantId)
                .ToListAsync();

            /// <summary>
            /// Maps the entities to evaluation history models.
            /// </summary>
            return _mapper.Map<List<EvaluationHistoryModel>>(entities);
        }

        /// <summary>
        /// Gets an evaluation history record by its ID and entity ID.
        /// </summary>
        /// <param name="id">The evaluation history ID.</param>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>The evaluation history model.</returns>
        public async Task<EvaluationHistoryModel> GetById(int id, int tenantId)
        {
            /// <summary>
            /// Retrieves the evaluation history entity by ID and entity ID.
            /// </summary>
            var entity = await _uow.EvaluationHistoryRepository.Query()
                .FirstOrDefaultAsync(x => x.EvaluationHistoryId == id && x.TenantId == tenantId);

            /// <summary>
            /// Maps the entity to an evaluation history model.
            /// </summary>
            return _mapper.Map<EvaluationHistoryModel>(entity);
        }

        /// <summary>
        /// Updates an existing evaluation history record.
        /// </summary>
        /// <param name="model">The evaluation history model with updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(EvaluationHistoryModel model)
        {
            /// <summary>
            /// Retrieves the existing evaluation history entity by ID.
            /// </summary>
            var entity = _uow.EvaluationHistoryRepository.GetById(model.EvaluationHistoryId);

            /// <summary>
            /// If the entity exists, updates it with the new data.
            /// </summary>
            if (entity != null)
            {
                /// <summary>
                /// Maps the updated model data to the existing entity.
                /// </summary>
                _mapper.Map(model, entity);

                /// <summary>
                /// Updates the entity in the repository.
                /// </summary>
                _uow.EvaluationHistoryRepository.Update(entity);

                /// <summary>
                /// Saves changes to the database.
                /// </summary>
                await _uow.CompleteAsync();
            }
        }
    }
}
