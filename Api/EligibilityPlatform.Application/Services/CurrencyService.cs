using AutoMapper;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides services related to currency management.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CurrencyService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public class CurrencyService(IUnitOfWork uow, IMapper mapper) : ICurrencyService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Adds a new currency to the repository.
        /// </summary>
        /// <param name="model">The currency model to be added.</param>
        public async Task Add(CurrencyModel model)
        {
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the model to Currency entity and adds it to the repository
            _uow.CurrencyRepository.Add(_mapper.Map<Currency>(model));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a currency from the repository based on the given ID.
        /// </summary>
        /// <param name="id">The ID of the currency to delete.</param>
        public async Task Delete(int id)
        {
            // Retrieves the currency entity by ID from the repository
            var Item = _uow.CurrencyRepository.GetById(id);
            // Removes the currency entity from the repository
            _uow.CurrencyRepository.Remove(Item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all currencies from the repository.
        /// </summary>
        /// <returns>A list of all currencies mapped to CurrencyModel.</returns>
        public List<CurrencyModel> GetAll()
        {
            // Retrieves all currency entities from the repository
            var currency = _uow.CurrencyRepository.GetAll();
            // Maps the currency entities to CurrencyModel objects and returns the list
            return _mapper.Map<List<CurrencyModel>>(currency);
        }

        /// <summary>
        /// Retrieves a single currency by ID.
        /// </summary>
        /// <param name="id">The ID of the currency to retrieve.</param>
        /// <returns>The currency model mapped from the entity.</returns>
        public CurrencyModel GetById(int id)
        {
            // Retrieves the currency entity by ID from the repository
            var city = _uow.CurrencyRepository.GetById(id);
            // Maps the currency entity to CurrencyModel object and returns it
            return _mapper.Map<CurrencyModel>(city);
        }

        /// <summary>
        /// Deletes multiple currencies from the repository.
        /// </summary>
        /// <param name="ids">The list of currency IDs to delete.</param>
        public async Task MultipleDelete(List<int> ids)
        {
            // Iterates through each currency ID in the list
            foreach (var id in ids)
            {
                // Retrieves the currency entity by ID from the repository
                var currencyitem = _uow.CurrencyRepository.GetById(id);
                // Checks if the currency entity exists
                if (currencyitem != null)
                {
                    // Removes the currency entity from the repository
                    _uow.CurrencyRepository.Remove(currencyitem);
                }
            }
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing currency with new values.
        /// </summary>
        /// <param name="model">The currency model containing updated values.</param>
        public async Task Update(CurrencyModel model)
        {
            // Retrieves the existing currency entity by ID from the repository
            var Item = _uow.CurrencyRepository.GetById(model.CurrencyId);
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Maps the updated model to the existing entity and updates it in the repository
            _uow.CurrencyRepository.Update(_mapper.Map<CurrencyModel, Currency>(model, Item));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }
    }
}
