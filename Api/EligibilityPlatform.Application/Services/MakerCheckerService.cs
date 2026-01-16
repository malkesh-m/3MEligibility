using AutoMapper;
using CsvHelper.Configuration;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Enums;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing maker-checker workflow operations including approval, rejection, and tracking of changes.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MakerCheckerService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance for database operations.</param>
    /// <param name="mapper">The AutoMapper instance for object mapping.</param>
    /// <param name="factorService">The factor service instance for factor operations.</param>
    /// <param name="parameterService">The parameter service instance for parameter operations.</param>
    /// <param name="apiDetailService">The API detail service instance for API operations.</param>
    /// <param name="categoryService">The category service instance for category operations.</param>
    /// <param name="cityService">The city service instance for city operations.</param>
    /// <param name="conditionService">The condition service instance for condition operations.</param>
    /// <param name="countryService">The country service instance for country operations.</param>
    /// <param name="currencyService">The currency service instance for currency operations.</param>
    /// <param name="dataTypeService">The data type service instance for data type operations.</param>
    /// <param name="ecardService">The ecard service instance for ecard operations.</param>
    /// <param name="entityService">The entity service instance for entity operations.</param>
    /// <param name="eruleService">The erule service instance for erule operations.</param>
    /// <param name="groupRoleService">The group role service instance for group role operations.</param>
    /// <param name="historyEcService">The history EC service instance for EC history operations.</param>
    /// <param name="historyErService">The history ER service instance for ER history operations.</param>
    /// <param name="historyParameterService">The history parameter service instance for parameter history operations.</param>
    /// <param name="historyPcService">The history PC service instance for PC history operations.</param>
    /// <param name="listItemService">The list item service instance for list item operations.</param>
    /// <param name="managedListService">The managed list service instance for managed list operations.</param>
    /// <param name="mappingfunctionService">The mapping function service instance for mapping operations.</param>
    /// <param name="nodeApiService">The node API service instance for node API operations.</param>
    /// <param name="nodeService">The node service instance for node operations.</param>
    /// <param name="paramtersMapService">The parameters map service instance for parameter mapping operations.</param>
    /// <param name="pcardService">The pcard service instance for pcard operations.</param>
    /// <param name="productService">The product service instance for product operations.</param>
    /// <param name="productParamservice">The product parameter service instance for product parameter operations.</param>
    /// <param name="userService">The user service instance for user operations.</param>
    /// <param name="exceptionManagement">The exception management service instance for exception handling.</param>
    /// <param name="productCapService">The product cap service instance for product cap operations.</param>
    /// <param name="eruleMasterService">The erule master service instance for erule master operations.</param>
    /// <param name="securityGroupService">The security group service instance for security group operations.</param>
    /// <param name="userGroupService">The user group service instance for user group operations.</param>
    /// <param name="productCapAmountService">The product cap amount service instance for product cap amount operations.</param>
    public class MakerCheckerService(IUnitOfWork uow, IMapper mapper, IFactorService factorService, IParameterService parameterService,
        IApiDetailService apiDetailService, ICategoryService categoryService, ICityService cityService, IConditionService conditionService, ICountryService countryService,
        ICurrencyService currencyService, IDataTypeService dataTypeService, IEcardService ecardService, /*IEntityService entityService,*/ IEruleService eruleService, IGroupRoleService groupRoleService,
        IHistoryEcService historyEcService, IHistoryErService historyErService, IHistoryParameterService historyParameterService, IHistoryPcService historyPcService,
        IListItemService listItemService, IManagedListService managedListService, IMappingfunctionService mappingfunctionService, INodeApiService nodeApiService, INodeService nodeService, IParamtersMapService paramtersMapService,
        IPcardService pcardService, IProductService productService, IProductParamservice productParamservice, IUserService userService, IExceptionManagementService exceptionManagement, IProductCapService productCapService,
        IEruleMasterService eruleMasterService, ISecurityGroupService securityGroupService, IUserGroupService userGroupService, IProductCapAmountService productCapAmountService, IApiParametersService apiParametersService, IApiParameterMapservice apiParameterMapservice) : IMakerCheckerService
    {
        // The unit of work instance for database operations and transaction management
        private readonly IUnitOfWork _uow = uow;

        // The AutoMapper instance for converting between entities and models
        private readonly IMapper _mapper = mapper;

        // Service dependencies for various domain operations
        private readonly IFactorService _factorService = factorService;
        private readonly IParameterService _parameterService = parameterService;
        private readonly IApiDetailService _apiDetailService = apiDetailService;
        private readonly ICategoryService _categoryService = categoryService;
        private readonly ICityService _cityService = cityService;
        private readonly IConditionService _conditionService = conditionService;
        private readonly ICountryService _countryService = countryService;
        private readonly ICurrencyService _currencyService = currencyService;
        private readonly IDataTypeService _dataTypeService = dataTypeService;
        private readonly IEcardService _ecardService = ecardService;
        //private readonly IEntityService _entityService = entityService;
        private readonly IEruleService _eruleService = eruleService;
        private readonly IGroupRoleService _groupRoleService = groupRoleService;
        private readonly IHistoryEcService _historyEcService = historyEcService;
        private readonly IHistoryErService _historyErService = historyErService;
        private readonly IHistoryParameterService _historyParameterService = historyParameterService;
        private readonly IHistoryPcService _historyPcService = historyPcService;
        private readonly IListItemService _listItemService = listItemService;
        private readonly IManagedListService _managedListService = managedListService;
        private readonly IMappingfunctionService _mappingfunctionService = mappingfunctionService;
        private readonly INodeApiService _nodeApiService = nodeApiService;
        private readonly INodeService _nodeService = nodeService;
        private readonly IParamtersMapService _paramtersMapService = paramtersMapService;
        private readonly IPcardService _pcardService = pcardService;
        private readonly IProductService _productService = productService;
        private readonly IProductParamservice _productParamservice = productParamservice;
        private readonly IUserService _userService = userService;
        private readonly IExceptionManagementService _exceptionManagement = exceptionManagement;
        private readonly IProductCapService _productCapService = productCapService;
        private readonly IEruleMasterService _eruleMasterService = eruleMasterService;
        private readonly ISecurityGroupService _securityGroupService = securityGroupService;
        private readonly IUserGroupService _userGroupService = userGroupService;
        private readonly IProductCapAmountService _productCapAmountService = productCapAmountService;
        private readonly IApiParametersService _apiParametersService = apiParametersService;
        private readonly IApiParameterMapservice _apiParameterMapservice = apiParameterMapservice;
        /// <summary>
        /// Adds a new MakerChecker record to the database.
        /// </summary>
        /// <param name="model">The MakerCheckerModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(MakerCheckerAddUpdateModel model)
        {
            // Set the last updated timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            model.MakerDate = DateTime.Now;
            // Map the model to entity and add to repository
            _uow.MakerCheckerRepository.Add(_mapper.Map<MakerChecker>(model));


            // Save changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Adds a new MakerChecker record using a MakerCheckerModelCopy.
        /// </summary>
        /// <param name="modelCopy">The MakerCheckerModelCopy containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(MakerCheckerModelCopy modelCopy)
        {
            // Set checker ID to null for new records
            modelCopy.CheckerId = null;

            // Set status to Approved for direct additions
            modelCopy.Status = MakerCheckerStatusEnum.Approved;

            // Set the last updated timestamp to current UTC time
            modelCopy.UpdatedByDateTime = DateTime.UtcNow;

            // Map the model copy to entity and add to repository
            _uow.MakerCheckerRepository.Add(_mapper.Map<MakerChecker>(modelCopy));

            // Save changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all MakerChecker records.
        /// </summary>
        /// <returns>A list of MakerCheckerModel representing all records.</returns>
        public List<MakerCheckerModel> GetAll()
        {
            // Query maker checker records joined with user data to get maker and checker names
            var makerChecker = from mc in _uow.MakerCheckerRepository.Query().OrderByDescending(p => p.MakerDate)
                               join u in _uow.UserRepository.Query() on mc.MakerId equals u.UserId
                               select new MakerCheckerModel
                               {
                                   // Map maker checker ID
                                   MakerCheckerId = mc.MakerCheckerId,

                                   // Map maker ID
                                   MakerId = mc.MakerId,

                                   // Map checker ID
                                   CheckerId = mc.CheckerId,

                                   // Map maker date
                                   MakerDate = mc.MakerDate,

                                   // Map checker date
                                   CheckerDate = mc.CheckerDate,

                                   // Map table name
                                   TableName = mc.TableName,

                                   // Map action name
                                   ActionName = mc.ActionName,

                                   // Map old value JSON
                                   OldValueJson = mc.OldValueJson,

                                   // Map new value JSON
                                   NewValueJson = mc.NewValueJson,

                                   // Map record ID
                                   RecordId = mc.RecordId,

                                   // Map status as enum
                                   Status = (MakerCheckerStatusEnum)mc.Status,

                                   // Get status name from enum value
                                   StatusName = ((MakerCheckerStatusEnum)mc.Status).ToString(),

                                   // Map comment
                                   Comment = mc.Comment,

                                   // Map last updated timestamp
                                   UpdatedByDateTime = mc.UpdatedByDateTime,

                                   // Map maker name from user table
                                   MakerName = u.UserName,

                                   // Map checker name if checker ID exists, otherwise null
                                   CheckerName = mc.CheckerId != null ? _uow.UserRepository.Query().FirstOrDefault(mmc => mmc.UserId == mc.CheckerId)!.UserName : null,
                               };

            // Return the query results as a list
            return [.. makerChecker];
        }

        /// <summary>
        /// Gets a MakerChecker record by its ID.
        /// </summary>
        /// <param name="id">The ID of the MakerChecker record to retrieve.</param>
        /// <returns>The MakerCheckerModel for the specified ID.</returns>
        public MakerCheckerModel? GetById(int id)
        {
            // Query maker checker records with specific ID joined with user data
            var makerChecker = _uow.MakerCheckerRepository.Query()
                // Filter by maker checker ID
                .Where(mc => mc.MakerCheckerId == id)

                // Join with user repository to get maker details
                .Join(_uow.UserRepository.Query(),
                      mc => mc.MakerId,
                      u => u.UserId,
                      (mc, u) => new { mc, u })

                // Select and project to MakerCheckerModel
                .Select(x => new MakerCheckerModel
                {
                    // Map maker checker ID
                    MakerCheckerId = x.mc.MakerCheckerId,

                    // Map maker ID
                    MakerId = x.mc.MakerId,

                    // Map checker ID
                    CheckerId = x.mc.CheckerId,

                    // Map maker date
                    MakerDate = x.mc.MakerDate,

                    // Map checker date
                    CheckerDate = x.mc.CheckerDate,

                    // Map table name
                    TableName = x.mc.TableName,

                    // Map action name
                    ActionName = x.mc.ActionName,

                    // Map old value JSON
                    OldValueJson = x.mc.OldValueJson,

                    // Map new value JSON
                    NewValueJson = x.mc.NewValueJson,

                    // Map record ID
                    RecordId = x.mc.RecordId,

                    // Map status as enum
                    Status = (MakerCheckerStatusEnum)x.mc.Status,

                    // Get status name from status value
                    StatusName = x.mc.Status.ToString(),

                    // Map comment
                    Comment = x.mc.Comment,

                    // Map last updated timestamp
                    UpdatedByDateTime = x.mc.UpdatedByDateTime,

                    // Map maker name from user table
                    MakerName = x.u.UserName,

                    // Map checker name if checker ID exists using subquery
                    CheckerName = x.mc.CheckerId != null
                      ? _uow.UserRepository.Query()
                      .Where(mmc => mmc.UserId == x.mc.CheckerId)
                      .Select(mmc => mmc.UserName)
                      .FirstOrDefault()
                      : null,
                })
                // Get first record or default (null if not found)
                .FirstOrDefault();

            // Return the found maker checker record
            return makerChecker;
        }

        /// <summary>
        /// Removes a MakerChecker record by its ID.
        /// </summary>
        /// <param name="id">The ID of the MakerChecker record to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Remove(int id)
        {
            // Retrieve the maker checker entity by ID
            var makerChecker = _uow.MakerCheckerRepository.GetById(id);

            // Mark the entity for removal
            _uow.MakerCheckerRepository.Remove(makerChecker);

            // Save the deletion to the database
            await _uow.CompleteAsync();
        }
        /// <summary>
        /// Updates a MakerChecker record and processes the associated action if the status is approved.
        /// </summary>
        /// <param name="entityId">The entity identifier used for entity-specific operations.</param>
        /// <param name="model">The MakerCheckerModel containing the updated data and status information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(int entityId, MakerCheckerModel model)
        {
            // Retrieve the existing MakerChecker entity from the database using the ID from the model
            var makerChecker = _uow.MakerCheckerRepository.GetById(model.MakerCheckerId);

            // Update the timestamp to indicate when this record was last modified
            model.UpdatedByDateTime = DateTime.UtcNow;

            // Map the updated model data to the existing entity and update it in the repository
            _uow.MakerCheckerRepository.Update(_mapper.Map<MakerCheckerModel, MakerChecker>(model, makerChecker));

            // Store the action name for potential use in switch statements or logging
            var actionName = model.ActionName;

            // Check if the status of the MakerChecker record is set to Approved
            if (model.Status == MakerCheckerStatusEnum.Approved)
            {
                // Determine which entity table this operation applies to
                switch (model.TableName)
                {
                    // Handle operations for the Factor entity
                    case nameof(Factor):
                        // Process Add operation for Factor
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Factor values
                            var newValue = JsonConvert.DeserializeObject<FactorAddUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            newValue.TenantId = entityId;
                            // Call the service to add the new Factor to the database
                            await _factorService.Add(newValue);
                        }
                        // Process Update operation for Factor
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Factor values
                            var newValue = JsonConvert.DeserializeObject<FactorAddUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the Factor update
                            newValue.TenantId = entityId;
                            // Call the service to update the existing Factor in the database
                            await _factorService.Update(newValue);
                        }
                        // Process Delete operation for Factor
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Factor to be deleted
                            var newValue = JsonConvert.DeserializeObject<FactorModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            var dtId = newValue.FactorId;
                            // Call the service to delete the Factor from the database
                            await _factorService.Delete(entityId, dtId);
                        }
                        // Exit the Factor case
                        break;

                    // Handle operations for the Apidetail entity
                    case nameof(Apidetail):
                        // Process Add operation for Apidetail
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Apidetail values
                            var newValue = JsonConvert.DeserializeObject<ApiDetailCreateotUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new Apidetail to the database
                            await _apiDetailService.Add(newValue);
                        }
                        // Process Update operation for Apidetail
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Apidetail values
                            var newValue = JsonConvert.DeserializeObject<ApiDetailCreateotUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing Apidetail in the database
                            await _apiDetailService.Update(newValue);
                        }
                        // Process Delete operation for Apidetail
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Apidetail to be deleted
                            var newValue = JsonConvert.DeserializeObject<ApiDetailModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the Apidetail ID from the deserialized object
                            var dtId = newValue.ApidetailsId;
                            // Call the service to delete the Apidetail from the database
                            await _apiDetailService.Delete(dtId);
                        }
                        // Exit the Apidetail case
                        break;

                    // Handle operations for the Parameter entity
                    case nameof(Parameter):
                        // Process Add operation for Parameter
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Parameter values
                            var newValue = JsonConvert.DeserializeObject<ParameterAddUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the new Parameter record
                            newValue.TenantId = entityId;
                            // Call the service to add the new Parameter to the database
                            await _parameterService.Add(newValue);
                        }
                        // Process Update operation for Parameter
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Parameter values
                            var newValue = JsonConvert.DeserializeObject<ParameterAddUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the Parameter update
                            newValue.TenantId = entityId;
                            // Call the service to update the existing Parameter in the database
                            await _parameterService.Update(newValue);
                        }
                        // Process Delete operation for Parameter
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Parameter to be deleted
                            var newValue = JsonConvert.DeserializeObject<ParameterModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the Parameter ID from the deserialized object
                            var dtId = newValue.ParameterId;
                            // Call the service to delete the Parameter from the database
                            await _parameterService.Delete(entityId, dtId);
                        }
                        // Exit the Parameter case
                        break;

                    // Handle operations for the Category entity
                    case nameof(Category):
                        // Process Add operation for Category
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Category values
                            var newValue = JsonConvert.DeserializeObject<CategoryCreateUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the new Category record
                            newValue.TenantId = entityId;
                            // Call the service to add the new Category to the database
                            await _categoryService.Add(newValue);
                        }
                        // Process Update operation for Category
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Category values
                            var newValue = JsonConvert.DeserializeObject<CategoryUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the Category update
                            newValue.TenantId = entityId;
                            // Call the service to update the existing Category in the database
                            await _categoryService.Update(newValue);
                        }
                        // Process Delete operation for Category
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Category to be deleted
                            var newValue = JsonConvert.DeserializeObject<CategoryModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the Category ID from the deserialized object
                            var dtId = newValue.CategoryId;
                            // Call the service to remove the Category from the database
                            await _categoryService.Remove(entityId, dtId);
                        }
                        // Exit the Category case
                        break;

                    // Handle operations for the City entity
                    case nameof(City):
                        // Process Add operation for City
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new City values
                            var newValue = JsonConvert.DeserializeObject<CityModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new City to the database
                            await _cityService.Add(newValue);
                        }
                        // Process Update operation for City
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated City values
                            var newValue = JsonConvert.DeserializeObject<CityModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing City in the database
                            await _cityService.Update(newValue);
                        }
                        // Process Delete operation for City
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the City to be deleted
                            var newValue = JsonConvert.DeserializeObject<CityModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the City ID from the deserialized object
                            var dtId = newValue.CityId;
                            // Call the service to delete the City from the database
                            await _cityService.Delete(dtId);
                        }
                        // Exit the City case
                        break;

                    // Handle operations for the Condition entity
                    case nameof(Condition):
                        // Process Add operation for Condition
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Condition values
                            var newValue = JsonConvert.DeserializeObject<ConditionModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new Condition to the database
                            await _conditionService.Add(newValue);
                        }
                        // Process Update operation for Condition
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Condition values
                            var newValue = JsonConvert.DeserializeObject<ConditionModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing Condition in the database
                            await _conditionService.Update(newValue);
                        }
                        // Process Delete operation for Condition
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Condition to be deleted
                            var newValue = JsonConvert.DeserializeObject<ConditionModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the Condition ID from the deserialized object
                            var dtId = newValue.ConditionId;
                            // Call the service to delete the Condition from the database
                            await _conditionService.Delete(dtId);
                        }
                        // Exit the Condition case
                        break;

                    // Handle operations for the Country entity
                    case nameof(Country):
                        // Process Add operation for Country
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Country values
                            var newValue = JsonConvert.DeserializeObject<CountryModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new Country to the database
                            await _countryService.Add(newValue);
                        }
                        // Process Update operation for Country
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Country values
                            var newValue = JsonConvert.DeserializeObject<CountryModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing Country in the database
                            await _countryService.Update(newValue);
                        }
                        // Process Delete operation for Country
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Country to be deleted
                            var newValue = JsonConvert.DeserializeObject<CountryModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the Country ID from the deserialized object
                            var dtId = newValue.CountryId;
                            // Call the service to delete the Country from the database
                            await _countryService.Delete(dtId);
                        }
                        // Exit the Country case
                        break;

                    // Handle operations for the Currency entity
                    case nameof(Currency):
                        // Process Add operation for Currency
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Currency values
                            var newValue = JsonConvert.DeserializeObject<CurrencyModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new Currency to the database
                            await _currencyService.Add(newValue);
                        }
                        // Process Update operation for Currency
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Currency values
                            var newValue = JsonConvert.DeserializeObject<CurrencyModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing Currency in the database
                            await _currencyService.Update(newValue);
                        }
                        // Process Delete operation for Currency
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Currency to be deleted
                            var newValue = JsonConvert.DeserializeObject<CurrencyModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the Currency ID from the deserialized object
                            var dtId = newValue.CurrencyId;
                            // Call the service to delete the Currency from the database
                            await _currencyService.Delete(dtId);
                        }
                        // Exit the Currency case
                        break;

                    // Handle operations for the DataType entity
                    case nameof(DataType):
                        // Process Add operation for DataType
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new DataType values
                            var newValue = JsonConvert.DeserializeObject<DataTypeModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new DataType to the database
                            await _dataTypeService.Add(newValue);
                        }
                        // Process Update operation for DataType
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated DataType values
                            var newValue = JsonConvert.DeserializeObject<DataTypeModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing DataType in the database
                            await _dataTypeService.Update(newValue);
                        }
                        // Process Delete operation for DataType
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the DataType to be deleted
                            var newValue = JsonConvert.DeserializeObject<DataTypeModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the DataType ID from the deserialized object
                            var dtId = newValue.DataTypeId;
                            // Call the service to delete the DataType from the database
                            await _dataTypeService.Delete(dtId);
                        }
                        // Exit the DataType case
                        break;

                    // Handle operations for the Ecard entity
                    case nameof(Ecard):
                        // Process Add operation for Ecard
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Ecard values
                            var newValue = JsonConvert.DeserializeObject<EcardAddUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the new Ecard record
                            newValue.TenantId = entityId;
                            // Call the service to add the new Ecard to the database
                            await _ecardService.Add(entityId, newValue);
                        }
                        // Process Update operation for Ecard
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Ecard values
                            var newValue = JsonConvert.DeserializeObject<EcardUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the Ecard update
                            newValue.TenantId = entityId;
                            // Call the service to update the existing Ecard in the database
                            await _ecardService.Update(newValue);
                        }
                        // Process Delete operation for Ecard
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Ecard to be deleted
                            var newValue = JsonConvert.DeserializeObject<EcardModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the Ecard ID from the deserialized object
                            var dtId = newValue.EcardId;
                            // Call the service to delete the Ecard from the database
                            await _ecardService.Delete(entityId, dtId);
                        }
                        // Exit the Ecard case
                        break;

                    // Handle operations for the Entity entity
                    //case nameof(Entity):
                    //    // Process Add operation for Entity
                    //    if (model.ActionName == "Add")
                    //    {
                    //        // Deserialize the JSON data containing new Entity values
                    //        var newValue = JsonConvert.DeserializeObject<CreateOrUpdateEntityModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                    //        await _entityService.Add(newValue);
                    //    }
                    //    // Process Update operation for Entity
                    //    else if (model.ActionName == "Update")
                    //    {
                    //        // Deserialize the JSON data containing updated Entity values
                    //        var newValue = JsonConvert.DeserializeObject<CreateOrUpdateEntityModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                    //        // Call the service to update the existing Entity in the database
                    //        await _entityService.Update(newValue);
                    //    }
                    //    // Process Delete operation for Entity
                    //    else if (model.ActionName == "Delete")
                    //    {
                    //        // Deserialize the JSON data containing the Entity to be deleted
                    //        var newValue = JsonConvert.DeserializeObject<EntityModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                    //        // Extract the Entity ID from the deserialized object
                    //        var dtId = newValue.TenantId;
                    //        // Call the service to delete the Entity from the database
                    //        await _entityService.Delete(dtId);
                    //    }
                    //    // Exit the Entity case
                    //    break;

                    // Handle operations for the Erule entity
                    case nameof(Erule):
                        // Process Add operation for Erule
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Erule values
                            var newValue = JsonConvert.DeserializeObject<EruleCreateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the new Erule record
                            newValue.TenantId = entityId;
                            // Call the service to create the new Erule in the database
                            await _eruleService.Create(newValue);
                        }
                        // Process Update operation for Erule
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Erule values
                            var newValue = JsonConvert.DeserializeObject<EruleUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the Erule update
                            newValue.TenantId = entityId;
                            // Call the service to update the existing Erule in the database
                            await _eruleService.Update(newValue);
                        }
                        // Process Delete operation for Erule
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Erule to be deleted
                            var newValue = JsonConvert.DeserializeObject<EruleModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the Erule ID from the deserialized object
                            var dtId = newValue.EruleId;
                            // Call the service to delete the Erule from the database
                            await _eruleService.Delete(entityId, dtId);
                        }
                        // Exit the Erule case
                        break;

                    // Handle operations for the HistoryEc entity
                    case nameof(HistoryEc):
                        // Process Add operation for HistoryEc (only Add operation is supported)
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new HistoryEc values
                            var newValue = JsonConvert.DeserializeObject<HistoryEcModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new HistoryEc to the database
                            await _historyEcService.Add(newValue);
                        }
                        // Exit the HistoryEc case
                        break;

                    // Handle operations for the HistoryEr entity
                    case nameof(HistoryEr):
                        // Process Add operation for HistoryEr
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new HistoryEr values
                            var newValue = JsonConvert.DeserializeObject<HistoryErModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new HistoryEr to the database
                            await _historyErService.Add(newValue);
                        }
                        // Process Update operation for HistoryEr
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated HistoryEr values
                            var newValue = JsonConvert.DeserializeObject<HistoryErModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing HistoryEr in the database
                            await _historyErService.Update(newValue);
                        }
                        // Process Delete operation for HistoryEr
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the HistoryEr to be deleted
                            var newValue = JsonConvert.DeserializeObject<HistoryErModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the sequence ID from the deserialized object
                            var dtId = newValue.Seq;
                            // Call the service to delete the HistoryEr from the database
                            await _historyErService.Delete(dtId);
                        }
                        // Exit the HistoryEr case
                        break;

                    // Handle operations for the HistoryParameter entity
                    case nameof(HistoryParameter):
                        // Process Add operation for HistoryParameter
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new HistoryParameter values
                            var newValue = JsonConvert.DeserializeObject<HistoryParameterModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new HistoryParameter to the database
                            await _historyParameterService.Add(newValue);
                        }
                        // Process Update operation for HistoryParameter
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated HistoryParameter values
                            var newValue = JsonConvert.DeserializeObject<HistoryParameterModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing HistoryParameter in the database
                            await _historyParameterService.Update(newValue);
                        }
                        // Process Delete operation for HistoryParameter
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the HistoryParameter to be deleted
                            var newValue = JsonConvert.DeserializeObject<HistoryParameterModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the sequence ID from the deserialized object
                            var dtId = newValue.Seq;
                            // Call the service to delete the HistoryParameter from the database
                            await _historyParameterService.Delete(dtId);
                        }
                        // Exit the HistoryParameter case
                        break;

                    // Handle operations for the HistoryPc entity
                    case nameof(HistoryPc):
                        // Process Add operation for HistoryPc
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new HistoryPc values
                            var newValue = JsonConvert.DeserializeObject<HistoryPcModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the new HistoryPc record
                            newValue.TenantId = entityId;
                            // Call the service to add the new HistoryPc to the database
                            await _historyPcService.Add(newValue);
                        }
                        // Process Update operation for HistoryPc
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated HistoryPc values
                            var newValue = JsonConvert.DeserializeObject<HistoryPcModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the HistoryPc update
                            newValue.TenantId = entityId;
                            // Call the service to update the existing HistoryPc in the database
                            await _historyPcService.Update(newValue);
                        }
                        // Process Delete operation for HistoryPc
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the HistoryPc to be deleted
                            var newValue = JsonConvert.DeserializeObject<HistoryPcModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the transaction ID from the deserialized object
                            var dtId = newValue.TranId;
                            // Call the service to delete the HistoryPc from the database
                            await _historyPcService.Delete(entityId, dtId);
                        }
                        // Exit the HistoryPc case
                        break;

                    // Handle operations for the ListItem entity
                    case nameof(ListItem):
                        // Process Add operation for ListItem
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new ListItem values
                            var newValue = JsonConvert.DeserializeObject<ListItemCreateUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new ListItem to the database
                            await _listItemService.Add(newValue);
                        }
                        // Process Update operation for ListItem
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated ListItem values
                            var newValue = JsonConvert.DeserializeObject<ListItemCreateUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing ListItem in the database
                            await _listItemService.Update(newValue);
                        }
                        // Process Delete operation for ListItem
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the ListItem to be deleted
                            var newValue = JsonConvert.DeserializeObject<ListItemModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the item ID from the deserialized object
                            var dtId = newValue.ItemId;
                            // Call the service to delete the ListItem from the database
                            await _listItemService.Delete(dtId);
                        }
                        // Exit the ListItem case
                        break;

                    // Handle operations for the ManagedList entity
                    case nameof(ManagedList):
                        // Process Add operation for ManagedList
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new ManagedList values
                            var newValue = JsonConvert.DeserializeObject<ManagedListAddUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the new ManagedList record
                            newValue.TenantId = entityId;
                            // Call the service to add the new ManagedList to the database
                            await _managedListService.Add(newValue);
                        }
                        // Process Update operation for ManagedList
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated ManagedList values
                            var newValue = JsonConvert.DeserializeObject<ManagedListUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the ManagedList update
                            newValue.TenantId = entityId;
                            // Call the service to update the existing ManagedList in the database
                            await _managedListService.Update(newValue);
                        }
                        // Process Delete operation for ManagedList
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the ManagedList to be deleted
                            var newValue = JsonConvert.DeserializeObject<ManagedListModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the list ID from the deserialized object
                            var dtId = newValue.ListId;
                            // Call the service to delete the ManagedList from the database
                            await _managedListService.Delete(entityId, dtId);
                        }
                        // Exit the ManagedList case
                        break;

                    // Handle operations for the MappingFunction entity
                    case nameof(MappingFunction):
                        // Process Add operation for MappingFunction
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new MappingFunction values
                            var newValue = JsonConvert.DeserializeObject<MappingFunctionModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new MappingFunction to the database
                            await _mappingfunctionService.Add(newValue);
                        }
                        // Process Update operation for MappingFunction
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated MappingFunction values
                            var newValue = JsonConvert.DeserializeObject<MappingFunctionModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing MappingFunction in the database
                            await _mappingfunctionService.Update(newValue);
                        }
                        // Process Delete operation for MappingFunction
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the MappingFunction to be deleted
                            var newValue = JsonConvert.DeserializeObject<MappingFunctionModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the mapping function ID from the deserialized object
                            var dtId = newValue.MapFunctionId;
                            // Call the service to delete the MappingFunction from the database
                            await _mappingfunctionService.Delete(dtId);
                        }
                        // Exit the MappingFunction case
                        break;

                    // Handle operations for the NodeApi entity
                    case nameof(NodeApi):
                        // Process Add operation for NodeApi
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new NodeApi values
                            var newValue = JsonConvert.DeserializeObject<NodeApiCreateOrUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new NodeApi to the database
                            await _nodeApiService.Add(newValue);
                        }
                        // Process Update operation for NodeApi
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated NodeApi values
                            var newValue = JsonConvert.DeserializeObject<NodeApiCreateOrUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing NodeApi in the database
                            await _nodeApiService.Update(newValue);
                        }
                        // Process Delete operation for NodeApi
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the NodeApi to be deleted
                            var newValue = JsonConvert.DeserializeObject<NodeApiModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the API ID from the deserialized object
                            var dtId = newValue.Apiid;
                            // Call the service to delete the NodeApi from the database
                            await _nodeApiService.Delete(dtId);
                        }
                        // Exit the NodeApi case
                        break;

                    // Handle operations for the Node entity
                    case nameof(Node):
                        // Process Add operation for Node
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Node values
                            var newValue = JsonConvert.DeserializeObject<NodeCreateUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the new Node record
                            newValue.TenantId = entityId;
                            // Call the service to add the new Node to the database
                            await _nodeService.Add(newValue);
                        }
                        // Process Update operation for Node
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Node values
                            var newValue = JsonConvert.DeserializeObject<NodeCreateUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the Node update
                            newValue.TenantId = entityId;
                            // Call the service to update the existing Node in the database
                            await _nodeService.Update(newValue);
                        }
                        // Process Delete operation for Node
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Node to be deleted
                            var newValue = JsonConvert.DeserializeObject<NodeModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the Node ID from the deserialized object
                            var dtId = newValue.NodeId;
                            // Call the service to delete the Node from the database
                            await _nodeService.Delete(entityId, dtId);
                        }
                        // Exit the Node case
                        break;

                    // Process operations for the ParameterMap entity
                    case nameof(ParameterMap):
                        // Process Add operation for ParameterMap
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new ParameterMap values
                            var newValue = JsonConvert.DeserializeObject<ParamtersMapModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new ParameterMap to the database
                            await _paramtersMapService.Add(newValue);
                        }
                        // Process Update operation for ParameterMap
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated ParameterMap values
                            var newValue = JsonConvert.DeserializeObject<ParamtersMapModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing ParameterMap in the database
                            await _paramtersMapService.Update(newValue);
                        }
                        // Process Delete operation for ParameterMap
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the ParameterMap to be deleted
                            var newValue = JsonConvert.DeserializeObject<ParamtersMapModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the API ID, Node ID, and Parameter ID from the deserialized object
                            var apiId = newValue.Apiid;
                            var nodeId = newValue.NodeId;
                            var parameterId = newValue.ParameterId;
                            // Call the service to delete the ParameterMap from the database
                            await _paramtersMapService.Delete(apiId, nodeId, parameterId);
                        }
                        // Exit the ParameterMap case
                        break;

                    // Process operations for the Pcard entity
                    case nameof(Pcard):
                        // Process Add operation for Pcard
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Pcard values
                            var newValue = JsonConvert.DeserializeObject<PcardAddUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the new Pcard record
                            newValue.TenantId = entityId;
                            // Call the service to add the new Pcard to the database
                            await _pcardService.Add(newValue);
                        }
                        // Process Update operation for Pcard
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Pcard values
                            var newValue = JsonConvert.DeserializeObject<PcardUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the Pcard update
                            newValue.TenantId = entityId;
                            // Call the service to update the existing Pcard in the database
                            await _pcardService.Update(newValue);
                        }
                        // Process Delete operation for Pcard
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Pcard to be deleted
                            var newValue = JsonConvert.DeserializeObject<PcardModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the Pcard ID from the deserialized object
                            var dtId = newValue.PcardId;
                            // Call the service to delete the Pcard from the database
                            await _pcardService.Delete(entityId, dtId);
                        }
                        // Exit the Pcard case
                        break;

                    // Process operations for the Product entity
                    case nameof(Product):
                        // Process Add operation for Product
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new Product values
                            var newValue = JsonConvert.DeserializeObject<ProductAddUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the new Product record
                            newValue.TenantId = entityId;
                            // Call the service to add the new Product to the database
                            await _productService.Add(newValue);
                        }
                        // Process Update operation for Product
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated Product values
                            var newValue = JsonConvert.DeserializeObject<ProductAddUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Set the entity ID for the Product update
                            newValue.TenantId = entityId;
                            // Call the service to update the existing Product in the database
                            await _productService.Update(newValue);
                        }
                        // Process Delete operation for Product
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the Product to be deleted
                            var newValue = JsonConvert.DeserializeObject<ProductModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Extract the Product ID from the deserialized object
                            var dtId = newValue.ProductId;
                            // Call the service to delete the Product from the database
                            await _productService.Delete(entityId, dtId);
                        }
                        // Exit the Product case
                        break;

                    // Process operations for the ProductParam entity
                    case nameof(ProductParam):
                        // Process Add operation for ProductParam
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new ProductParam values
                            var newValue = JsonConvert.DeserializeObject<ProductParamAddUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            newValue.TenantId = entityId;
                            // Call the service to add the new ProductParam to the database
                            await _productParamservice.Add(newValue);
                        }
                        // Process Update operation for ProductParam
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated ProductParam values
                            var newValue = JsonConvert.DeserializeObject<ProductParamAddUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            newValue.TenantId = entityId;
                            // Call the service to update the existing ProductParam in the database
                            await _productParamservice.Update(newValue);
                        }
                        // Process Delete operation for ProductParam
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the ProductParam to be deleted
                            var newValue = JsonConvert.DeserializeObject<ProductParamModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            var dtId = newValue.ProductId;
                            var parameterId = newValue.ParameterId;
                            // Call the service to delete the ProductParam from the database
                            await _productParamservice.Delete(dtId, parameterId, entityId);
                        }
                        // Exit the ProductParam case
                        break;

                    // Process operations for the User entity
                    case nameof(User):
                        // Process Add operation for User
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new User values
                            var newValue = JsonConvert.DeserializeObject<UserAddModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            newValue.TenantId = entityId;
                            // Call the service to add the new User to the database with profile file
                            await _userService.Add(newValue.UserProfileFile, newValue, newValue.UserName);
                        }
                        // Process Update operation for User
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated User values
                            var newValue = JsonConvert.DeserializeObject<UserEditModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            newValue.TenantId = entityId;
                            // Call the service to update the existing User in the database with profile file
                            await _userService.Update(newValue.UserProfileFile, newValue, newValue.UserName);
                        }
                        // Process Delete operation for User
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the User to be deleted
                            var newValue = JsonConvert.DeserializeObject<UserGetModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            var dtId = newValue.UserId;
                            // Call the service to remove the User from the database
                            await _userService.Remove(entityId, dtId);
                        }
                        // Exit the User case
                        break;

                    // Process operations for the ExceptionManagement entity
                    case nameof(ExceptionManagement):
                        // Process Add operation for ExceptionManagement
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new ExceptionManagement values
                            var newValue = JsonConvert.DeserializeObject<ExceptionManagementCreateOrUpdateModel>(model.NewValueJson);
                            // Parse the JSON to extract ExceptionProducts array
                            var jObject = JObject.Parse(model.NewValueJson);
                            var exceptionProductsToken = jObject["ExceptionProducts"];
                            if (newValue == null)
                            {
                                throw new Exception("Invalid JSON data");
                            }
                            // Check if ExceptionProducts exists and is an array
                            if (exceptionProductsToken != null && exceptionProductsToken.Type == JTokenType.Array)
                            {
                                // Extract Product IDs from the ExceptionProducts array
                                var productIds = exceptionProductsToken
                                    .Select(x => (int?)x["ProductId"]) // Safely cast to int?
                                    .Where(id => id.HasValue)
                                    .Select(id => id!.Value)
                                    .ToList();

                                // Assign the extracted product IDs to the model
                                newValue.ProductId = productIds;
                            }
                            // Call the service to add the new ExceptionManagement to the database
                            await _exceptionManagement.Add(entityId, newValue);
                        }
                        // Process Update operation for ExceptionManagement
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated ExceptionManagement values
                            var newValue = JsonConvert.DeserializeObject<ExceptionManagementCreateOrUpdateModel>(model.NewValueJson);
                            // Parse the JSON to extract ExceptionProducts array
                            var jObject = JObject.Parse(model.NewValueJson);
                            var exceptionProductsToken = jObject["ExceptionProducts"];
                            if (newValue == null)
                            {
                                throw new Exception("Invalid JSON data");
                            }
                            // Check if ExceptionProducts exists and is an array
                            if (exceptionProductsToken != null && exceptionProductsToken.Type == JTokenType.Array)
                            {
                                // Extract Product IDs from the ExceptionProducts array
                                var productIds = exceptionProductsToken
                                    .Select(x => (int?)x["ProductId"]) // Safely cast to int?
                                    .Where(id => id.HasValue)
                                    .Select(id => id!.Value)
                                    .ToList();

                                // Assign the extracted product IDs to the model
                                newValue.ProductId = productIds;
                            }
                            // Call the service to update the existing ExceptionManagement in the database
                            await _exceptionManagement.Update(entityId, newValue);
                        }
                        // Process Delete operation for ExceptionManagement
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the ExceptionManagement to be deleted
                            var newValue = JsonConvert.DeserializeObject<ExceptionManagementModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to delete the ExceptionManagement from the database
                            await _exceptionManagement.Delete(entityId, newValue.ExceptionManagementId);
                        }
                        // Exit the ExceptionManagement case
                        break;

                    // Process operations for the ProductCap entity
                    case nameof(ProductCap):
                        // Process Add operation for ProductCap
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new ProductCap values
                            var newValue = JsonConvert.DeserializeObject<ProductCapModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new ProductCap to the database
                            await _productCapService.Add(newValue);
                        }
                        // Process Update operation for ProductCap
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated ProductCap values
                            var newValue = JsonConvert.DeserializeObject<ProductCapModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing ProductCap in the database
                            await _productCapService.Update(newValue);
                        }
                        // Process Delete operation for ProductCap
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the ProductCap to be deleted
                            var newValue = JsonConvert.DeserializeObject<ProductCapModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to delete the ProductCap from the database
                            await _productCapService.Delete(newValue.Id);
                        }
                        // Exit the ProductCap case
                        break;

                    // Process operations for the ProductCapAmount entity
                    case nameof(ProductCapAmount):
                        // Process Add operation for ProductCapAmount
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new ProductCapAmount values
                            var newValue = JsonConvert.DeserializeObject<ProductCapAmountAddModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new ProductCapAmount to the database
                            await _productCapAmountService.Add(newValue);
                        }
                        // Process Update operation for ProductCapAmount
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated ProductCapAmount values
                            var newValue = JsonConvert.DeserializeObject<ProductCapAmountUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing ProductCapAmount in the database
                            await _productCapAmountService.Update(newValue);
                        }
                        // Process Delete operation for ProductCapAmount
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the ProductCapAmount to be deleted
                            var newValue = JsonConvert.DeserializeObject<ProductCapAmountModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to delete the ProductCapAmount from the database
                            await _productCapAmountService.Delete(newValue.Id);
                        }
                        // Exit the ProductCapAmount case
                        break;

                    // Process operations for the EruleMaster entity
                    case nameof(EruleMaster):
                        // Process Add operation for EruleMaster
                        if (model.ActionName == "Add")
                        {
                            // Parse the JSON string for transformation
                            string json = model.NewValueJson;
                            var jObj = JObject.Parse(json);

                            // Manually map the keys to transform the JSON structure
                            var transformed = new JObject
                            {
                                ["EruleId"] = jObj["Id"],
                                ["EruleName"] = jObj["EruleName"],
                                ["Description"] = jObj["EruleDesc"],
                                ["TenantId"] = jObj["TenantId"],
                                ["IsActive"] = jObj["IsActive"],
                                ["CreatedBy"] = jObj["CreatedBy"],
                                //["UpdatedBy"] = jObj["UpdatedBy"]
                            };

                            // Convert the transformed JSON to EruleMasterCreateUpdateModel
                            var newValue = transformed.ToObject<EruleMasterCreateUpodateModel>() ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new EruleMaster to the database
                            await _eruleMasterService.Add(newValue, entityId);
                        }
                        // Process Update operation for EruleMaster
                        else if (model.ActionName == "Update")
                        {
                            // Parse the JSON string for transformation
                            string json = model.NewValueJson;
                            var jObj = JObject.Parse(json);

                            // Manually map the keys to transform the JSON structure
                            var transformed = new JObject
                            {
                                ["EruleId"] = jObj["Id"],
                                ["Name"] = jObj["EruleName"],
                                ["Description"] = jObj["EruleDesc"],
                                ["TenantId"] = jObj["TenantId"],
                                ["IsActive"] = jObj["IsActive"],
                                ["CreatedBy"] = jObj["CreatedBy"],
                                ["UpdatedBy"] = jObj["UpdatedBy"]
                            };

                            // Convert the transformed JSON to EruleMasterCreateUpdateModel
                            var newValue = transformed.ToObject<EruleMasterCreateUpodateModel>() ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing EruleMaster in the database
                            await _eruleMasterService.Edit(newValue, entityId);
                        }
                        // Exit the EruleMaster case
                        break;

                    // Process operations for the SecurityGroup entity
                    case nameof(SecurityGroup):
                        // Process Add operation for SecurityGroup
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new SecurityGroup values
                            var newValue = JsonConvert.DeserializeObject<SecurityGroupUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new SecurityGroup to the database
                            await _securityGroupService.Add(newValue);
                        }
                        // Process Update operation for SecurityGroup
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated SecurityGroup values
                            var newValue = JsonConvert.DeserializeObject<SecurityGroupUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing SecurityGroup in the database
                            await _securityGroupService.Update(newValue);
                        }
                        // Process Delete operation for SecurityGroup
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the SecurityGroup to be deleted
                            var newValue = JsonConvert.DeserializeObject<SecurityGroupModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to remove the SecurityGroup from the database
                            await _securityGroupService.Remove(newValue.GroupId);
                        }
                        // Exit the SecurityGroup case
                        break;

                    // Process operations for the UserGroup entity
                    case nameof(UserGroup):
                        // Process Add operation for UserGroup
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new UserGroup values
                            var newValue = JsonConvert.DeserializeObject<UserGroupCreateUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new UserGroup to the database
                            await _userGroupService.Add(newValue);
                        }
                        // Process Delete operation for UserGroup
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the UserGroup to be deleted
                            var newValue = JsonConvert.DeserializeObject<UserGroupModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to remove the UserGroup from the database
                            await _userGroupService.RemoveUserGroup(newValue.UserId, newValue.GroupId);
                        }
                        // Exit the UserGroup case
                        break;
                    case nameof(ApiParameter):
                        // Process Add operation for UserGroup
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new UserGroup values
                            var newValue = JsonConvert.DeserializeObject<ApiParametersCreateUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new UserGroup to the database
                            await _apiParametersService.Add(newValue);
                        }
                        // Process Delete operation for UserGroup
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the UserGroup to be deleted
                            var newValue = JsonConvert.DeserializeObject<ApiParametersCreateUpdateModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to remove the UserGroup from the database
                            await _apiParametersService.Remove(newValue.ApiParamterId);
                        }
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated SecurityGroup values
                            var newValue = JsonConvert.DeserializeObject<ApiParametersCreateUpdateModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing SecurityGroup in the database
                            await _apiParametersService.Update(newValue);
                        }
                        // Exit the UserGroup case
                        break;
                    case nameof(ApiParameterMap):
                        // Process Add operation for UserGroup
                        if (model.ActionName == "Add")
                        {
                            // Deserialize the JSON data containing new UserGroup values
                            var newValue = JsonConvert.DeserializeObject<ApiParameterCreateUpdateMapModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to add the new UserGroup to the database
                            await _apiParameterMapservice.Add(newValue);
                        }
                        // Process Delete operation for UserGroup
                        else if (model.ActionName == "Delete")
                        {
                            // Deserialize the JSON data containing the UserGroup to be deleted
                            var newValue = JsonConvert.DeserializeObject<ApiParameterCreateUpdateMapModel>(model.OldValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to remove the UserGroup from the database
                            await _apiParameterMapservice.Remove(newValue.Id);
                        }
                        else if (model.ActionName == "Update")
                        {
                            // Deserialize the JSON data containing updated SecurityGroup values
                            var newValue = JsonConvert.DeserializeObject<ApiParameterCreateUpdateMapModel>(model.NewValueJson) ?? throw new Exception("Invalid JSON data");
                            // Call the service to update the existing SecurityGroup in the database
                            await _apiParameterMapservice.Update(newValue);
                        }
                        // Exit the UserGroup case
                        break;
                    // Handle unsupported table names
                    default:
                        throw new ArgumentException($"Unsupported table name: {model.TableName}");
                }
            }
            // Complete the unit of work to persist all changes to the database
            await _uow.CompleteAsync();
        }
    }
}
