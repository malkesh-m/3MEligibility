using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing product cap amounts.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProductCapAmountService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class ProductCapAmountService(IUnitOfWork uow, IMapper mapper) : IProductCapAmountService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new product cap amount to the database.
        /// </summary>
        /// <param name="model">The <see cref="ProductCapAmountAddModel"/> containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(ProductCapAmountAddModel model)
        {
            // Check if associated product exists
            var isproduct = _uow.ProductRepository.Query().Any(p => p.ProductId == model.ProductId);
            // Throw if product not found
            if (!isproduct)
            {
                throw new Exception("Product is not available.");
            }
            // Map model to entity
            var entity = _mapper.Map<ProductCapAmount>(model);
            // Add entity to repository
            _uow.ProductCapAmountRepository.Add(entity);
            // Save changes
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all product cap amounts.
        /// </summary>
        /// <returns>A task with a list of <see cref="ProductCapAmountModel"/> instances.</returns>
        public List<ProductCapAmountModel> GetAll()
        {
            // Retrieve all entities from repository
            var entities = _uow.ProductCapAmountRepository.GetAll().ToList();
            // Map entities to models
            return _mapper.Map<List<ProductCapAmountModel>>(entities);
        }

        /// <summary>
        /// Updates an existing product cap amount.
        /// </summary>
        /// <param name="model">The <see cref="ProductCapAmountUpdateModel"/> containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(ProductCapAmountUpdateModel model)
        {
            // Check if product exists
            var isproduct = _uow.ProductRepository.Query().Any(p => p.ProductId == model.ProductId);
            if (!isproduct)
            {
                throw new Exception("Product is not available.");
            }

            // Retrieve existing cap amount entity
            var existingEntity = _uow.ProductCapAmountRepository.GetById(model.Id) ?? throw new Exception("Stream Cap Amount not found.");

            // Map update model onto existing entity
            _mapper.Map(model, existingEntity);
            // Update repository
            _uow.ProductCapAmountRepository.Update(existingEntity);
            // Save changes
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a product cap amount by its ID.
        /// </summary>
        /// <param name="id">The product cap amount ID to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int id)
        {
            // Retrieve entity by ID
            var entity = _uow.ProductCapAmountRepository.GetById(id);
            if (entity != null)
            {
                // Remove entity if found
                _uow.ProductCapAmountRepository.Remove(entity);
                // Save changes
                await _uow.CompleteAsync();
            }
            else
            {
                // Throw if not found
                throw new Exception("Stream Cap Amount not found.");
            }
        }

        /// <summary>
        /// Gets all product cap amounts by product ID.
        /// </summary>
        /// <param name="id">The product ID.</param>
        /// <returns>A task with a list of <see cref="ProductCapAmountModel"/> for the specified product ID.</returns>
        public async Task<List<ProductCapAmountModel>> GetByProductId(int id)
        {
            // Query repository by product ID
            var entity = await _uow.ProductCapAmountRepository.Query().Where(p => p.ProductId == id).ToListAsync();
            // Throw if none found
            if (entity.Count == 0)
            {
                throw new Exception("Stream Cap Amount not found.");
            }
            // Map and return models
            return _mapper.Map<List<ProductCapAmountModel>>(entity);
        }
    }
}
