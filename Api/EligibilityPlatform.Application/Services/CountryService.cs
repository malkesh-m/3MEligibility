using System.Diagnostics.Metrics;
using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides functionality to manage countries, including adding, deleting, and updating country information.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CountryService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class CountryService(IUnitOfWork uow, IMapper mapper) : ICountryService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new country to the repository.
        /// </summary>
        /// <param name="model">The country model to be added.</param>
        public async Task Add(CountryModel model)
        {
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the model to Country entity and adds it to the repository
            _uow.CountryRepository.Add(_mapper.Map<Country>(model));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a country from the repository based on the given ID.
        /// </summary>
        /// <param name="id">The ID of the country to delete.</param>
        public async Task Delete(int id)
        {
            // Retrieves the country entity by ID from the repository
            var Item = _uow.CountryRepository.GetById(id);
            // Removes the country entity from the repository
            _uow.CountryRepository.Remove(Item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all countries from the repository.
        /// </summary>
        /// <returns>A list of all countries mapped to CountryModel.</returns>
        public List<CountryModel> GetAll()
        {
            // Retrieves all country entities from the repository
            var countries = _uow.CountryRepository.GetAll();
            // Maps the country entities to CountryModel objects and returns the list
            return _mapper.Map<List<CountryModel>>(countries);
        }

        /// <summary>
        /// Retrieves a single country by ID.
        /// </summary>
        /// <param name="id">The ID of the country to retrieve.</param>
        /// <returns>The country model mapped from the entity.</returns>
        public CountryModel GetById(int id)
        {
            // Retrieves the country entity by ID from the repository
            var countries = _uow.CountryRepository.GetById(id);
            // Maps the country entity to CountryModel object and returns it
            return _mapper.Map<CountryModel>(countries);
        }

        /// <summary>
        /// Deletes multiple countries from the repository.
        /// </summary>
        /// <param name="ids">The list of country IDs to delete.</param>
        public async Task MultipleDelete(List<int> ids)
        {
            // Iterates through each country ID in the list
            foreach (var id in ids)
            {
                // Retrieves the country entity by ID from the repository
                var item = _uow.CountryRepository.GetById(id);
                // Checks if the country entity exists
                if (item != null)
                {
                    // Removes the country entity from the repository
                    _uow.CountryRepository.Remove(item);
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
        /// Updates an existing country with new values.
        /// </summary>
        /// <param name="model">The country model containing updated values.</param>
        public async Task Update(CountryModel model)
        {
            // Retrieves the existing country entity by ID from the repository
            var countries = _uow.CountryRepository.GetById(model.CountryId);
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the updated model to the existing entity and updates it in the repository
            _uow.CountryRepository.Update(_mapper.Map<CountryModel, Country>(model, countries));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
