using MapsterMapper;
using MEligibilityPlatform.Application.Repository;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides services for managing API details operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ApiDetailService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class ApiDetailService(IUnitOfWork uow, IMapper mapper, INodeApiRepository nodeApiRepository, INodeModelRepository nodeModelRepository) : IApiDetailService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;
        private readonly INodeApiRepository _nodeApiRepository = nodeApiRepository;
        private readonly INodeModelRepository _nodeModelRepository = nodeModelRepository;

        /// <summary>
        /// Adds a new API detail.
        /// </summary>
        /// <param name="model">The API detail model to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(ApiDetailCreateotUpdateModel model)
        {
            // Maps the model to entity
            var entites = _mapper.Map<Apidetail>(model);
            // Sets creation timestamp
            entites.CreatedByDateTime = DateTime.Now;
            // Sets update timestamp
            entites.UpdatedByDateTime = DateTime.Now;
            // Adds the entity to repository
            _uow.ApiDetailRepository.Add(entites);
            // Completes the unit of work transaction
            await _uow.CompleteAsync();
        }



        /// <summary>
        /// Deletes an API detail by its ID.
        /// </summary>
        /// <param name="id">The ID of the API detail to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int id)
        {
            // Retrieves API detail by ID from repository
            var apiDetails = _uow.ApiDetailRepository.GetById(id);
            // Removes the entity from repository
            _uow.ApiDetailRepository.Remove(apiDetails);
            // Completes the unit of work transaction
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple API details based on the provided list of IDs.
        /// </summary>
        /// <param name="ids">The list of API detail IDs to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteMultiple(List<int> ids)
        {
            // Iterates through each ID in the list
            foreach (var id in ids)
            {
                // Retrieves API detail by ID from repository
                var apis = _uow.ApiDetailRepository.GetById(id);
                // Checks if entity exists
                if (apis != null)
                {
                    // Removes the entity from repository
                    _uow.ApiDetailRepository.Remove(apis);
                }
            }
            // Completes the unit of work transaction
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all API details.
        /// </summary>
        /// <returns>A list of API detail models.</returns>
        public List<ApiDetailListModel> GetAll()
        {
            // Retrieves all entities from repository
            var apiDetails = _uow.ApiDetailRepository.GetAll();
            // Maps entities to models and returns
            return _mapper.Map<List<ApiDetailListModel>>(apiDetails);
        }

        /// <summary>
        /// Retrieves an API detail by its ID.
        /// </summary>
        /// <param name="id">The ID of the API detail.</param>
        /// <returns>The API detail model if found; otherwise, null.</returns>
        public ApiDetailListModel GetById(int id)
        {
            // Retrieves entity by ID from repository
            var apiDetail = _uow.ApiDetailRepository.GetById(id);
            // Maps entity to model and returns
            return _mapper.Map<ApiDetailListModel>(apiDetail);
        }

        /// <summary>
        /// Updates an existing API detail.
        /// </summary>
        /// <param name="model">The API detail model to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(ApiDetailCreateotUpdateModel model)
        {
            // Retrieves the existing entity by ID
            var apiDetail = _uow.ApiDetailRepository.GetById(model.ApidetailsId);
            // Maps model properties to existing entity
            var entites = _mapper.Map<ApiDetailCreateotUpdateModel, Apidetail>(model, apiDetail);
            // Sets update timestamp
            entites.UpdatedByDateTime = DateTime.Now;
            // Updates the entity in repository
            _uow.ApiDetailRepository.Update(entites);
            // Completes the unit of work transaction
            await _uow.CompleteAsync();
        }

        public async Task<List<ApiListModel>> GetAllApiDetailsWithNode(int tenantId)
        {
            return await _nodeApiRepository.Query().Where(n=>n.TenantId==tenantId)
     .Join(_nodeModelRepository.Query(),
           api => api.NodeId,
           node => node.NodeId,
           (api, node) => new ApiListModel
           {
               Apiid = api.Apiid,
               Apiname = api.Apiname,
               EndpointPath = api.EndpointPath,
               HttpMethodType = api.HttpMethodType,
               Apidesc = api.Apidesc,
               NodeId = api.NodeId,
               NodeUrl = node.NodeUrl,
               BinaryXml = api.BinaryXml,
               XmlfileName = api.XmlfileName,
               Header = api.Header,
               CreatedBy = api.CreatedBy,
               CreatedByDateTime = api.CreatedByDateTime,
               UpdatedBy = api.UpdatedBy,
               UpdatedByDateTime = api.UpdatedByDateTime,
               RequestParameters = api.RequestParameters,
               RequestBody = api.RequestBody,
               ResponseFormate = api.ResponseFormate
           })
     .ToListAsync();
        }
    }
}