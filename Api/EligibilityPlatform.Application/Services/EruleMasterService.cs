using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides services for managing eligibility rules.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EruleMasterService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class EruleMasterService(IUnitOfWork uow, IMapper mapper) : IEruleMasterService
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
        /// Adds a new EruleMaster record for the specified entity.
        /// </summary>
        /// <param name="model">The EruleMaster create/update model.</param>
        /// <param name="TenantId">The entity ID.</param>
        public async Task Add(EruleMasterCreateUpodateModel model, int TenantId)
        {
            var existingRule = _uow.EruleMasterRepository.Query().Any(r => r.EruleName == model.EruleName && r.TenantId == model.TenantId);
            if (existingRule)
            {
                throw new Exception("Rule with this name already exists");
            }
            // Maps the input model to EruleMaster entity
            var entity = _mapper.Map<EruleMaster>(model);
            // Sets the rule description from the model
            entity.EruleDesc = model.Description;
            // Sets the rule name from the model
            entity.EruleName = model.EruleName ?? "";
            // Sets the creation timestamp to current time
            entity.CreatedByDateTime = DateTime.Now;
            // Sets the entity ID for the rule
            entity.TenantId = TenantId;
            // Sets the created by user from the model
            entity.CreatedBy = model.CreatedBy;
            // Sets the updated by user from the model
            entity.UpdatedBy = model.UpdatedBy;
            // Sets the active status with default value if not provided
            entity.IsActive = model.IsActive ?? false;
            // Adds the entity to the repository
            _uow.EruleMasterRepository.Add(entity);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Edits an existing EruleMaster record for the specified entity.
        /// </summary>
        /// <param name="model">The EruleMaster create/update model.</param>
        /// <param name="TenantId">The entity ID.</param>
        public async Task Edit(EruleMasterCreateUpodateModel model, int TenantId)
        {
            var existingRule = _uow.EruleMasterRepository.Query().Any(r => r.EruleName == model.EruleName && r.TenantId == TenantId && model.EruleId != r.Id);
            if (existingRule)
            {
                throw new Exception("Rule with this name already exists");
            }
            // Retrieves the existing entity by ID and entity ID
            var entity = _uow.EruleMasterRepository.Query()
                .FirstOrDefault(p => p.Id == model.EruleId && p.TenantId == TenantId) ?? throw new Exception("Original rule not found.");

            // Maps the model properties to the existing entity
            _mapper.Map(model, entity);
            // Sets the update timestamp to current time
            entity.UpdatedByDateTime = DateTime.Now;
            // Sets the entity ID for the rule
            entity.TenantId = TenantId;
            // Sets the updated by user from the model
            entity.UpdatedBy = model.UpdatedBy;
            // Sets the rule description from the model
            entity.EruleDesc = model.Description;
            // Sets the rule name from the model
            entity.EruleName = model.EruleName ?? "";
            // Sets the active status from the model
            entity.IsActive = model.IsActive ?? false;

            // Updates the entity in the repository
            _uow.EruleMasterRepository.Update(entity);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all EruleMaster records for a given entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of EruleMasterListModel.</returns>
        public async Task<List<EruleMasterListModel>> GetAll(int tenantId)
        {
            // Retrieves all entities for the specified entity ID
            var entities = await _uow.EruleMasterRepository.Query().Where(p => p.TenantId == tenantId).ToListAsync();

            // Maps the entities to list model and returns
            return _mapper.Map<List<EruleMasterListModel>>(entities);
        }

        /// <summary>
        /// Gets an EruleMaster record by its ID and entity ID.
        /// </summary>
        /// <param name="id">The EruleMaster ID.</param>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>The EruleMasterModel.</returns>
        public async Task<EruleMasterModel> GetById(int id, int tenantId)
        {
            // Retrieves the entity by ID and entity ID
            var entity = await _uow.EruleMasterRepository
                                   .Query()
                                   .Where(p => p.Id == id && p.TenantId == tenantId).FirstAsync();
            // Maps the entity to model and returns
            return _mapper.Map<EruleMasterModel>(entity);
        }
        public async Task<string> Delete(int id)
        {
            var entity = await _uow.EruleMasterRepository.Query()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity == null)
                return "Erule not found."; // return string if no data

            _uow.EruleMasterRepository.Remove(entity);
            await _uow.CompleteAsync();

            return GlobalcConstants.Deleted; // return success message
        }
        public async Task<string> RemoveMultiple(int tenantId, List<int> ids)
        {
            // Initializes the result message
            var resultMessage = "";

            // Retrieves all eCards for the entity
            var eCards = _uow.EcardRepository.Query().Where(f => f.TenantId == tenantId);

            // Initializes collections for tracking deleted and not deleted rules
            var notDeletedRules = new HashSet<string>();  // Use HashSet to avoid duplicates
            var deletedRules = new List<int>();

            try
            {
                // Processes each rule ID in the list
                foreach (var id in ids)
                {
                    // Retrieves the rule by ID and entity ID
                    var item = _uow.EruleMasterRepository.Query().First(f => f.Id == id && f.TenantId == tenantId);

                    // Checks if the rule is being used in any eCard
                    var isInUse = eCards.Any(card => card.Expression.Contains(id.ToString()));

                    if (isInUse)
                    {
                        if (item != null)
                        {
                            // Adds to not deleted rules collection
                            //notDeletedRules.Add(item.EruleName);
                        }
                    }
                    else
                    {
                        if (item != null)
                        {
                            // Removes the rule from repository
                            _uow.EruleMasterRepository.Remove(item);
                            // Adds to deleted rules collection
                            deletedRules.Add(id);
                        }
                    }
                }

                // Commits the changes to the database
                await _uow.CompleteAsync();

                // Constructs message for not deleted rules
                if (notDeletedRules.Count != 0)
                {
                    var notDeletedMessage = $"The following rules could not be deleted because they are being used in one or more ECards: {string.Join(", ", notDeletedRules)}.";
                    resultMessage += notDeletedMessage;
                }

                // Constructs message for deleted rules
                if (deletedRules.Count != 0)
                {
                    var deletedMessage = $"{deletedRules.Count} " + " Rules " + GlobalcConstants.Deleted;
                    resultMessage += " " + deletedMessage;
                }

                // Handles case where no rules were processed
                if (deletedRules.Count == 0 && notDeletedRules.Count == 0)
                {
                    resultMessage = "No rules were deleted.";
                }
            }
            catch (Exception ex)
            {
                // Sets error message if exception occurs
                resultMessage = ex.Message;
            }

            return resultMessage;
        }
    }
}
