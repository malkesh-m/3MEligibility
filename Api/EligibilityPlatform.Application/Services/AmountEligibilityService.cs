using AutoMapper;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Provides services for managing amount eligibility and related calculations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AmountEligibilityService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="pcardService">The PCard service.</param>
    public class AmountEligibilityService(IUnitOfWork uow, IMapper mapper, IPcardService pcardService) : IAmountEligibilityService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;
        private readonly IPcardService _pcardService = pcardService;

        /// <summary>
        /// Adds a new amount eligibility record.
        /// </summary>
        /// <param name="model">The amount eligibility model to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(AmountEligibilityModel model)
        {
            var Pcard = _uow.PcardRepository.Query().Where(p => p.PcardId == model.PcardID).FirstOrDefault() ?? throw new Exception("No Pcard found");

            // Maps the model to entity
            var entity = _mapper.Map<AmountEligibility>(model);
            // Adds the entity to repository
            _uow.AmountEligibilityRepository.Add(entity);
            // Completes the unit of work transaction
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes an amount eligibility record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the amount eligibility record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int id)
        {
            // Retrieves the entity by ID
            var entity = _uow.AmountEligibilityRepository.GetById(id);
            // Checks if entity exists
            if (entity != null)
            {
                // Removes the entity from repository
                _uow.AmountEligibilityRepository.Remove(entity);
                // Completes the unit of work transaction
                await _uow.CompleteAsync();
            }
        }

        /// <summary>
        /// Retrieves all amount eligibility records.
        /// </summary>
        /// <returns>A list of amount eligibility models.</returns>
        public List<AmountEligibilityModel> GetAll()
        {
            // Retrieves all entities from repository
            var entities = _uow.AmountEligibilityRepository.GetAll();
            // Maps entities to models and returns
            return _mapper.Map<List<AmountEligibilityModel>>(entities);
        }

        /// <summary>
        /// Retrieves an amount eligibility record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the amount eligibility record.</param>
        /// <returns>The amount eligibility model if found; otherwise, null.</returns>
        public AmountEligibilityModel GetById(int id)
        {
            // Retrieves entity by ID from repository
            var entity = _uow.AmountEligibilityRepository.GetById(id);
            // Maps entity to model and returns
            return _mapper.Map<AmountEligibilityModel>(entity);
        }

        /// <summary>
        /// Updates an existing amount eligibility record.
        /// </summary>
        /// <param name="model">The amount eligibility model to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(AmountEligibilityModel model)
        {
            var Pcard = _uow.PcardRepository.Query().Where(p => p.PcardId == model.PcardID).FirstOrDefault() ?? throw new Exception("No Pcard found");

            // Retrieves the existing entity by ID
            var entity = _uow.AmountEligibilityRepository.GetById(model.Id);
            // Checks if entity exists
            if (entity != null)
            {
                // Maps model properties to entity
                _mapper.Map(model, entity);
                // Updates the entity in repository
                _uow.AmountEligibilityRepository.Update(entity);
                // Completes the unit of work transaction
                await _uow.CompleteAsync();
            }
        }

        /// <summary>
        /// Calculates the eligible amount based on entity, pre-amount, and PCard ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="Preamount">The pre-amount value as a string.</param>
        /// <param name="pcardId">The PCard ID.</param>
        /// <returns>The calculated eligible amount as a string.</returns>
        public string AmountCalculate(int tenantId, string Preamount, int pcardId)
        {
            // Retrieves PCard by entity ID and PCard ID
            var pcard = _pcardService.GetById(tenantId, pcardId);
            // Validates if PCard exists
            if (pcard == null)
            {
                // Returns error message for invalid card
                return "Invalid Card";
            }

            // Queries exception management repository
            var exceptionManagement = _uow.ExceptionManagementRepository.Query()
                                              //.Where(x => x.ProductId == pcard.ProductId)
                                              .First();

            // Retrieves amount eligibility list for the PCard
            var amountList = _uow.AmountEligibilityRepository.Query().Where(x => x.PcardId == pcardId);

            //if (ExceptionMangeVal == null)
            //{
            //    return "No Exception Management Record Found";
            //}

            // Gets current date for date range validation
            DateTime currentDate = DateTime.Now;

            // Checks if current date is within exception management date range
            bool isWithinDateRange = exceptionManagement != null
                         && exceptionManagement.StartDate != null
                         && exceptionManagement.EndDate != null
                         && currentDate >= exceptionManagement.StartDate
                         && currentDate <= exceptionManagement.EndDate;

            // Processes exception management rules if within date range
            if (isWithinDateRange)
            {
                // Handles fixed percentage calculation
                if (exceptionManagement?.FixedPercentage != 0)
                {
                    // Parses fixed percentage value
                    if (decimal.TryParse(exceptionManagement?.FixedPercentage.ToString(), out decimal fixedPercentValue))
                    {
                        // Calculates fixed amount (TODO: Replace hardcoded value with actual product amount)
                        decimal fixedAmount = 100;
                        // Returns formatted fixed amount
                        return fixedAmount.ToString("0.00");
                    }
                    else
                    {
                        // Returns error for invalid fixed percentage
                        return "Invalid Fixed Percentage";
                    }
                }

                // Handles variation percentage calculation
                if (exceptionManagement.VariationPercentage != 0)
                {
                    // Parses variation percentage value
                    if (decimal.TryParse(exceptionManagement.VariationPercentage.ToString(), out decimal variationPercentValue))
                    {
                        // Adjusts pre-amount with variation percentage
                        Preamount = (decimal.Parse(Preamount) + variationPercentValue).ToString();
                    }
                    else
                    {
                        // Returns error for invalid variation percentage
                        return "Invalid Variation Percentage";
                    }
                }
            }

            // Initializes calculated amount
            decimal calculateAmount = 0;
            string percentage = Preamount;

            // Parses percentage value for eligibility calculation
            if (int.TryParse(percentage.ToString(), out int percentageValue))
            {
                // Sorts amount list by eligible percentage in descending order
                var sortedList = amountList.OrderByDescending(x => x.EligiblePercentage).ToList();

                // Iterates through sorted list to find eligible percentage
                foreach (var item in sortedList)
                {
                    int EligiblePercentage = item.EligiblePercentage;

                    // Parses eligible percentage value
                    if (int.TryParse(EligiblePercentage.ToString(), out int eligiblePercentailValue))
                    {
                        // Checks if current percentage meets eligibility criteria
                        if (percentageValue >= eligiblePercentailValue)
                        {
                            int eligiblePercent = item.AmountPrcentage;
                            // Calculates eligible amount (TODO: Replace hardcoded value with actual product amount)
                            //calculateAmount = (pcard.Amount * eligiblePercent) / 100;
                            break;
                        }
                    }
                }
            }
            // Returns formatted calculated amount
            return calculateAmount.ToString("0.00");
        }

        //public decimal AmountCalculate(string Preamount, int pcardId)
        //{
        //    var pcard = _pcardService.GetById(pcardId);
        //    if (pcard == null)
        //    {
        //        throw new Exception("Invalid Card");
        //    }

        //    var exceptionManagement = _uow.ExceptionManagementRepository.Query()
        //                                      //.Where(x => x.ProductId == pcard.ProductId)
        //                                      .First();

        //    var amountList = _uow.AmountEligibilityRepository.Query().Where(x => x.PcardID == pcardId);


        //    //if (ExceptionMangeVal == null)
        //    //{
        //    //    return "No Exception Management Record Found";
        //    //}

        //    DateTime currentDate = DateTime.Now;

        //    bool isWithinDateRange = exceptionManagement != null
        //                 && exceptionManagement.StartDate != null
        //                 && exceptionManagement.EndDate != null
        //                 && currentDate >= exceptionManagement.StartDate
        //                 && currentDate <= exceptionManagement.EndDate;



        //    if (isWithinDateRange)
        //    {
        //        if (exceptionManagement.FixedPercentage != 0)
        //        {

        //            if (decimal.TryParse(exceptionManagement.FixedPercentage.ToString(), out decimal fixedPercentValue))
        //            {
        //                decimal fixedAmount = (exceptionManagement.LimitAmount * fixedPercentValue) / 100;
        //                return fixedAmount.ToString("0.00");
        //            }
        //            else
        //            {
        //                return "Invalid Fixed Percentage";
        //            }
        //        }

        //        if (exceptionManagement.VariationPercentage != 0)
        //        {
        //            if (decimal.TryParse(exceptionManagement.VariationPercentage.ToString(), out decimal variationPercentValue))
        //            {
        //                Preamount = (exceptionManagement.LimitAmount + ((exceptionManagement.LimitAmount * variationPercentValue) / 100)).ToString();
        //            }
        //            else
        //            {
        //                return "Invalid Variation Percentage";
        //            }
        //        }
        //    }

        //    decimal calculateAmount = 0;
        //    string percentage = Preamount;

        //    if (int.TryParse(percentage.ToString(), out int percentageValue))
        //    {
        //        var sortedList = amountList.OrderByDescending(x => x.EligiblePercentage).ToList();

        //        foreach (var item in sortedList)
        //        {
        //            int EligiblePercentage = item.EligiblePercentage;

        //            if (int.TryParse(EligiblePercentage.ToString(), out int eligiblePercentailValue))
        //            {
        //                if (percentageValue >= eligiblePercentailValue)
        //                {
        //                    int eligiblePercent = item.AmountPrcentage;
        //                    calculateAmount = (pcard.Amount * eligiblePercent) / 100;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    return calculateAmount.ToString("0.00");

        //}

    }
}