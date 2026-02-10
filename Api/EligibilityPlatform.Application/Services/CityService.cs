using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CityService"/> class.
    /// </summary>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class CityService(IUnitOfWork uow, IMapper mapper) : ICityService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new city to the repository.
        /// </summary>
        /// <param name="model">The city model containing details to be added.</param>
        public async Task Add(CityModel model)
        {
            // Sets CountryId to null if it is 0, otherwise keeps the original value
            model.CountryId = model.CountryId == 0 ? (int?)null : model.CountryId;
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the model to City entity and adds it to the repository
            _uow.CityRepository.Add(_mapper.Map<City>(model));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a city from the repository by its ID.
        /// </summary>
        /// <param name="id">The ID of the city to be deleted.</param>
        public async Task Delete(int id)
        {
            // Retrieves the city entity by ID from the repository
            var Item = _uow.CityRepository.GetById(id);
            // Removes the city entity from the repository
            _uow.CityRepository.Remove(Item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple cities from the repository based on a list of IDs.
        /// </summary>
        /// <param name="ids">List of city IDs to be deleted.</param>
        public async Task DeleteMultiple(List<int> ids)
        {
            // Iterates through each city ID in the list
            foreach (var id in ids)
            {
                // Retrieves the city entity by ID from the repository
                var products = _uow.CityRepository.GetById(id);
                // Checks if the city entity exists
                if (products != null)
                {
                    // Removes the city entity from the repository
                    _uow.CityRepository.Remove(products);
                }
            }
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all cities from the repository.
        /// </summary>
        /// <returns>A list of city models.</returns>
        public List<CityModel> GetAll()
        {
            // Retrieves all city entities from the repository
            var Cities = _uow.CityRepository.GetAll();
            // Maps the city entities to CityModel objects and returns the list
            return _mapper.Map<List<CityModel>>(Cities);
        }

        /// <summary>
        /// Retrieves a city by its ID.
        /// </summary>
        /// <param name="id">The ID of the city to retrieve.</param>
        /// <returns>The city model corresponding to the given ID.</returns>
        public CityModel GetById(int id)
        {
            // Retrieves the city entity by ID from the repository
            var city = _uow.CityRepository.GetById(id);
            // Maps the city entity to CityModel object and returns it
            return _mapper.Map<CityModel>(city);
        }

        /// <summary>
        /// Updates an existing city in the repository.
        /// </summary>
        /// <param name="model">The updated city model.</param>
        public async Task Update(CityModel model)
        {
            // Retrieves the existing city entity by ID from the repository
            var Item = _uow.CityRepository.GetById(model.CityId);
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the updated model to the existing entity and updates it in the repository
            _uow.CityRepository.Update(_mapper.Map<CityModel, City>(model, Item));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
