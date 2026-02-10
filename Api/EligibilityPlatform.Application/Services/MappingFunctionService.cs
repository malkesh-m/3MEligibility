using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing MappingFunction operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MappingFunctionService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class MappingFunctionService(IUnitOfWork uow, IMapper mapper) : IMappingfunctionService
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
        /// Adds a new mapping function to the database.
        /// </summary>
        /// <param name="model">The MappingFunctionModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(MappingFunctionModel model)
        {
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the incoming model to MappingFunction entity and adds to repository
            _uow.MappingFunctionRepository.Add(_mapper.Map<MappingFunction>(model));

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a mapping function by its ID.
        /// </summary>
        /// <param name="id">The ID of the mapping function to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int id)
        {
            // Retrieves the mapping function by ID
            var mappings = _uow.MappingFunctionRepository.GetById(id);
            // Removes the mapping function from the repository
            _uow.MappingFunctionRepository.Remove(mappings);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all mapping functions.
        /// </summary>
        /// <returns>A list of MappingFunctionModel representing all mapping functions.</returns>
        public List<MappingFunctionModel> GetAll()
        {
            // Retrieves all mapping functions from the repository
            var mappings = _uow.MappingFunctionRepository.GetAll();
            // Maps the mapping functions to MappingFunctionModel objects
            return _mapper.Map<List<MappingFunctionModel>>(mappings);
        }

        /// <summary>
        /// Gets a mapping function by its ID.
        /// </summary>
        /// <param name="id">The ID of the mapping function to retrieve.</param>
        /// <returns>The MappingFunctionModel for the specified ID.</returns>
        public MappingFunctionModel GetById(int id)
        {
            // Retrieves the specific mapping function by ID
            var mappings = _uow.MappingFunctionRepository.GetById(id);
            // Maps the mapping function to MappingFunctionModel object
            return _mapper.Map<MappingFunctionModel>(mappings);
        }

        /// <summary>
        /// Updates an existing mapping function.
        /// </summary>
        /// <param name="model">The MappingFunctionModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(MappingFunctionModel model)
        {
            // Retrieves the existing mapping function by ID
            var mappings = _uow.MappingFunctionRepository.GetById(model.MapFunctionId);
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Updates the mapping function with new data using AutoMapper
            _uow.MappingFunctionRepository.Update(_mapper.Map<MappingFunctionModel, MappingFunction>(model, mappings));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple mapping functions by their IDs.
        /// </summary>
        /// <param name="ids">A list of IDs of the mapping functions to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when one or more provided IDs are not found in the database.</exception>
        public async Task MultipleDelete(List<int> ids)
        {
            // Validates that all provided IDs exist in the database
            foreach (var id in ids)
            {
                var hasvalue = await _uow.MappingFunctionRepository.Query().AnyAsync(item => item.MapFunctionId == id);
                if (hasvalue == false)
                {
                    throw new Exception($"These id's: {id} is not present. Please provide valid id.");
                }
            }

            // Deletes all mapping functions with the provided IDs
            foreach (var id in ids)
            {
                var mapfunitem = _uow.MappingFunctionRepository.GetById(id);
                if (mapfunitem != null)
                {
                    _uow.MappingFunctionRepository.Remove(mapfunitem);
                }
            }

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
