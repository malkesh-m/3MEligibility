using AutoMapper;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing product caps.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProductCapService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class ProductCapService(IUnitOfWork uow, IMapper mapper) : IProductCapService
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
        /// Adds a new product cap to the database.
        /// </summary>
        /// <param name="model">The ProductCapModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(ProductCapModel model)
        {
            // Maps the incoming model to ProductCap entity
            var entity = _mapper.Map<ProductCap>(model);
            // Adds the product cap entity to the repository
            _uow.ProductCapRepository.Add(entity);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a product cap by its ID.
        /// </summary>
        /// <param name="id">The product cap ID to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int id)
        {
            // Retrieves the product cap by ID
            var entity = _uow.ProductCapRepository.GetById(id);
            // Checks if the entity exists
            if (entity != null)
            {
                // Removes the product cap from the repository
                _uow.ProductCapRepository.Remove(entity);
                // Commits the changes to the database
                await _uow.CompleteAsync();
            }
            else
            {
                // Throws exception if product cap is not found
                throw new Exception("Product Cap not found.");
            }
        }

        /// <summary>
        /// Gets all product caps.
        /// </summary>
        /// <returns>A list of ProductCapModel representing all product caps.</returns>
        public List<ProductCapModel> GetAll()
        {
            // Retrieves all product caps from the repository
            var entities = _uow.ProductCapRepository.GetAll();
            // Maps the product caps to ProductCapModel objects
            return _mapper.Map<List<ProductCapModel>>(entities);
        }

        /// <summary>
        /// Gets all product caps by product ID.
        /// </summary>
        /// <param name="id">The product ID.</param>
        /// <returns>A list of ProductCapModel for the specified product ID.</returns>
        public async Task<List<ProductCapModel>> GetByProductId(int id)
        {
            // Retrieves product caps filtered by product ID
            var entity = await _uow.ProductCapRepository.Query().Where(p => p.ProductId == id).ToListAsync();
            // Throws exception if no product caps found for the product ID
            if (entity.Count == 0)
                throw new Exception("Product Cap not found.");

            // Maps the product caps to ProductCapModel objects
            return _mapper.Map<List<ProductCapModel>>(entity);
        }

        /// <summary>
        /// Gets a product cap by its ID.
        /// </summary>
        /// <param name="id">The product cap ID to retrieve.</param>
        /// <returns>The ProductCapModel for the specified ID.</returns>
        public ProductCapModel GetById(int id)
        {
            // Retrieves the product cap by ID
            var entity = _uow.ProductCapRepository.GetById(id) ?? throw new Exception("Product Cap not found.");

            // Maps the product cap to ProductCapModel object
            return _mapper.Map<ProductCapModel>(entity);
        }

        /// <summary>
        /// Updates an existing product cap.
        /// </summary>
        /// <param name="model">The ProductCapModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(ProductCapModel model)
        {
            // Retrieves the existing product cap by ID
            var existingEntity = _uow.ProductCapRepository.GetById(model.Id) ?? throw new Exception("Product Cap not found.");

            // Maps updated data from model to existing entity
            _mapper.Map(model, existingEntity);
            // Updates the product cap in the repository
            _uow.ProductCapRepository.Update(existingEntity);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
