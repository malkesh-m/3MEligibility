using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using DataType = MEligibilityPlatform.Domain.Entities.DataType;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing data types.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DataTypeService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class DataTypeService(IUnitOfWork uow, IMapper mapper) : IDataTypeService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new data type to the repository.
        /// </summary>
        /// <param name="model">The data type model to be added.</param>
        public async Task Add(DataTypeModel model)
        {
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the model to DataType entity and adds it to the repository
            _uow.DataTypeRepository.Add(_mapper.Map<DataType>(model));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a data type from the repository based on the given ID.
        /// </summary>
        /// <param name="id">The ID of the data type to delete.</param>
        public async Task Delete(int id)
        {
            // Retrieves the data type entity by ID from the repository
            var Item = _uow.DataTypeRepository.GetById(id);
            // Removes the data type entity from the repository
            _uow.DataTypeRepository.Remove(Item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all data types from the repository.
        /// </summary>
        /// <returns>A list of all data types mapped to DataTypeModel.</returns>
        public List<DataTypeModel> GetAll()
        {
            // Retrieves all data type entities from the repository
            var dataTypes = _uow.DataTypeRepository.GetAll();
            // Maps the data type entities to DataTypeModel objects and returns the list
            return _mapper.Map<List<DataTypeModel>>(dataTypes);
        }

        /// <summary>
        /// Retrieves a single data type by ID.
        /// </summary>
        /// <param name="id">The ID of the data type to retrieve.</param>
        /// <returns>The data type model mapped from the entity.</returns>
        public DataTypeModel GetById(int id)
        {
            // Retrieves the data type entity by ID from the repository
            var dataType = _uow.DataTypeRepository.GetById(id);
            // Maps the data type entity to DataTypeModel object and returns it
            return _mapper.Map<DataTypeModel>(dataType);
        }

        /// <summary>
        /// Deletes multiple data types from the repository.
        /// </summary>
        /// <param name="ids">The list of data type IDs to delete.</param>
        public async Task MultipleDelete(List<int> ids)
        {
            // Iterates through each data type ID in the list
            foreach (var id in ids)
            {
                // Retrieves the data type entity by ID from the repository
                var item = _uow.DataTypeRepository.GetById(id);
                // Checks if the data type entity exists
                if (item != null)
                {
                    // Removes the data type entity from the repository
                    _uow.DataTypeRepository.Remove(item);
                }
            }
            try
            {
                // Commits the changes to the database
                await _uow.CompleteAsync();
            }
            catch (DbUpdateException ex)
            {
                // Throws a new exception with the original error message if database update fails
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing data type with new values.
        /// </summary>
        /// <param name="model">The data type model containing updated values.</param>
        public async Task Update(DataTypeModel model)
        {
            // Retrieves the existing data type entity by ID from the repository
            var Item = _uow.DataTypeRepository.GetById(model.DataTypeId);
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the updated model to the existing entity and updates it in the repository
            _uow.DataTypeRepository.Update(_mapper.Map<DataTypeModel, DataType>(model, Item));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
