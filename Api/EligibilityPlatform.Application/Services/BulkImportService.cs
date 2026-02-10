using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for handling bulk import operations from Excel files.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BulkImportService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="countryService">The country service.</param>
    /// <param name="cityService">The city service.</param>
    /// <param name="entityService">The entity service.</param>
    /// <param name="eruleService">The erule service.</param>
    /// <param name="parameterService">The parameter service.</param>
    /// <param name="conditionService">The condition service.</param>
    /// <param name="factorService">The factor service.</param>
    /// <param name="managedListService">The managed list service.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="ecardService">The ecard service.</param>
    /// <param name="productService">The product service.</param>
    /// <param name="categoryService">The category service.</param>
    public partial class BulkImportService(IUnitOfWork uow, IMapper mapper, ICountryService countryService, ICityService cityService/*, IEntityService entityService*/, IEruleService eruleService, IParameterService parameterService, IConditionService conditionService, IFactorService factorService, IManagedListService managedListService, IDataTypeService dataTypeService, IEcardService ecardService, IProductService productService, ICategoryService categoryService) : IBulkImportService
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
        /// The country service instance for country-related operations.
        /// </summary>
        private readonly ICountryService _countryService = countryService;

        /// <summary>
        /// The city service instance for city-related operations.
        /// </summary>
        private readonly ICityService _cityService = cityService;

        /// <summary>
        /// The entity service instance for entity-related operations.
        /// </summary>
        //private readonly IEntityService _entityService = entityService;

        /// <summary>
        /// The erule service instance for eligibility rule operations.
        /// </summary>
        private readonly IEruleService _eruleService = eruleService;

        /// <summary>
        /// The parameter service instance for parameter operations.
        /// </summary>
        private readonly IParameterService _parameterService = parameterService;

        /// <summary>
        /// The condition service instance for condition operations.
        /// </summary>
        private readonly IConditionService _conditionService = conditionService;

        /// <summary>
        /// The factor service instance for factor operations.
        /// </summary>
        private readonly IFactorService _factorService = factorService;

        /// <summary>
        /// The managed list service instance for managed list operations.
        /// </summary>
        private readonly IManagedListService _managedListService = managedListService;

        /// <summary>
        /// The data type service instance for data type operations.
        /// </summary>
        private readonly IDataTypeService _dataTypeService = dataTypeService;

        /// <summary>
        /// The ecard service instance for eligibility card operations.
        /// </summary>
        private readonly IEcardService _ecardService = ecardService;

        /// <summary>
        /// The product service instance for product operations.
        /// </summary>
        private readonly IProductService _productService = productService;

        /// <summary>
        /// The category service instance for category operations.
        /// </summary>
        private readonly ICategoryService _categoryService = categoryService;

        /// <summary>
        /// Counter for successfully imported records.
        /// </summary>
        public int successCount = 0;

        /// <summary>
        /// Counter for failed import records.
        /// </summary>
        public int failureCount = 0;

        /// <summary>
        /// Counter for total records processed.
        /// </summary>
        public int totalRecords = 0;

        /// <summary>
        /// Symbol indicating required fields in import templates.
        /// </summary>
        public string Symbol = "* Fields marked with an asterisk are required.";

        /// <summary>
        /// Handles bulk import of data from an Excel file.
        /// </summary>
        /// <param name="fileStream">The file stream containing the Excel file.</param>
        /// <param name="fileName">The name of the uploaded file.</param>
        /// <param name="createdBy">The user who initiated the import.</param>
        /// <returns>A summary of the import results as a string.</returns>
        public async Task<string> BulkImport(Stream fileStream, string fileName, string createdBy, int tenantId)
        {
            // Sets the EPPlus license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new Excel package from the file stream
            using var package = new ExcelPackage(fileStream);
            // Creates a new import document record
            var importDocument = new ImportDocument
            {
                // Sets the document name to the uploaded file name
                Name = fileName,
                // Sets the import start time to current UTC time
                ImportTime = DateTime.UtcNow,
                // Sets the import end time to current UTC time (will be updated later)
                EndTime = DateTime.UtcNow,
                // Marks the import as not completed initially
                Completed = false,
                // Initializes total records count to 0
                TotalRecords = 0,
                // Initializes success count to 0
                SuccessCount = 0,
                // Initializes failure count to 0
                FailureCount = 0,
                // Stores the file data as byte array
                FileData = package.GetAsByteArray()
            };

            // Adds the import document to the repository
            // Resets the counters for the new import
            successCount = 0; failureCount = 0; totalRecords = 0;

            // List to store results from each worksheet
            var results = new List<string>();
            // Iterates through each worksheet in the Excel package
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                // Normalizes the sheet name by trimming and converting to lowercase
                string sheetName = worksheet.Name.Trim().ToLower();

                // Processes each sheet based on its name
                switch (sheetName)
                {
                    //case "entities":
                    //    results.Add(await ImportEntities(worksheet, createdBy));
                    //    break;
                    case "lists":
                        results.Add(await ImportList(worksheet, createdBy, tenantId));
                        break;
                    case "listitem":
                        results.Add(await ImportListIteams(worksheet, createdBy,tenantId));
                        break;
                    //case "customer-parameter":
                    //    results.Add(await ImportParameterCustomer( worksheet, 1, createdBy));
                    //    break;
                    case "parameter":
                        results.Add(await ImportParameterProduct(worksheet, 2, createdBy, tenantId));
                        break;
                    case "factors":
                        results.Add(await ImportFactor(worksheet, createdBy, tenantId));
                        break;
                    case "category":
                        results.Add(await ImportCategory(worksheet, createdBy,tenantId));
                        break;
                    case "stream":
                        results.Add(await ImportInfo(worksheet, createdBy, tenantId));
                        break;
                    case "details":
                        results.Add(await ImportDetails(worksheet, createdBy));
                        break;
                    case "erules":
                        results.Add(await ImportEruleMaster(worksheet, createdBy, tenantId));
                        break;
                    case "ecards":
                        results.Add(await ImportECard(worksheet, tenantId, createdBy));
                        break;
                    case "streamcards":
                        results.Add(await ImportPCard(worksheet, tenantId, createdBy));
                        break;
                    default:
                        results.Add($"Skipped unrecognized sheet: {sheetName}");
                        break;
                }
            }

            // Update import record after processing

            importDocument.Completed = true;

            importDocument.TotalRecords = totalRecords;
            importDocument.SuccessCount = successCount;
            importDocument.FailureCount = failureCount;
            importDocument.EndTime = DateTime.UtcNow;
            importDocument.CreatedBy = createdBy;
            if (totalRecords > 0)
            {
                importDocument = await _uow.ImportDocumentHistoryRepository.AddAsync(importDocument);

                await _uow.ImportDocumentHistoryRepository.UpdateAsync(importDocument);
            }
            var summary = string.Join(Environment.NewLine, results.Where(r => !string.IsNullOrWhiteSpace(r)));

            return string.IsNullOrWhiteSpace(summary) ? "No new records were inserted." : summary;
        }

        /// <summary>
        /// Updates the import statistics by tracking the total, successful, and failed records.
        /// </summary>
        /// <param name="rowCount">The total number of rows processed.</param>
        /// <param name="insertedRecordsCount">The number of successfully inserted records.</param>
        public void UpdateImportCounts(int rowCount, int insertedRecordsCount)
        {
            // Calculates the number of failed records
            int failedRecordsCount = rowCount - insertedRecordsCount;

            // Accumulates total records processed
            totalRecords += rowCount;
            // Accumulates successful records
            successCount += insertedRecordsCount;
            // Accumulates failed records
            failureCount += failedRecordsCount;
        }

        /// <summary>
        /// Retrieves the complete import history records from the database.
        /// </summary>
        /// <returns>A list of <see cref="ImportDocument"/> objects containing import history details.</returns>
        public List<ImportDocument> GetAllImportHistory()
        {
            // Retrieves all import history records from the repository
            return _uow.ImportDocumentHistoryRepository.GetAllImportHistory();
        }

        /// <summary>
        /// Imports entity data from an Excel worksheet and stores it in the database.
        /// </summary>
        /// <param name="worksheet">The worksheet containing entity data.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, returning a summary message of the import process.</returns>
        //public async Task<string> ImportEntities(ExcelWorksheet worksheet, string createdBy)
        //{
        //    // Retrieves all existing entities from the service
        //    List<EntityModel> entities = _entityService.GetAll();
        //    // Gets the number of data rows in the worksheet
        //    int rowCount = GetRowCount(worksheet);
        //    // List to store entity models from the Excel file
        //    var models = new List<EntityModel>();
        //    // Counter for skipped records
        //    int skippedRecordsCount = 0;
        //    // Counter for inserted records
        //    int insertedRecordsCount = 0;
        //    // Result message for the import operation
        //    var resultMessage = "";
        //    // Counter for duplicate records
        //    int dublicatedRecordsCount = 0;
        //    // Gets the last entity to determine the next code
        //    var lastEntity = _uow.EntityRepository.GetAll()
        //             .OrderByDescending(e => e.Code)
        //             .FirstOrDefault();
        //    // Parses the last code or defaults to 0 if not found
        //    string[] expectedHeaders = [
        //      "EntityName*",
        //      "CountryName*",
        //      "CountryId*",
        //      "CityName*",
        //      "CityId*",
        //      "Address*",
        //      "IsChild",
        //      "ParentEntity",
        //      "ParentTenantId",
        //      "Code*"];
        //    try
        //    {
        //        // Checks if the worksheet is empty
        //        if (rowCount == 0 || rowCount == -1)
        //        {
        //            return resultMessage = "Entity Page = Uploaded File Entities sheets Is Empty";
        //        }
        //        // Read data from the Excel file into models
        //        for (int row = 2; row <= rowCount + 1; row++)
        //        {
        //            try
        //            {
        //                var Code = worksheet.Cells[row, 10].Text;
        //                var entityName = worksheet.Cells[row, 1].Text;
        //                var countryIdText = worksheet.Cells[row, 3].Text;
        //                var cityIdText = worksheet.Cells[row, 5].Text;
        //                var address = worksheet.Cells[row, 6].Text;

        //                _ = int.TryParse(countryIdText, out int countryId);
        //                _ = int.TryParse(cityIdText, out int cityId);

        //                // if required fields missing → skip
        //                if (string.IsNullOrWhiteSpace(Code) ||
        //                    string.IsNullOrWhiteSpace(entityName) ||
        //                    countryId == 0 ||
        //                    cityId == 0 ||
        //                    string.IsNullOrWhiteSpace(address))
        //                {
        //                    skippedRecordsCount++;
        //                    continue;
        //                }

        //                var model = new EntityModel
        //                {
        //                    Code = Code,
        //                    EntityName = entityName,
        //                    CountryId = countryId,
        //                    CityId = cityId,
        //                    EntityAddress = address,
        //                    CreatedBy = createdBy,
        //                };

        //                models.Add(model);
        //            }
        //            catch
        //            {
        //                skippedRecordsCount++; // row crashed due to wrong format
        //                continue;
        //            }
        //        }
        //        static bool Same(string a, string b)
        //        {
        //            return string.Equals(
        //                a?.Replace(" ", "").Trim(),
        //                b?.Replace(" ", "").Trim(),
        //                StringComparison.OrdinalIgnoreCase
        //            );
        //        }
        //        for (int i = 0; i < expectedHeaders.Length; i++)
        //        {
        //            string excelHeader = worksheet.Cells[1, i + 1].Text;

        //            if (!Same(excelHeader, expectedHeaders[i]))
        //            {
        //                return $"Incorrect file format. Expected header '{expectedHeaders[i]}' in column {i + 1}.";
        //            }
        //        }
        //        // Filters out existing entities to get only new ones
        //        List<EntityModel> existingEntities = _entityService.GetAll();
        //        var newModels = models.Where(model =>
        //        !existingEntities.Any(existing => (existing.Code ?? "0") == model.Code)).ToList();

        //        // Returns message if no new records to insert
        //        if (newModels.Count == 0)
        //        {
        //            dublicatedRecordsCount++;
        //        }

        //        // Processes each model for insertion
        //        foreach (var model in models)
        //        {
        //            // Checks if the entity already exists
        //            var existingEntity = await _uow.EntityRepository.Query().AnyAsync(p => p.Code == model.Code || (p.EntityName == model.EntityName && p.CountryId == model.CountryId && p.CityId == model.CityId && p.EntityAddress == model.EntityAddress));
        //            if (existingEntity)
        //            {
        //                // Increments duplicate count if entity exists
        //                continue;
        //            }

        //            // Sets creation and update timestamps
        //            model.CreatedByDateTime = DateTime.UtcNow;
        //            model.UpdatedByDateTime = DateTime.UtcNow;
        //            model.UpdatedBy = createdBy;

        //            var entity = _mapper.Map<Entity>(model);

        //            // Adds entity to repository
        //            _uow.EntityRepository.Add(entity);
        //        }
        //        insertedRecordsCount = newModels.Count;
        //        // Updates import counts
        //        UpdateImportCounts(rowCount, insertedRecordsCount);

        //        // Commit the changes to the database
        //        await _uow.CompleteAsync();

        //        // Builds result message based on import results
        //        if (newModels.Count > 0)
        //        {
        //            resultMessage = $"Entity Page= {newModels.Count} Entities Inserted Successfully.{Environment.NewLine}";
        //        }
        //        if (skippedRecordsCount > 0)
        //        {
        //            resultMessage += $"Entity Page={skippedRecordsCount} records were not inserted because of missing required field.{Environment.NewLine}";
        //        }
        //        if (dublicatedRecordsCount > 0)
        //        {
        //            resultMessage += $"Entity Page= {dublicatedRecordsCount} record already exists.{Environment.NewLine}";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Returns error message if exception occurs
        //        resultMessage = "Error Entity Page = " + ex.Message + Environment.NewLine;
        //    }
        //    return resultMessage;
        //}

        /// <summary>
        /// Imports a list of managed entities from an Excel worksheet and stores them in the database.
        /// </summary>
        /// <param name="worksheet">The worksheet containing the list data.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, returning a summary message 
        /// about the number of records inserted, skipped, or marked as duplicates.
        /// </returns>
        public async Task<string> ImportList(ExcelWorksheet worksheet, string createdBy, int tenantId)
        {
            // Gets the number of data rows in the worksheet
            int rowCount = GetRowCount(worksheet);
            // List to store managed list models
            var models = new List<ManagedList>();
            // Counter for skipped records
            int skippedRecordsCount = 0;
            // Counter for duplicate records
            int dublicatedRecordsCount = 0;
            // Counter for inserted records
            int insertedRecordsCount = 0;
            // Result message for the import operation
            var resultMessage = "";

            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    return resultMessage = "List Page = Uploaded File Lists sheets Is Empty";
                }
                string[] expectedHeaders =
                     [
                        "ListName*"

                    ];

                // Helper: Case-insensitive and space-insensitive compare
                static bool Same(string a, string b)
                {
                    return string.Equals(
                        a?.Replace(" ", "").Trim(),
                        b?.Replace(" ", "").Trim(),
                        StringComparison.OrdinalIgnoreCase
                    );
                }

                // Validate Excel header row
                for (int i = 0; i < expectedHeaders.Length; i++)
                {
                    string excelHeader = worksheet.Cells[1, i + 1].Text;

                    if (!Same(excelHeader, expectedHeaders[i]))
                    {
                        return $"Incorrect file format at Column {i + 1}. Expected '{expectedHeaders[i]}'.";
                    }
                }
                // Read data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Reads list name from column 1
                    var ListName = worksheet.Cells[row, 1].Text;
                    // Reads entity ID from column 3

                    // Check for missing or invalid data
                    if (string.IsNullOrWhiteSpace(ListName))

                    {
                        // Increments skipped records count if required fields are missing
                        skippedRecordsCount++;
                        continue;
                    }

                    // Creates a new managed list model
                    var model = new ManagedList
                    {
                        // Sets the list name
                        ListName = ListName,
                        // Parses and sets entity ID
                        TenantId = tenantId,
                        // Sets the created by user
                        CreatedBy = createdBy,
                        UpdatedBy = createdBy
                    };
                    // Adds model to the list
                    models.Add(model);
                }

                // Processes each model for insertion
                foreach (var model in models)
                {
                    // Checks if the managed list already exists
                    var existingEntity = await _uow.ManagedListRepository.Query().AnyAsync(p => p.ListName == model.ListName && p.TenantId == model.TenantId);
                    if (existingEntity)
                    {
                        // Increments duplicate count if managed list exists
                        dublicatedRecordsCount++;
                        continue;
                    }

                    // Sets creation and update timestamps
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    model.CreatedByDateTime = DateTime.UtcNow;
                    // Adds managed list to repository
                    _uow.ManagedListRepository.Add(_mapper.Map<ManagedList>(model));
                    // Increments inserted records count
                    insertedRecordsCount++;
                }

                // Commits changes to the database
                await _uow.CompleteAsync();
                // Updates import counts
                UpdateImportCounts(rowCount, insertedRecordsCount);
                // Builds result message based on import results
                if (insertedRecordsCount > 0)
                {
                    resultMessage = $" List Page = {insertedRecordsCount} List Items Inserted Successfully.";
                }
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $" List Page = {skippedRecordsCount} records were not inserted because of missing required field.{Environment.NewLine}";
                }
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $" List Page = {dublicatedRecordsCount}  record already exists.{Environment.NewLine}";
                }
            }
            catch (Exception ex)
            {
                // Returns error message if exception occurs
                resultMessage += "Error On List Page = " + ex.Message + Environment.NewLine;
            }
            return resultMessage;
        }

        /// <summary>
        /// Imports list items from an Excel worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet containing the list item data.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, returning a summary message of the import process.</returns>
        public async Task<string> ImportListIteams(ExcelWorksheet worksheet, string createdBy,int  tenantId)
        {
            // Gets the number of data rows in the worksheet
            int rowCount = GetRowCount(worksheet);
            // List to store list item models
            var models = new List<ListItemModel>();
            // Counter for skipped records
            int skippedRecordsCount = 0;
            // Counter for duplicate records
            int dublicatedRecordsCount = 0;
            // Counter for inserted records
            int insertedRecordsCount = 0;
            // Result message for the import operation
            var resultMessage = "";

            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    return resultMessage = "ListItem Page = Uploaded File ListItem sheets Is Empty";
                }

                string[] expectedHeaders =
                     [
                      "ListName*",
                      "ItemName*",
                      "ListId*"
        ];

                static bool Same(string a, string b)
                {
                    return string.Equals(
                        a?.Replace(" ", "").Trim(),
                        b?.Replace(" ", "").Trim(),
                        StringComparison.OrdinalIgnoreCase
                    );
                }

                for (int i = 0; i < expectedHeaders.Length; i++)
                {
                    string excelHeader = worksheet.Cells[1, i + 1].Text;

                    if (!Same(excelHeader, expectedHeaders[i]))
                    {
                        return $"Incorrect file format at Column {i + 1}. " +
                               $"Expected '{expectedHeaders[i]}'.";
                    }
                }
                // Read data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Reads item name from column 2
                    var ItemName = worksheet.Cells[row, 2].Text;
                    // Reads list ID from column 3
                    var ListId = worksheet.Cells[row, 3].Text;

                    // Check for missing or invalid data
                    if (string.IsNullOrWhiteSpace(ItemName) || !int.TryParse(ListId, out _))
                    {
                        // Increments skipped records count if required fields are missing
                        skippedRecordsCount++;
                        continue;
                    }

                    // Creates a new list item model
                    var model = new ListItemModel
                    {
                        // Sets the item name
                        ItemName = ItemName,
                        // Parses and sets list ID
                        ListId = int.TryParse(ListId, out int listId) ? listId : 0,
                        // Sets the created by user
                        CreatedBy = createdBy,
                        UpdatedBy = createdBy,
                        TenantId=tenantId
                        
                    };
                    // Adds model to the list
                    models.Add(model);
                }

                // Processes each model for insertion
                foreach (var model in models)
                {
                    // Checks if the list item already exists
                    var existingEntity = await _uow.ListItemRepository.Query().AnyAsync(p => p.ItemName == model.ItemName && p.ListId == model.ListId);
                    if (existingEntity)
                    {
                        // Increments duplicate count if list item exists
                        dublicatedRecordsCount++;
                        continue;
                    }

                    // Sets list ID to null if 0
                    model.ListId = model.ListId == 0 ? null : model.ListId;
                    // Sets creation and update timestamps
                    model.CreatedByDateTime = DateTime.UtcNow;
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    
                    // Adds list item to repository
                    _uow.ListItemRepository.Add(_mapper.Map<ListItem>(model));
                    // Increments inserted records count
                    insertedRecordsCount++;
                }

                // Commits changes to the database
                await _uow.CompleteAsync();

                // Updates import counts
                UpdateImportCounts(rowCount, insertedRecordsCount);
                // Builds result message based on import results
                if (insertedRecordsCount > 0)
                {
                    resultMessage = $"List Iteam = {models.Count} List Items Inserted Successfully.";
                }
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $"List Iteam = {skippedRecordsCount} records were not inserted because of missing required field. {Environment.NewLine}";
                }
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $"List Iteam = {dublicatedRecordsCount} record already exists.{Environment.NewLine}";
                }
            }
            catch (Exception ex)
            {
                // Returns error message if exception occurs
                resultMessage = "Error On List Iteam page = " + ex.Message + Environment.NewLine;
            }
            return resultMessage;
        }

        /// <summary>
        /// Imports customer parameters from an Excel worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet containing the parameter data.</param>
        /// <param name="Identifier">The identifier for the parameter type.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, returning a summary message of the import process.</returns>
        public async Task<string> ImportParameterCustomer(ExcelWorksheet worksheet, int Identifier, string createdBy)
        {
            // Gets the number of data rows in the worksheet
            int rowCount = GetRowCount(worksheet);
            // List to store parameter models
            var models = new List<Parameter>();
            // Counter for skipped records
            int skippedRecordsCount = 0;
            // Counter for duplicate records
            int dublicatedRecordsCount = 0;
            // Counter for inserted records
            int insertedRecordsCount = 0;
            // Result message for the import operation
            var resultMessage = "";

            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    return resultMessage = " Cutomer Parameter = Uploaded File Customer-Parameter sheets Is Empty";
                }
                // Read data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Reads parameter name from column 1
                    var ParameterName = worksheet.Cells[row, 1].Text;
                    // Reads data type ID from column 3
                    var DataTypeId = worksheet.Cells[row, 3].Text;
                    // Reads has factors flag from column 4
                    var HasFactors = worksheet.Cells[row, 4].Text;
                    // Reads entity ID from column 10
                    var TenantId = worksheet.Cells[row, 10].Text;
                    // Reads condition ID from column 8
                    var ConditionId = worksheet.Cells[row, 8].Text;
                    // Reads factor order from column 6
                    var FactorOrder = worksheet.Cells[row, 6].Text;

                    // Check for missing or invalid data
                    if (string.IsNullOrWhiteSpace(ParameterName) || !int.TryParse(DataTypeId, out _) || string.IsNullOrWhiteSpace(HasFactors) || !int.TryParse(TenantId, out _))
                    {
                        // Increments skipped records count if required fields are missing
                        skippedRecordsCount++;
                        continue;
                    }
                    // Parses has factors flag
                    bool factors = bool.TryParse(HasFactors, out bool hasFactors) && hasFactors;
                    // Additional validation if factors are enabled
                    if (factors)
                    {
                        if (string.IsNullOrWhiteSpace(FactorOrder) || !int.TryParse(ConditionId, out _))
                        {
                            // Increments skipped records count if factor-related fields are missing
                            skippedRecordsCount++;
                            continue;
                        }
                    }

                    // Creates a new parameter model
                    var model = new Parameter
                    {
                        // Sets the parameter name
                        ParameterName = ParameterName,
                        // Parses and sets data type ID
                        DataTypeId = int.TryParse(DataTypeId, out int dataTypeId) ? dataTypeId : 0,
                        // Sets the has factors flag
                        HasFactors = factors,
                        // Sets the created by user
                        CreatedBy = createdBy,
                        // Sets the parameter identifier
                        Identifier = Identifier,
                        // Marks parameter as required
                        IsRequired = true,
                        // Parses and sets entity ID
                        TenantId = int.TryParse(TenantId, out int tenantId) ? tenantId : 0,
                        // Parses and sets condition ID (nullable)
                        ConditionId = int.TryParse(ConditionId, out int conditionId) ? conditionId : null,
                        // Sets the factor order
                        FactorOrder = FactorOrder,
                    };
                    // Adds model to the list
                    models.Add(model);
                }

                // Processes each model for insertion
                foreach (var model in models)
                {
                    // Checks if the parameter already exists
                    var existingEntity = await _uow.ParameterRepository.Query().AnyAsync(p => p.ParameterName == model.ParameterName && p.DataTypeId == model.DataTypeId && p.HasFactors == model.HasFactors && p.TenantId == model.TenantId && p.ConditionId == model.ConditionId && p.FactorOrder == model.FactorOrder);
                    if (existingEntity)
                    {
                        // Increments duplicate count if parameter exists
                        dublicatedRecordsCount++;
                        continue;
                    }

                    // Sets creation and update timestamps
                    model.CreatedByDateTime = DateTime.UtcNow;
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    // Maps model to parameter entity
                    var entity = _mapper.Map<Parameter>(model);
                    // Adds parameter to repository
                    _uow.ParameterRepository.Add(entity);
                    // Increments inserted records count
                    insertedRecordsCount++;
                }

                // Commits changes to the database
                await _uow.CompleteAsync();

                // Updates import counts
                UpdateImportCounts(rowCount, insertedRecordsCount);

                // Builds result message based on import results
                if (insertedRecordsCount > 0)
                {
                    resultMessage = $" Cutomer Parameter = {models.Count} Parameter Customer Inserted Successfully.";
                }
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $" Cutomer Parameter = {skippedRecordsCount} records were not inserted because of missing required field.";
                }
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $" Cutomer Parameter =  {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                // Returns error message if exception occurs
                resultMessage = "Error On Cutomer Parameter = " + ex.Message;
            }
            return resultMessage;
        }

        /// <summary>
        /// Imports product parameters from an Excel worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet containing the parameter data.</param>
        /// <param name="Identifier">The identifier for the parameter type.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, returning a summary message of the import process.</returns>
        public async Task<string> ImportParameterProduct(ExcelWorksheet worksheet, int Identifier, string createdBy, int tenantId)
        {
            // Gets the number of data rows in the worksheet
            int rowCount = GetRowCount(worksheet);
            // List to store parameter models
            var models = new List<Parameter>();
            // Counter for skipped records
            int skippedRecordsCount = 0;
            // Counter for duplicate records
            int dublicatedRecordsCount = 0;
            // Counter for inserted records
            int insertedRecordsCount = 0;
            // Result message for the import operation
            var resultMessage = "";
            string[] expectedHeaders =
[
             "ParameterName*",
              "ParameterType*",
              "ParameterTypeId*",

              "IsMandatory"
           ];

            static bool Same(string a, string b)
            {
                return string.Equals(
                    a?.Replace(" ", "").Trim(),
                    b?.Replace(" ", "").Trim(),
                    StringComparison.OrdinalIgnoreCase
                );
            }

            // Validate header row
            for (int i = 0; i < expectedHeaders.Length; i++)
            {
                string excelHeader = worksheet.Cells[1, i + 1].Text;

                if (!Same(excelHeader, expectedHeaders[i]))
                {
                    return $"Incorrect file format at Column {i + 1}. Expected header '{expectedHeaders[i]}'.";
                }
            }
            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    return resultMessage = " Product Parameter = Uploaded File Product-Parameter sheets Is Empty";
                }
                HashSet<string> excelDuplicateCheck = [];



                // Read data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Reads parameter name from column 1
                    var parameterName = worksheet.Cells[row, 1].Text?.Trim();
                    var dataTypeId = worksheet.Cells[row, 3].Text?.Trim();
                    var isMandatory = worksheet.Cells[row, 4].Text?.Trim();
                    string excelKey = $"{parameterName?.ToLower()}_{tenantId}";

                    if (excelDuplicateCheck.Contains(excelKey))
                    {
                        dublicatedRecordsCount++;
                        continue; // Skip duplicates from Excel itself
                    }

                    excelDuplicateCheck.Add(excelKey);
                    if (string.IsNullOrWhiteSpace(parameterName) || !int.TryParse(dataTypeId, out _))
                    {
                        // Increments skipped records count if required fields are missing
                        skippedRecordsCount++;
                        continue;
                    }
                    // Check for missing or invalid data

                    // Parses has factors flag
                    // Additional validation if factors are enabled


                    // Creates a new parameter model
                    var model = new Parameter
                    {
                        // Sets the parameter name
                        ParameterName = parameterName,
                        // Parses and sets data type ID
                        DataTypeId = int.TryParse(dataTypeId, out int dataTypeIds) ? dataTypeIds : 0,
                        // Sets the has factors flag
                        // Sets the parameter identifier
                        Identifier = Identifier,
                        // Sets the created by user
                        CreatedBy = createdBy,
                        // Parses and sets entity ID
                        TenantId = tenantId,
                        // Parses and sets condition ID (nullable)
                        IsRequired = bool.TryParse(isMandatory, out bool isRequired) && isRequired,
                    };
                    // Adds model to the list
                    models.Add(model);
                }

                // Processes each model for insertion
                foreach (var model in models)
                {
                    // Checks if the parameter already exists
                    var existingEntity = await _uow.ParameterRepository.Query().AnyAsync(p => p.ParameterName == model.ParameterName && p.TenantId == model.TenantId);
                    if (existingEntity)
                    {
                        // Increments duplicate count if parameter exists
                        dublicatedRecordsCount++;
                        continue;
                    }

                    // Sets creation and update timestamps
                    model.CreatedByDateTime = DateTime.UtcNow;
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    model.UpdatedBy = createdBy;                    // Maps model to parameter entity
                    var entity = _mapper.Map<Parameter>(model);
                    // Adds parameter to repository
                    _uow.ParameterRepository.Add(entity);
                    insertedRecordsCount++;
                }

                await _uow.CompleteAsync();

                UpdateImportCounts(rowCount, insertedRecordsCount);

                if (insertedRecordsCount > 0)
                {
                    resultMessage = $" Product Parameter = {insertedRecordsCount} Product Parameter Inserted Successfully.";
                }
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $" Product Parameter = {skippedRecordsCount} records were not inserted because of missing required field.";
                }
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $" Product Parameter = {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                resultMessage = "Error On Product Parameter = " + ex.Message;
            }

            return resultMessage;
        }
        /// <summary>
        /// Imports factors from an Excel worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet containing the factor data.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, returning a summary message of the import process.</returns>
        public async Task<string> ImportFactor(ExcelWorksheet worksheet, string createdBy, int tenantId)
        {
            // Gets the total number of rows in the worksheet
            int rowCount = GetRowCount(worksheet);
            // Initializes a list to store factor models
            var models = new List<Factor>();
            // Counter for skipped records due to validation errors
            int skippedRecordsCount = 0;
            // Counter for duplicate records found in the database
            int dublicatedRecordsCount = 0;
            // Counter for successfully inserted records
            int insertedRecordsCount = 0;
            // Stores the result message of the import operation
            var resultMessage = "";

            try
            {
                string[] requiredHeaders =
       [
            "factorName*",
            "parameter*",
            "parameterId*",
            "condition*",
            "conditionId*",
            "value1*",
            "value2"
       ];

                // Read header row
                var excelHeaders = new List<string>();
                for (int col = 1; col <= requiredHeaders.Length; col++)
                {
                    excelHeaders.Add(worksheet.Cells[1, col].Text?.Trim()!);
                }

                for (int i = 0; i < requiredHeaders.Length; i++)
                {
                    if (!excelHeaders[i].Equals(requiredHeaders[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Incorrect file format at Column {i + 1}. " +
                               $"Expected '{requiredHeaders[i]}'.";
                    }
                }
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    // Returns message indicating empty worksheet
                    return resultMessage = " Factor page = Uploaded File Factors sheets Is Empty";
                }
                // Read data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    var FactorName = worksheet.Cells[row, 1].Text;
                    var ParameterId = worksheet.Cells[row, 3].Text;
                    var ConditionId = worksheet.Cells[row, 5].Text;
                    var Value1 = worksheet.Cells[row, 6].Text;
                    var Value2 = worksheet.Cells[row, 7].Text;

                    // required field check
                    if (string.IsNullOrWhiteSpace(FactorName) ||
                        string.IsNullOrWhiteSpace(ParameterId) ||
                        string.IsNullOrWhiteSpace(ConditionId) ||
                         string.IsNullOrWhiteSpace(Value1))

                    {
                        skippedRecordsCount++;
                        continue;
                    }


                    // Handles special conditions (12 and 13) that use different value column
                    if (ConditionId == "12" || ConditionId == "13")
                    {
                        // Reads value from column 7 for special conditions
                        Value1 = worksheet.Cells[row, 7].Text;
                    }

                    // Creates new factor model instance
                    var model = new Factor
                    {
                        // Sets factor name from Excel data
                        FactorName = FactorName,
                        // Parses and sets parameter ID, defaults to 0 if invalid
                        ParameterId = int.TryParse(ParameterId, out int parameterId) ? parameterId : 0,
                        // Parses and sets condition ID, defaults to 0 if invalid
                        ConditionId = int.TryParse(ConditionId, out int conditionId) ? conditionId : 0,
                        // Sets primary value
                        Value1 = Value1,
                        // Sets secondary value (can be null/empty)
                        Value2 = Value2,
                        // Sets creator identifier
                        CreatedBy = createdBy,
                        // Sets default note value
                        Note = "string"
                    };
                    // Adds model to the collection
                    models.Add(model);
                }

                // Processes each model for database insertion
                foreach (var model in models)
                {
                    // Checks if identical record already exists in database
                    var existingEntity = await _uow.FactorRepository.Query().AnyAsync(p => p.FactorName == model.FactorName && p.ParameterId == model.ParameterId && p.ConditionId == model.ConditionId && p.ConditionId == model.ConditionId && p.Value1 == model.Value1);
                    if (existingEntity)
                    {
                        // Increments duplicate records counter
                        dublicatedRecordsCount++;
                        // Skips insertion for duplicate records
                        continue;
                    }
                    model.TenantId = tenantId;
                    // Sets creation timestamp to current UTC time
                    model.CreatedByDateTime = DateTime.UtcNow;
                    // Sets first update timestamp to current UTC time
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    // Maps and adds model to repository for insertion
                    _uow.FactorRepository.Add(_mapper.Map<Factor>(model));
                    // Increments inserted records counter
                    insertedRecordsCount++;
                }

                // Commits all changes to database
                await _uow.CompleteAsync();

                // Updates global import statistics
                UpdateImportCounts(rowCount, insertedRecordsCount);

                // Builds success message if records were inserted
                if (insertedRecordsCount > 0)
                {
                    // Sets success message with count
                    resultMessage = $" Factor page = {insertedRecordsCount} Factor Inserted Successfully.";
                }
                // Appends skipped records message if any were skipped
                if (skippedRecordsCount > 0)
                {
                    // Appends skipped records information to result message
                    resultMessage += $" Factor page = {skippedRecordsCount} records were not inserted because of missing required field.";
                }
                // Appends duplicate records message if any duplicates found
                if (dublicatedRecordsCount > 0)
                {
                    // Appends duplicate records information to result message
                    resultMessage += $" Factor page = {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                // Returns error message with exception details
                resultMessage = "Error On Factor page = " + ex.Message;
            }

            // Returns final result message
            return resultMessage;
        }

        /// <summary>
        /// Imports categories from an Excel worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet containing the category data.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, returning a summary message of the import process.</returns>
        public async Task<string> ImportCategory(ExcelWorksheet worksheet, string createdBy,int tenantId)
        {
            // Gets the total number of rows in the worksheet
            int rowCount = GetRowCount(worksheet);
            // Initializes a list to store category models
            var models = new List<Category>();
            // Counter for skipped records due to validation errors
            int skippedRecordsCount = 0;
            // Counter for duplicate records found in the database
            int dublicatedRecordsCount = 0;
            // Counter for successfully inserted records
            int insertedRecordsCount = 0;
            // Stores the result message of the import operation
            var resultMessage = "";
            string[] requiredHeaders =
[
    "CategoryName*",
    "CatDescription*",
    "EntityName*",
    "TenantId*"
];

            // Read header row from Excel
            var excelHeaders = new List<string>();
            for (int col = 1; col <= requiredHeaders.Length; col++)
            {
                excelHeaders.Add(worksheet.Cells[1, col].Text?.Trim()!);
            }

            // Validate each header
            for (int i = 0; i < requiredHeaders.Length; i++)
            {
                if (!excelHeaders[i].Equals(requiredHeaders[i], StringComparison.OrdinalIgnoreCase))
                {
                    return $"Incorrect file format at Column {i + 1}. " +
                           $"Expected '{requiredHeaders[i]}' but found '{excelHeaders[i]}'";
                }
            }
            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    // Returns message indicating empty worksheet
                    return resultMessage = " Category page = Uploaded File Category sheets Is Empty";
                }
                // Read data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Reads category name from column 1
                    var CategoryName = worksheet.Cells[row, 1].Text;
                    // Reads category description from column 2
                    var CatDescription = worksheet.Cells[row, 2].Text;
                    // Reads entity ID from column 4
                    //var TenantId = worksheet.Cells[row, 4].Text;

                    // Check for empty or invalid fields
                    if (string.IsNullOrWhiteSpace(CategoryName) || string.IsNullOrWhiteSpace(CatDescription) )
                    {
                        // Increments skipped records counter
                        skippedRecordsCount++;
                        // Skip the record if any required field is missing
                        continue;
                    }

                    // Creates new category model instance
                    var model = new Category
                    {
                        // Sets category name from Excel data
                        CategoryName = CategoryName,
                        // Sets category description from Excel data
                        CatDescription = CatDescription,
                        // Parses and sets entity ID, defaults to 0 if invalid
                        TenantId = tenantId,
                        // Sets creator identifier
                        CreatedBy = createdBy
                    };
                    // Adds model to the collection
                    models.Add(model);
                }

                // Processes each model for database insertion
                foreach (var model in models)
                {
                    // Checks if identical record already exists in database
                    var existingEntity = await _uow.CategoryRepository.Query().AnyAsync(p => p.CategoryName == model.CategoryName && p.TenantId == model.TenantId);
                    if (existingEntity)
                    {
                        // Increments duplicate records counter
                        dublicatedRecordsCount++;
                        // Skips insertion for duplicate records
                        continue;
                    }

                    // Sets creation timestamp to current UTC time
                    model.CreatedByDateTime = DateTime.UtcNow;
                    // Sets first update timestamp to current UTC time
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    // Maps and adds model to repository for insertion
                    _uow.CategoryRepository.Add(_mapper.Map<Category>(model));
                    // Increments inserted records counter
                    insertedRecordsCount++;
                }

                // Commits all changes to database
                await _uow.CompleteAsync();

                // Updates global import statistics
                UpdateImportCounts(rowCount, insertedRecordsCount);

                // Builds success message if records were inserted
                if (insertedRecordsCount > 0)
                {
                    // Sets success message with count
                    resultMessage = $" Category page = {insertedRecordsCount} Category Inserted Successfully.";
                }
                // Appends skipped records message if any were skipped
                if (skippedRecordsCount > 0)
                {
                    // Appends skipped records information to result message
                    resultMessage += $" Category page = {skippedRecordsCount} records were not inserted because of missing required field.";
                }
                // Appends duplicate records message if any duplicates found
                if (dublicatedRecordsCount > 0)
                {
                    // Appends duplicate records information to result message
                    resultMessage += $" Category page =  {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                // Returns error message with exception details
                resultMessage = "Error On Category page = " + ex.Message;
            }

            // Returns final result message
            return resultMessage;
        }

        /// <summary>
        /// Imports product information from an Excel worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet containing the product info data.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, returning a summary message of the import process.</returns>
        public async Task<string> ImportInfo(ExcelWorksheet worksheet, string createdBy,int tenantId)
        {
            // Gets the total number of rows in the worksheet
            int rowCount = GetRowCount(worksheet);
            // Initializes a list to store product models
            var models = new List<Product>();
            // Counter for skipped records due to validation errors
            int skippedRecordsCount = 0;
            // Counter for duplicate records found in the database
            int dublicatedRecordsCount = 0;
            // Counter for successfully inserted records
            int insertedRecordsCount = 0;
            // Stores the result message of the import operation
            var resultMessage = "";
            string[] requiredHeaders =
[
    "Code*",
    "StreamName*",
    "Category*",
    "CategoryId*",
    "Entity*",
    "TenantId*",
    "StreamImage",
    "Narrative",
    "Description"

];

            // Read header row from Excel
            var excelHeaders = new List<string>();
            for (int col = 1; col <= requiredHeaders.Length; col++)
            {
                excelHeaders.Add(worksheet.Cells[1, col].Text?.Trim()!);
            }

            // Validate each header
            for (int i = 0; i < requiredHeaders.Length; i++)
            {
                if (!excelHeaders[i].Equals(requiredHeaders[i], StringComparison.OrdinalIgnoreCase))
                {
                    return $"Incorrect file format at Column {i + 1}. " +
                           $"Expected '{requiredHeaders[i]}' but found '{excelHeaders[i]}'";
                }
            }
            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    // Returns message indicating empty worksheet
                    return resultMessage = " Stream page = Uploaded File Info sheets Is Empty";
                }
                // Processes each row in the worksheet
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Reads product code from column 1
                    var Code = worksheet.Cells[row, 1].Text;
                    // Reads product name from column 2
                    var ProductName = worksheet.Cells[row, 2].Text;
                    // Reads category ID from column 4
                    var CategoryId = worksheet.Cells[row, 4].Text;
                    // Reads entity from column 5
                    var Entity = worksheet.Cells[row, 5].Text;
                    // Reads entity ID from column 6
                    var TenantId = worksheet.Cells[row, 6].Text;
                    // Reads image URL from column 7
                    var imageUrl = worksheet.Cells[row, 7].Text;
                    // Reads narrative from column 8
                    var Narrative = worksheet.Cells[row, 8].Text;
                    // Reads description from column 9
                    var Description = worksheet.Cells[row, 9].Text;

                    // Check for missing or invalid data
                    if (string.IsNullOrWhiteSpace(Code) || !int.TryParse(CategoryId, out _) || string.IsNullOrWhiteSpace(ProductName) || !int.TryParse(TenantId, out _))
                    {
                        // Increments skipped records counter
                        skippedRecordsCount++;
                        // Skips current record due to validation failure
                        continue;
                    }

                    // Add valid model to the list
                    var model = new Product
                    {
                        // Sets product code from Excel data
                        Code = Code,
                        // Sets product name from Excel data
                        ProductName = ProductName,
                        // Parses and sets category ID, defaults to 0 if invalid
                        CategoryId = int.TryParse(CategoryId, out int categoryId) ? categoryId : 0,
                        // Parses and sets entity ID, defaults to 0 if invalid
                        TenantId = tenantId,
                        // Sets narrative from Excel data
                        Narrative = Narrative,
                        // Sets creator identifier
                        CreatedBy = createdBy
                    };
                    // Adds model to the collection
                    models.Add(model);
                }

                // Processes each model for database insertion
                foreach (var model in models)
                {
                    // Checks if identical record already exists in database
                    var existingEntity = await _uow.ProductRepository.Query().AnyAsync(p => p.Code == model.Code || (p.ProductName == model.ProductName && p.CategoryId == model.CategoryId && p.TenantId == model.TenantId && p.TenantId == model.TenantId));
                    if (existingEntity)
                    {
                        // Increments duplicate records counter
                        dublicatedRecordsCount++;
                        // Skips insertion for duplicate records
                        continue;
                    }

                    // Sets creation timestamp to current UTC time
                    model.CreatedByDateTime = DateTime.UtcNow;
                    // Sets first update timestamp to current UTC time
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    // Sets category ID to null if it's 0, otherwise keeps the value
                    model.CategoryId = model.CategoryId == 0 ? null : model.CategoryId;
                    // Maps and adds model to repository for insertion
                    _uow.ProductRepository.Add(_mapper.Map<Product>(model));
                    // Increments inserted records counter
                    insertedRecordsCount++;
                }

                // Commits all changes to database
                await _uow.CompleteAsync();

                // Updates global import statistics
                UpdateImportCounts(rowCount, insertedRecordsCount);

                // Builds success message if records were inserted
                if (insertedRecordsCount > 0)
                {
                    // Sets success message with count
                    resultMessage = $" Stream page = {insertedRecordsCount} Stream Inserted Successfully.";
                }
                // Appends skipped records message if any were skipped
                if (skippedRecordsCount > 0)
                {
                    // Appends skipped records information to result message
                    resultMessage += $" Stream page = {skippedRecordsCount} records were not inserted because of missing required field.";
                }
                // Appends duplicate records message if any duplicates found
                if (dublicatedRecordsCount > 0)
                {
                    // Appends duplicate records information to result message
                    resultMessage += $" Stream page = {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                // Returns error message with exception details
                resultMessage = "Error On Stream page = " + ex.Message;
            }

            // Returns final result message
            return resultMessage;
        }
        private static async Task<byte[]?> SafeLoadImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            try
            {
                return await ConvertImagePathToByteArrayAsync(imageUrl);
            }
            catch
            {
                // URL not valid / file missing / permission denied → return null
                return null;
            }
        }

        /// <summary>
        /// Converts an image path or URL to a byte array.
        /// </summary>
        /// <param name="imageUrl">The image URL or file path.</param>
        /// <returns>A byte array representing the image.</returns>
        private static async Task<byte[]> ConvertImagePathToByteArrayAsync(string imageUrl)
        {
            // Checks if URL is HTTP or HTTPS
            if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
            {
                // Creates HTTP client for downloading image
                using var httpClient = new HttpClient();
                // Download the image as a byte array from the HTTP/HTTPS URL
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                // Return the image as byte[]
                return imageBytes;
            }
            // Checks if URL is file protocol
            else if (imageUrl.StartsWith("file://"))
            {
                var filePath = imageUrl[7..];

                // Read the file as a byte array from the local file system
                if (File.Exists(filePath))
                {
                    // Reads file content as byte array
                    var imageBytes = await File.ReadAllBytesAsync(filePath);
                    // Return the image as byte[]
                    return imageBytes;
                }
                else
                {
                    // Throws exception if file not found
                    throw new FileNotFoundException("The specified file was not found.", filePath);
                }
            }
            else
            {
                // Throws exception for invalid URL format
                throw new ArgumentException("Invalid URL format.");
            }
        }

        /// <summary>
        /// Imports product details from an Excel worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet containing the details data.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, returning a summary message of the import process.</returns>
        public async Task<string> ImportDetails(ExcelWorksheet worksheet, string createdBy)
        {
            // Gets the total number of rows in the worksheet
            int rowCount = GetRowCount(worksheet);
            // Initializes a list to store product parameter models
            var models = new List<ProductParam>();
            // Counter for skipped records due to validation errors
            int skippedRecordsCount = 0;
            // Counter for successfully inserted records
            int insertedRecordsCount = 0;
            // Counter for duplicate records found in the database
            int dublicatedRecordsCount = 0;
            // Stores the result message of the import operation
            var resultMessage = "";

            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    // Returns message indicating empty worksheet
                    return resultMessage = " Details page = Uploaded File Details sheets Is Empty";
                }
                // Read data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Reads product ID from column 2
                    var productId = worksheet.Cells[row, 2].Text;
                    // Reads entity ID from column 4
                    var tenantId = worksheet.Cells[row, 4].Text;
                    // Reads parameter ID from column 6
                    var parameterId = worksheet.Cells[row, 6].Text;
                    // Reads parameter value from column 7
                    var paramValue = worksheet.Cells[row, 7].Text;
                    // Reads display order from column 8
                    var DisplayOrder = worksheet.Cells[row, 8].Text;
                    // Reads is required flag from column 9
                    var IsRequired = worksheet.Cells[row, 9].Text;

                    // Check if required fields are empty or invalid
                    if (!int.TryParse(productId, out _) || !int.TryParse(tenantId, out _) || !int.TryParse(parameterId, out _) || string.IsNullOrEmpty(paramValue) || string.IsNullOrEmpty(DisplayOrder) || !bool.TryParse(IsRequired, out _))
                    {
                        // Increments skipped records counter
                        skippedRecordsCount++;
                        // Skip the record if any required field is missing
                        continue;
                    }

                    // Creates new product parameter model instance
                    var model = new ProductParam
                    {
                        // Parses and sets product ID, defaults to 0 if invalid
                        ProductId = int.TryParse(productId, out int ProductId) ? ProductId : 0,
                        // Parses and sets entity ID, defaults to 0 if invalid
                        TenantId = int.TryParse(tenantId, out int TenantId) ? TenantId : 0,
                        // Parses and sets parameter ID, defaults to 0 if invalid
                        ParameterId = int.TryParse(parameterId, out int ParameterId) ? ParameterId : 0,
                        // Sets parameter value from Excel data
                        ParamValue = paramValue,
                        // Sets creator identifier
                        CreatedBy = createdBy,
                        // Parses and sets display order, defaults to 0 if invalid
                        DisplayOrder = int.TryParse(DisplayOrder, out int displayOrder) ? displayOrder : 0,
                        // Parses and sets is required flag, defaults to false if invalid
                        IsRequired = bool.TryParse(IsRequired, out bool isRequired) && isRequired,
                    };
                    // Adds model to the collection
                    models.Add(model);
                }

                // Processes each model for database insertion
                foreach (var model in models)
                {
                    // Checks if identical record already exists in database
                    var existingEntity = await _uow.ProductParamRepository.Query().AnyAsync(p => p.ProductId == model.ProductId && p.ParameterId == model.ParameterId);
                    if (existingEntity)
                    {
                        // Increments duplicate records counter
                        dublicatedRecordsCount++;
                        // Skips insertion for duplicate records
                        continue;
                    }

                    // Sets creation timestamp to current UTC time
                    model.CreatedByDateTime = DateTime.UtcNow;
                    // Sets first update timestamp to current UTC time
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    // Sets second update timestamp to current UTC time (duplicate line)
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    // Maps and adds model to repository for insertion
                    _uow.ProductParamRepository.Add(_mapper.Map<ProductParam>(model));
                    // Increments inserted records counter
                    insertedRecordsCount++;
                }

                // Commits all changes to database
                await _uow.CompleteAsync();

                // Updates global import statistics
                UpdateImportCounts(rowCount, insertedRecordsCount);

                // Builds success message if records were inserted
                if (insertedRecordsCount > 0)
                {
                    // Sets success message with count
                    resultMessage = $" Details page = {models.Count} Details Inserted Successfully.";
                }
                // Appends skipped records message if any were skipped
                if (skippedRecordsCount > 0)
                {
                    // Appends skipped records information to result message
                    resultMessage += $" Details page = {skippedRecordsCount} records were not inserted because of missing required field.";
                }
                // Appends duplicate records message if any duplicates found
                if (dublicatedRecordsCount > 0)
                {
                    // Appends duplicate records information to result message
                    resultMessage += $" Details page = {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                // Returns error message with exception details
                resultMessage = "Error On Details page = " + ex.Message;
            }

            // Returns final result message
            return resultMessage;
        }

        public async Task<string> ImportEruleMaster(int tenantId, Stream fileStream, string createdBy)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];

            if (worksheet == null)
                return "Invalid file format. Worksheet not found.";

            // Expected header row
            string[] expectedHeaders =
            [
        "RuleName*",
        "RuleDescription*",
        "IsActive"
            ];

            static bool Same(string a, string b)
            {
                return string.Equals(
                    a?.Replace(" ", "").Trim(),
                    b?.Replace(" ", "").Trim(),
                    StringComparison.OrdinalIgnoreCase
                );
            }

            // Validate header row
            for (int col = 0; col < expectedHeaders.Length; col++)
            {
                string header = worksheet.Cells[1, col + 1].Text.Trim();
                if (!Same(header, expectedHeaders[col]))
                {
                    return $"Incorrect file format at Column {col + 1}. Expected '{expectedHeaders[col]}'.";
                }
            }

            int rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount <= 1)
                return "ERuleMaster = Uploaded file is empty.";

            int inserted = 0, skipped = 0, duplicate = 0;
            var models = new List<EruleMaster>();
            HashSet<string> excelDuplicateCheck = [];

            try
            {
                for (int row = 2; row <= rowCount; row++)
                {
                    string ruleName = worksheet.Cells[row, 1].Text.Trim();
                    string ruleDesc = worksheet.Cells[row, 2].Text.Trim();
                    string isActiveStr = worksheet.Cells[row, 3].Text.Trim();

                    // Excel-level duplicate prevention
                    string excelKey = $"{ruleName?.ToLower()}_{tenantId}";
                    if (excelDuplicateCheck.Contains(excelKey))
                    {
                        skipped++;
                        continue;
                    }
                    excelDuplicateCheck.Add(excelKey);

                    // Required field validation
                    if (string.IsNullOrEmpty(ruleName) || string.IsNullOrEmpty(ruleDesc))
                    {
                        skipped++;
                        continue;
                    }

                    // Parse IsActive (supports: true/false/1/0/yes/no)
                    bool isActive = false;
                    if (!string.IsNullOrWhiteSpace(isActiveStr))
                    {
                        string lower = isActiveStr.ToLower();
                        if (lower == "1" || lower == "true" || lower == "yes")
                            isActive = true;
                    }

                    var master = new EruleMaster
                    {
                        EruleName = ruleName,
                        EruleDesc = ruleDesc,
                        IsActive = isActive,
                        TenantId = tenantId,
                        CreatedBy = createdBy,
                        CreatedByDateTime = DateTime.UtcNow,
                        UpdatedBy = createdBy,
                        UpdatedByDateTime = DateTime.UtcNow
                    };

                    models.Add(master);
                }

                // Insert models into DB
                foreach (var model in models)
                {
                    bool exists = await _uow.EruleMasterRepository.Query()
                        .AnyAsync(x => x.TenantId == tenantId && x.EruleName == model.EruleName);

                    if (exists)
                    {
                        duplicate++;
                        continue;
                    }

                    _uow.EruleMasterRepository.Add(model);
                    inserted++;
                }

                await _uow.CompleteAsync();

                // Final combined message
                var messages = new List<string>();

                if (inserted > 0)
                    messages.Add($" ERuleMaster = {inserted} record{(inserted > 1 ? "s" : "")} inserted successfully");

                if (skipped > 0)
                    messages.Add($" ERuleMaster = {skipped} record{(skipped > 1 ? "s were" : " was")} skipped due to missing required fields");

                if (duplicate > 0)
                    messages.Add($" ERuleMaster = {duplicate} duplicate record{(duplicate > 1 ? "s" : "")} found");

                return messages.Count > 0
                    ? string.Join(". ", messages) + "."
                    : "No new records to insert.";
            }
            catch (Exception ex)
            {
                return "Error on ERuleMaster page: " + ex.Message;
            }
        }

        public async Task<string> ImportEruleMaster(ExcelWorksheet worksheet, string createdBy, int tenantId)
        {
            if (worksheet == null)
                return "Invalid file format. Worksheet not found.";

            // Expected header row
            string[] expectedHeaders =
            [
        "RuleName*",
        "RuleDescription*",
        "IsActive*"
            ];

            static bool Same(string a, string b)
            {
                return string.Equals(
                    a?.Replace(" ", "").Trim(),
                    b?.Replace(" ", "").Trim(),
                    StringComparison.OrdinalIgnoreCase
                );
            }

            // Validate header row
            for (int col = 0; col < expectedHeaders.Length; col++)
            {
                string header = worksheet.Cells[1, col + 1].Text.Trim();
                if (!Same(header, expectedHeaders[col]))
                {
                    return $"Incorrect file format at Column {col + 1}. Expected '{expectedHeaders[col]}'.";
                }
            }

            int rowCount = GetRowCount(worksheet);
            if (rowCount <= 1)
                return "Uploaded file is empty.";

            int inserted = 0, skipped = 0, duplicate = 0;
            var models = new List<EruleMaster>();
            HashSet<string> excelDuplicateCheck = [];

            try
            {
                for (int row = 2; row <= rowCount; row++)
                {
                    string ruleName = worksheet.Cells[row, 1].Text.Trim();
                    string ruleDesc = worksheet.Cells[row, 2].Text.Trim();
                    string isActiveStr = worksheet.Cells[row, 3].Text.Trim();

                    // Excel-level duplicate prevention
                    string excelKey = ruleName?.ToLower()!;
                    if (excelDuplicateCheck.Contains(excelKey))
                    {
                        skipped++;
                        continue;
                    }
                    excelDuplicateCheck.Add(excelKey);

                    // Required field validation
                    if (string.IsNullOrEmpty(ruleName) || string.IsNullOrEmpty(ruleDesc) || string.IsNullOrEmpty(isActiveStr))
                    {
                        skipped++;
                        continue;
                    }

                    // Parse IsActive (supports: true/false/1/0/yes/no)
                    bool isActive = false;
                    if (!string.IsNullOrWhiteSpace(isActiveStr))
                    {
                        string lower = isActiveStr.ToLower();
                        if (lower == "1" || lower == "true" || lower == "yes")
                            isActive = true;
                    }

                    var master = new EruleMaster
                    {
                        TenantId = tenantId,
                        EruleName = ruleName,
                        EruleDesc = ruleDesc,
                        IsActive = isActive,
                        CreatedBy = createdBy,
                        CreatedByDateTime = DateTime.UtcNow,
                        UpdatedBy = createdBy,
                        UpdatedByDateTime = DateTime.UtcNow
                    };

                    models.Add(master);
                }

                // Insert models into DB
                foreach (var model in models)
                {
                    bool exists = await _uow.EruleMasterRepository.Query()
                        .AnyAsync(x => x.EruleName == model.EruleName);

                    if (exists)
                    {
                        duplicate++;
                        continue;
                    }

                    _uow.EruleMasterRepository.Add(model);
                    inserted++;
                }

                await _uow.CompleteAsync();
                UpdateImportCounts(rowCount, inserted);

                // Final combined message
                var messages = new List<string>();
                if (inserted > 0)
                    messages.Add($"{inserted} record{(inserted > 1 ? "s" : "")} inserted successfully");
                if (skipped > 0)
                    messages.Add($"{skipped} record{(skipped > 1 ? "s were" : " was")} skipped due to missing required fields");
                if (duplicate > 0)
                    messages.Add($"{duplicate} duplicate record{(duplicate > 1 ? "s" : "")} found");

                return messages.Count > 0
                    ? string.Join(". ", messages) + "."
                    : "No new records to insert.";
            }
            catch (Exception ex)
            {
                return "Error on ERuleMaster page: " + ex.Message;
            }
        }

        /// <summary>
        /// Imports rules from an Excel worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet containing the rule data.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, returning a summary message of the import process.</returns>
        public async Task<string> ImportErule(ExcelWorksheet worksheet, string createdBy)
        {
            // Gets the number of rows in the worksheet
            int rowCount = GetRowCount(worksheet);

            // Initializes a list to store Erule models
            var models = new List<EruleListModel>();

            // Counter for skipped records
            int skippedRecordsCount = 0;

            // Counter for inserted records
            int insertedRecordsCount = 0;

            // Counter for duplicated records
            int dublicatedRecordsCount = 0;

            // Result message for the import operation
            var resultMessage = "";

            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    // Returns message indicating empty worksheet
                    return resultMessage = " Rule page = Uploaded File ERules sheets Is Empty";
                }

                // Reads data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Gets Erule name from cell
                    var EruleName = worksheet.Cells[row, 1].Text;

                    // Gets Erule description from cell
                    var EruleDesc = worksheet.Cells[row, 2].Text;

                    // Gets expression shown from cell
                    var ExpShown = worksheet.Cells[row, 9].Text;

                    // Checks if required fields are empty
                    if (string.IsNullOrEmpty(EruleName) || string.IsNullOrEmpty(EruleDesc) || string.IsNullOrEmpty(ExpShown))
                    {
                        // Increments skipped records counter
                        skippedRecordsCount++;

                        // Skips the record if any required field is missing
                        continue;
                    }

                    // Creates a new Erule model
                    var model = new EruleListModel
                    {
                        // Sets expression properties
                        Expression = ExpShown,
                        ExpShown = ExpShown,

                        // Sets created by information
                        CreatedBy = createdBy
                    };

                    // Adds model to the list
                    models.Add(model);
                }

                // Processes each model for insertion
                foreach (var model in models)
                {
                    // Sets creation timestamp
                    model.CreatedByDateTime = DateTime.UtcNow;

                    // Sets update timestamp
                    model.UpdatedByDateTime = DateTime.UtcNow;

                    // Maps and adds the model to repository
                    _uow.EruleRepository.Add(_mapper.Map<Erule>(model));

                    // Increments inserted records counter
                    insertedRecordsCount++;
                }

                // Commits changes to database
                await _uow.CompleteAsync();

                // Updates import statistics
                UpdateImportCounts(rowCount, insertedRecordsCount);

                // Appends success message if records were inserted
                if (insertedRecordsCount > 0)
                {
                    resultMessage = $" Rule page = {models.Count} Factor Inserted Successfully.";
                }

                // Appends skipped records message if any records were skipped
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $" Rule page = {skippedRecordsCount} records were not inserted because of missing required field.";
                }

                // Appends duplicate records message if any duplicates found
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $" Rule page = {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                // Returns error message if exception occurs
                resultMessage = "Error On Rule page = " + ex.Message;
            }

            // Returns the result message
            return resultMessage;
        }

        public async Task<string> ImportECard(ExcelWorksheet worksheet, int tenantId, string createdBy)
        {
            int rowCount = GetRowCount(worksheet);
            var models = new List<EcardListModel>();

            int skippedRecordsCount = 0;
            int insertedRecordsCount = 0;
            int duplicatedRecordsCount = 0;

            string resultMessage = "";
            string[] requiredHeaders =
[
    "CardName*",
    "CardDescription*",
    "ExpressionShown*"
];

            // Read header row from Excel
            var excelHeaders = new List<string>();
            for (int col = 1; col <= requiredHeaders.Length; col++)
            {
                excelHeaders.Add(worksheet.Cells[1, col].Text?.Trim()!);
            }

            // Validate each header
            for (int i = 0; i < requiredHeaders.Length; i++)
            {
                if (!excelHeaders[i].Equals(requiredHeaders[i], StringComparison.OrdinalIgnoreCase))
                {
                    return $"Incorrect file format at Column {i + 1}. " +
                           $"Expected '{requiredHeaders[i]}' but found '{excelHeaders[i]}'";
                }
            }
            try
            {
                if (rowCount <= 0)
                    return "E Card Page = Uploaded File Is Empty";

                // Read rows
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    var EcardName = worksheet.Cells[row, 1].Text;
                    var EcardDesc = worksheet.Cells[row, 2].Text;
                    var ExpressionShown = worksheet.Cells[row, 3].Text;

                    if (string.IsNullOrEmpty(EcardName) ||
                        string.IsNullOrEmpty(EcardDesc) ||
                        string.IsNullOrEmpty(ExpressionShown))
                    {
                        skippedRecordsCount++;
                        continue;
                    }

                    models.Add(new EcardListModel
                    {
                        TenantId = tenantId,
                        EcardName = EcardName,
                        EcardDesc = EcardDesc,
                        Expshown = ExpressionShown,
                        CreatedBy = createdBy
                    });
                }

                // Save models
                foreach (var model in models)
                {
                    var finalExpression = await BuildExpressionFromShown(model.Expshown!, _uow);

                    if (finalExpression == null)
                    {
                        skippedRecordsCount++;
                        resultMessage += $"Invalid ExpressionShown for Ecard '{model.EcardName}'. Please check expression. ";
                        continue;
                    }

                    model.Expression = finalExpression;

                    // Duplicate check
                    var exists = await _uow.EcardRepository.Query()
                        .AnyAsync(p =>
                            p.TenantId == tenantId &&
                            p.EcardName == model.EcardName &&
                            p.EcardDesc == model.EcardDesc
                        );

                    if (exists)
                    {
                        duplicatedRecordsCount++;
                        continue;
                    }

                    // timestamps
                    model.CreatedByDateTime = DateTime.UtcNow;
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    model.UpdatedBy = createdBy;
                    model.IsImport = true;

                    _uow.EcardRepository.Add(_mapper.Map<Ecard>(model));
                    insertedRecordsCount++;
                }

                await _uow.CompleteAsync();
                UpdateImportCounts(rowCount, insertedRecordsCount);

                if (insertedRecordsCount > 0)
                {
                    resultMessage = $"E Card page = {insertedRecordsCount} Ecard Inserted Successfully.";
                }

                // Appends skipped records message if any records were skipped
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $"E Card page = {skippedRecordsCount} records were not inserted because of missing required field.";
                }

                // Appends duplicate records message if any duplicates found
                if (duplicatedRecordsCount > 0)
                {
                    resultMessage += $"E Card page = {duplicatedRecordsCount} record already exists.";
                }
                //resultMessage =
                //    $"E Card page = {insertedRecordsCount} Ecard Inserted Successfully." +
                //    $"E Card page = {skippedRecordsCount} records were not inserted because of missing required field." +
                //    $"E Card page = {duplicatedRecordsCount} record already exists.";
            }
            catch (Exception ex)
            {
                resultMessage = "Error On E Card Page = " + ex.Message;
            }

            return resultMessage;
        }
        private static async Task<string?> BuildExpressionFromShown(string exprShown, IUnitOfWork _uow)
        {
            if (string.IsNullOrWhiteSpace(exprShown))
                return null;

            // 1️⃣ Split tokens by AND / OR
            var tokens = MyRegex3().Split(exprShown)
                              .Where(t => !string.IsNullOrWhiteSpace(t))
                              .ToList();

            // 2️⃣ Extract rule names from tokens
            var ruleNames = tokens
                .Where(t =>
                    !string.Equals(t, "AND", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(t, "OR", StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Trim('(', ')').Trim())
                .Distinct()
                .ToList();

            if (ruleNames.Count == 0)
                return null;

            // 3️⃣ First fetch matching EruleMaster rows
            var masterRules = await _uow.EruleMasterRepository.Query()
                                .Where(m => ruleNames.Contains(m.EruleName))
                                .ToListAsync();

            if (masterRules.Count != ruleNames.Count)
                return null;

            var masterIds = masterRules.Select(x => x.Id).ToList();

            // 4️⃣ Get related Erule rows (child rule table)
            var childRules = await _uow.EruleRepository.Query()
                                .Where(r => masterIds.Contains(r.EruleMasterId))
                                .ToListAsync();

            // 5️⃣ Pick highest version child rule for each master rule
            var finalRuleMap = childRules
                .GroupBy(r => r.EruleMasterId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.Version).First()
                );

            // 6️⃣ Build final expression using Erule.Id (NOT EruleMaster.Id)
            string finalExpr = "";
            string lastToken;

            foreach (var t in tokens)
            {
                var token = t.Trim();

                if (string.Equals(token, "And", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(token, "Or", StringComparison.OrdinalIgnoreCase))
                {
                    finalExpr += " " + token + " ";
                    lastToken = token;
                    continue;
                }

                string ruleName = token.Trim('(', ')').Trim();

                var master = masterRules.First(
                    m => m.EruleName.Equals(ruleName, StringComparison.OrdinalIgnoreCase)
                );

                var child = finalRuleMap[master.Id]; // highest version

                finalExpr += child.EruleId;

                lastToken = "RULE";
            }

            finalExpr = "(" + finalExpr.Trim() + ")";

            return finalExpr;
        }
        /// <summary>
        /// Imports ECard data from an Excel worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet containing the ECard data.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, returning a summary message of the import process.</returns>
        public async Task<string> ImportECard(ExcelWorksheet worksheet, string createdBy)
        {
            // Gets the number of rows in the worksheet
            int rowCount = GetRowCount(worksheet);

            // Initializes a list to store Ecard models
            var models = new List<EcardListModel>();

            // Counter for skipped records
            int skippedRecordsCount = 0;

            // Counter for inserted records
            int insertedRecordsCount = 0;

            // Counter for duplicated records
            int dublicatedRecordsCount = 0;

            // Result message for the import operation
            var resultMessage = "";

            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    // Returns message indicating empty worksheet
                    return resultMessage = " E Card Page = Uploaded File ECards sheets Is Empty";
                }

                // Reads data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Gets Ecard name from cell
                    var EcardName = worksheet.Cells[row, 1].Text;

                    // Gets Ecard description from cell
                    var EcardDesc = worksheet.Cells[row, 2].Text;

                    // Gets expression shown from cell
                    var ExpShown = worksheet.Cells[row, 7].Text;

                    // Checks if required fields are empty
                    if (string.IsNullOrEmpty(EcardName) || string.IsNullOrEmpty(EcardDesc) || string.IsNullOrEmpty(ExpShown))
                    {
                        // Increments skipped records counter
                        skippedRecordsCount++;

                        // Skips the record if any required field is missing
                        continue;
                    }

                    // Creates a new Ecard model
                    var model = new EcardListModel
                    {
                        // Sets Ecard name
                        EcardName = EcardName,

                        // Sets Ecard description
                        EcardDesc = EcardDesc,

                        // Sets expression shown
                        Expshown = ExpShown,

                        // Sets created by information
                        CreatedBy = createdBy
                    };

                    // Adds model to the list
                    models.Add(model);
                }

                // Processes each model for insertion
                foreach (var model in models)
                {
                    // Checks if entity already exists in database
                    var existingEntity = await _uow.EcardRepository.Query().AnyAsync(p => p.EcardName == model.EcardName && p.EcardDesc == model.EcardDesc);

                    // If entity exists, skip insertion
                    if (existingEntity)
                    {
                        // Increments duplicate records counter
                        dublicatedRecordsCount++;

                        // Skips to next model
                        continue;
                    }

                    // Sets creation timestamp
                    model.CreatedByDateTime = DateTime.UtcNow;

                    // Sets update timestamp
                    model.UpdatedByDateTime = DateTime.UtcNow;

                    // Maps and adds the model to repository
                    _uow.EcardRepository.Add(_mapper.Map<Ecard>(model));

                    // Increments inserted records counter
                    insertedRecordsCount++;
                }

                // Commits changes to database
                await _uow.CompleteAsync();

                // Updates import statistics
                UpdateImportCounts(rowCount, insertedRecordsCount);

                // Appends success message if records were inserted
                if (insertedRecordsCount > 0)
                {
                    resultMessage = $" E Card Page = {models.Count} New Eligibility Cards Inserted Successfully.";
                }

                // Appends skipped records message if any records were skipped
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $" E Card Page = {skippedRecordsCount} records were not inserted because of missing required field.";
                }

                // Appends duplicate records message if any duplicates found
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $" E Card Page = {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                // Returns error message if exception occurs
                resultMessage = "Error On E Card Page = " + ex.Message;
            }

            // Returns the result message
            return resultMessage;
        }

        /// <summary>
        /// Imports PCard data from an Excel worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet containing the PCard data.</param>
        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
        /// <returns>A task that represents the asynchronous operation, returning a summary message of the import process.</returns>
        public async Task<string> ImportPCards(ExcelWorksheet worksheet, string createdBy)
        {
            // Gets the number of rows in the worksheet
            int rowCount = GetRowCount(worksheet);

            // Initializes a list to store Pcard models
            var models = new List<PcardListModel>();

            // Counter for skipped records
            int skippedRecordsCount = 0;

            // Counter for inserted records
            int insertedRecordsCount = 0;

            // Counter for duplicated records
            int dublicatedRecordsCount = 0;

            // Result message for the import operation
            var resultMessage = "";

            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    // Returns message indicating empty worksheet
                    return resultMessage = " P Card Page = Uploaded File PCards sheets Is Empty";
                }

                // Reads data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Gets Pcard name from cell
                    var PcardName = worksheet.Cells[row, 1].Text;

                    // Gets Pcard description from cell
                    var PcardDesc = worksheet.Cells[row, 2].Text;

                    // Gets product ID from cell
                    var ProductId = worksheet.Cells[row, 4].Text;

                    // Gets expression shown from cell
                    var Expshown = worksheet.Cells[row, 9].Text;

                    // Checks if required fields are empty or invalid
                    if (string.IsNullOrEmpty(PcardName) || string.IsNullOrEmpty(PcardDesc) || string.IsNullOrEmpty(Expshown) || !int.TryParse(ProductId, out _))
                    {
                        // Increments skipped records counter
                        skippedRecordsCount++;

                        // Skips the record if any required field is missing
                        continue;
                    }

                    // Creates a new Pcard model
                    var model = new PcardListModel
                    {
                        // Sets Pcard name
                        PcardName = PcardName,

                        // Sets Pcard description
                        PcardDesc = PcardDesc,

                        // Sets expression shown
                        Expshown = Expshown,

                        // Sets created by information
                        CreatedBy = createdBy,

                        // Parses and sets product ID
                        ProductId = int.TryParse(ProductId, out int productId) ? productId : 0,
                    };

                    // Adds model to the list
                    models.Add(model);
                }

                // Processes each model for insertion
                foreach (var model in models)
                {
                    // Checks if entity already exists in database
                    var existingEntity = await _uow.PcardRepository.Query().AnyAsync(p => p.PcardName == model.PcardName && p.PcardDesc == model.PcardDesc && p.ProductId == model.ProductId);

                    // If entity exists, skip insertion
                    if (existingEntity)
                    {
                        // Increments duplicate records counter
                        dublicatedRecordsCount++;

                        // Skips to next model
                        continue;
                    }

                    // Sets creation timestamp
                    model.CreatedByDateTime = DateTime.UtcNow;

                    // Sets update timestamp
                    model.UpdatedByDateTime = DateTime.UtcNow;

                    // Maps and adds the model to repository
                    _uow.PcardRepository.Add(_mapper.Map<Pcard>(model));

                    // Increments inserted records counter
                    insertedRecordsCount++;
                }

                // Commits changes to database
                await _uow.CompleteAsync();

                // Updates import statistics
                UpdateImportCounts(rowCount, insertedRecordsCount);

                // Appends success message if records were inserted
                if (insertedRecordsCount > 0)
                {
                    resultMessage = $" P Card Page = {models.Count} Product Card Inserted Successfully.";
                }

                // Appends skipped records message if any records were skipped
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $" P Card Page = {skippedRecordsCount} records were not inserted because of missing required field.";
                }

                // Appends duplicate records message if any duplicates found
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $" P Card Page = {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                // Returns error message if exception occurs
                resultMessage = "Error On P Card Page = " + ex.Message;
            }

            // Returns the result message
            return resultMessage;
        }

        public async Task<string> ImportPCard(ExcelWorksheet worksheet, int tenantId, string createdBy)
        {
            int rowCount = GetRowCount(worksheet);
            var models = new List<PcardListModel>();

            int skippedRecordsCount = 0;
            int insertedRecordsCount = 0;
            int duplicatedRecordsCount = 0;

            string resultMessage = "";
            string[] requiredHeaders =
            [
                "StreamCardName*",
        "StreamCardDescription*",
        "StreamName*",
        "StreamId*",
        "ExpressionShown*"
            ];

            // Read header row from Excel
            var excelHeaders = new List<string>();
            for (int col = 1; col <= requiredHeaders.Length; col++)
            {
                excelHeaders.Add(worksheet.Cells[1, col].Text?.Trim()!);
            }

            // Validate headers
            for (int i = 0; i < requiredHeaders.Length; i++)
            {
                if (!excelHeaders[i].Equals(requiredHeaders[i], StringComparison.OrdinalIgnoreCase))
                {
                    return $"Incorrect file format at Column {i + 1}. " +
                           $"Expected '{requiredHeaders[i]}' but found '{excelHeaders[i]}'";
                }
            }

            try
            {
                if (rowCount <= 0)
                    return "Stream Card Page = Uploaded File PCards sheet is empty.";

                // Read rows
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    var PcardName = worksheet.Cells[row, 1].Text;
                    var PcardDesc = worksheet.Cells[row, 2].Text;
                    var ProductName = worksheet.Cells[row, 3].Text;
                    var ProductIdText = worksheet.Cells[row, 4].Text;
                    var ExpressionShown = worksheet.Cells[row, 5].Text;

                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(PcardName) ||
                        string.IsNullOrWhiteSpace(PcardDesc) ||
                        string.IsNullOrWhiteSpace(ProductName) ||
                        string.IsNullOrWhiteSpace(ProductIdText) ||
                        string.IsNullOrWhiteSpace(ExpressionShown))
                    {
                        skippedRecordsCount++;
                        continue;
                    }

                    // Parse ProductId
                    if (!int.TryParse(ProductIdText, out var productId))
                    {
                        skippedRecordsCount++;
                        resultMessage += $"Invalid ProductId '{ProductIdText}' for PCard '{PcardName}'. ";
                        continue;
                    }

                    // Check product exists
                    var product = await _uow.ProductRepository.Query()
                        .Where(p => p.ProductName != null &&
                                    p.ProductName == ProductName)
                        .FirstOrDefaultAsync();
                                        if (product == null)
                    {
                        skippedRecordsCount++;
                        resultMessage += $"Provide correct Stream name '{ProductName}' for PCard '{PcardName}'. ";
                        continue;
                    }

                    // Check already associated
                    var alreadyExist = _uow.PcardRepository.Query().Any(p => p.ProductId == productId);
                    if (alreadyExist)
                    {
                        resultMessage += $"Stream '{ProductName}' for Stream Card '{PcardName}' is already associated with another Stream Card. ";
                        continue;
                    }

                    // Create model
                    var model = new PcardListModel
                    {
                        PcardName = PcardName,
                        PcardDesc = PcardDesc,
                        Expshown = ExpressionShown,
                        CreatedBy = createdBy,
                        UpdatedBy = createdBy,
                        ProductId = productId,
                        TenantId = tenantId
                    };

                    models.Add(model);
                }

                // Process models for insertion
                foreach (var model in models)
                {
                    // Build final expression similar to ECard
                    var finalExpression = await BuildPCardExpressionFromShown(model.Expshown!, _uow);

                    if (finalExpression == null)
                    {
                        skippedRecordsCount++;
                        resultMessage += $"Invalid ExpressionShown for Stream Card '{model.PcardName}'. Please check expression. ";
                        continue;
                    }

                    model.Expression = finalExpression;

                    // Duplicate check
                    var exists = await _uow.PcardRepository.Query()
                        .AnyAsync(p =>
                            p.TenantId == model.TenantId &&
                            p.PcardName == model.PcardName &&
                            p.PcardDesc == model.PcardDesc &&
                            p.ProductId == model.ProductId);

                    if (exists)
                    {
                        duplicatedRecordsCount++;
                        continue;
                    }

                    // timestamps
                    model.CreatedByDateTime = DateTime.UtcNow;
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    model.UpdatedBy = createdBy;
                    model.IsImport = true;

                    _uow.PcardRepository.Add(_mapper.Map<Pcard>(model));
                    insertedRecordsCount++;
                }

                await _uow.CompleteAsync();
                UpdateImportCounts(rowCount, insertedRecordsCount);

                // Messages
                if (insertedRecordsCount > 0)
                    resultMessage += $" Stream Card Page = {insertedRecordsCount} Stream Card(s) inserted successfully.";

                if (skippedRecordsCount > 0)
                    resultMessage += $" Stream Card Page = {skippedRecordsCount} record(s) skipped due to missing/invalid data or expression.";

                if (duplicatedRecordsCount > 0)
                    resultMessage += $" Stream Card Page = {duplicatedRecordsCount} record(s) already exist.";
            }
            catch (Exception ex)
            {
                resultMessage = "Error On Stream Card Page = " + ex.Message;
            }

            return resultMessage;
        }

        private static async Task<string?> BuildPCardExpressionFromShown(string exprShown, IUnitOfWork _uow)
        {
            if (string.IsNullOrWhiteSpace(exprShown))
                return null;

            // 1️⃣ Split tokens by AND / OR
            var tokens = MyRegex3().Split(exprShown)
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                            .ToList();

            // 2️⃣ Extract ECard names from tokens
            var ecardNames = tokens
                .Where(t => !string.Equals(t, "AND", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(t, "OR", StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Trim('(', ')').Trim())
                .Distinct()
                .ToList();

            if (ecardNames.Count == 0)
                return null;

            // 3️⃣ Fetch matching ECard rows
            var ecardList = await _uow.EcardRepository.Query()
                                  .Where(e => ecardNames.Contains(e.EcardName))
                                  .ToListAsync();

            if (ecardList.Count != ecardNames.Count)
                return null; // Some ECard names are invalid

            // 4️⃣ Build a map of ECard name -> Id
            var ecardMap = ecardList.ToDictionary(e => e.EcardName, e => e.EcardId);

            // 5️⃣ Build final expression using ECard.Id
            string finalExpr = "";

            foreach (var t in tokens)
            {
                var token = t.Trim();

                if (string.Equals(token, "AND", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(token, "OR", StringComparison.OrdinalIgnoreCase))
                {
                    finalExpr += " " + token + " ";
                    continue;
                }

                var ecardName = token.Trim('(', ')').Trim();

                if (!ecardMap.TryGetValue(ecardName, out int value))
                    return null; // Safety check

                finalExpr += value;
            }

            return "(" + finalExpr.Trim() + ")";
        }
        /// <summary>
        /// Gets the number of data rows in the specified worksheet.
        /// </summary>
        /// <param name="worksheet">The Excel worksheet to check.</param>
        /// <returns>The number of data rows in the worksheet.</returns>
        static int GetRowCount(ExcelWorksheet worksheet)
        {
            // Gets the last row number in the worksheet
            int lastRow = worksheet.Dimension.End.Row;

            // Initializes counter for last non-empty row
            int lastNonEmptyRow = 0;

            // Iterates through each row to find the last non-empty row
            for (int row = 2; row <= lastRow; row++)
            {
                // Checks if any of the first three cells have data
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text);

                // If row has data, update last non-empty row
                if (hasData)
                {
                    // Track the last row that has data
                    lastNonEmptyRow = row;
                }
            }

            // Returns the count of data rows (subtracting 1 for header row)
            return lastNonEmptyRow - 1;
        }

        /// <summary>
        /// Downloads an Excel template for bulk import.
        /// </summary>
        /// <param name="tenantId">The entity ID for which to generate the template.</param>
        /// <param name="selectedList">The selected list type.</param>
        /// <returns>A byte array containing the template file.</returns>
        public async Task<byte[]> DownloadTemplate(int tenantId, string selectedList)
        {
            // Retrieves all countries from service
            List<CountryModel> countries = _countryService.GetAll();

            // Retrieves all cities from service, ordered by country ID
            List<CityModel> cities = [.. _cityService.GetAll().OrderBy(x => x.CountryId)];

            // Retrieves all entities from service
            //List<EntityModel> entities = _entityService.GetAll();

            // Retrieves all erules for the specified entity
            List<EruleListModel> erule = _eruleService.GetAll(tenantId);

            // Retrieves all parameters for the specified entity
            List<ParameterListModel> parameters = _parameterService.GetAll(tenantId);

            // Retrieves all conditions from service
            List<ConditionModel> conditions = _conditionService.GetAll();

            // Retrieves all factors for the specified entity
            List<FactorListModel> factors = _factorService.GetAll(tenantId);

            // Retrieves all managed list items for the specified entity
            List<ManagedListGetModel> listItem = _managedListService.GetAll(tenantId);

            // Retrieves all data types from service
            List<DataTypeModel> dataTypes = _dataTypeService.GetAll();

            // Retrieves all ecards for the specified entity
            List<EcardListModel> ecard = _ecardService.GetAll(tenantId);

            // Retrieves all products for the specified entity
            List<ProductListModel> product = _productService.GetAll(tenantId);

            // Retrieves all categories for the specified entity
            List<CategoryListModel> categories = _categoryService.GetAll(tenantId);

            // Sets EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Creates a new Excel package
            using var package = new ExcelPackage();

            // Creates VBA project for the workbook
            package.Workbook.CreateVBAProject();

            // Only add pages based on selectedList
            if (selectedList == "All" || selectedList == "Entities")
            {
                // Adds Entities worksheet
                var sheet1 = package.Workbook.Worksheets.Add("Entities");

                // Populates Entities template
                EntityTemplate(sheet1, countries, cities/*, entities*/);
            }

            if (selectedList == "All" || selectedList == "Lists")
            {
                // Adds Lists worksheet
                var sheet2 = package.Workbook.Worksheets.Add("Lists");

                // Populates Lists template
                ListsTemplate(sheet2);
            }

            if (selectedList == "All" || selectedList == "ListItem")
            {
                // Adds ListItem worksheet
                var sheet3 = package.Workbook.Worksheets.Add("ListItem");

                // Populates ListItem template
                ListIteamTemplate(sheet3, listItem);
            }



            if (selectedList == "All" || selectedList == "Parameter")
            {
                // Adds Product-Parameter worksheet
                var sheet5 = package.Workbook.Worksheets.Add("Parameter");

                // Populates Product Parameter template
                ProductParameterTemplate(sheet5, dataTypes, conditions/*, entities*/);
            }

            if (selectedList == "All" || selectedList == "Factors")
            {
                // Adds Factors worksheet
                var sheet6 = package.Workbook.Worksheets.Add("Factors");

                // Populates Factors template
                FactorTemplate(sheet6, parameters, conditions, listItem);
            }

            if (selectedList == "All" || selectedList == "Category")
            {
                // Adds Category worksheet
                var sheet7 = package.Workbook.Worksheets.Add("Category");

                // Populates Category template
                CategoryTemplate(sheet7/*, entities*/);
            }

            if (selectedList == "All" || selectedList == "Stream")
            {
                // Adds Info worksheet
                var sheet8 = package.Workbook.Worksheets.Add("Stream");

                // Populates Info template
                InfoTemplate(sheet8/*, entities*/, categories);
            }

            //if (selectedList == "All" || selectedList == "Details")
            //{
            //    // Adds Details worksheet
            //    var sheet9 = package.Workbook.Worksheets.Add("Details");

            //    // Populates Details template
            //    DetailsTemplate(sheet9, product, entities, parameters, factors);
            //}

            if (selectedList == "All" || selectedList == "ERules")
            {
                // Adds ERules worksheet
                var sheet10 = package.Workbook.Worksheets.Add("ERules");

                // Populates Rules template
                EruleMasterRulesTemplate(sheet10);
            }

            if (selectedList == "All" || selectedList == "ECards")
            {
                // Adds ECards worksheet
                var sheet11 = package.Workbook.Worksheets.Add("ECards");

                // Populates ECard template
                ECardsTemplates(sheet11, erule);
            }

            if (selectedList == "All" || selectedList == "StreamCards")
            {
                // Adds PCards worksheet
                var sheet12 = package.Workbook.Worksheets.Add("StreamCards");

                // Populates PCard template
                PCardsTemplates(sheet12, ecard, product);
            }

            // AutoFit columns for each sheet added
            foreach (var sheet in package.Workbook.Worksheets)
            {
                // Auto-fits columns for better visibility
                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            }

            // Returns the Excel file as byte array
            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// Populates the entity template worksheet.
        /// </summary>
        /// <param name="sheet1">The worksheet to populate.</param>
        /// <param name="countries">List of countries.</param>
        /// <param name="cities">List of cities.</param>
        /// <param name="entities">List of entities.</param>
        public static void EntityTemplate(ExcelWorksheet sheet1, List<CountryModel> countries, List<CityModel> cities/*, List<EntityModel> entities*/)
        {
            // Sanitizes country names for use as range names
            foreach (var country in countries)
            {
                country.CountryName = SanitizeRangeName(country.CountryName ?? "");
            }

            // Sanitizes city names for use as range names
            foreach (var city in cities)
            {
                city.CityName = SanitizeRangeName(city.CityName ?? "");
            }

            // UPDATED: Entity header columns (same as DownloadTemplate)
            string[] entityHeader =
            [
                "EntityName*",
        "CountryName*",
        "CountryId*",
        "CityName*",
        "CityId*",
        "Address*",
        "IsChild",
        "ParentEntity",
        "ParentTenantId",
        "Code*",
        "Field Description"
            ];

            // Populate header row
            for (int i = 0; i < entityHeader.Length; i++)
            {
                sheet1.Cells[1, i + 1].Value = entityHeader[i];
            }

            // Reference columns (NO CHANGE)
            sheet1.Cells[1, 13].Value = "CountryNameValue";
            sheet1.Cells[1, 14].Value = "CountryIdValue";
            sheet1.Cells[1, 16].Value = "CityNameValue";
            sheet1.Cells[1, 17].Value = "CityIdValue";
            sheet1.Cells[1, 18].Value = "CityCountryIdValue";
            sheet1.Cells[1, 20].Value = "IsChildValue";
            sheet1.Cells[1, 22].Value = "ParentEntityName";
            sheet1.Cells[1, 23].Value = "ParentTenantId";

            // IsChild dropdown values
            string[] IsChildOptions = ["True", "False"];
            for (int i = 0; i < IsChildOptions.Length; i++)
            {
                sheet1.Cells[i + 2, 20].Value = IsChildOptions[i];
            }

            // UPDATED: Description in correct column (column 11 = K)
            sheet1.Cells[2, 11].Value = "* Fields marked with an asterisk are required.";
            sheet1.Cells[2, 11].Style.Font.Bold = true;
            sheet1.Cells[2, 11].Style.Font.Color.SetColor(Color.Red);

            // NO OTHER LOGIC CHANGED
            PopulateColumn(sheet1, [.. countries.Select(e => e.CountryName ?? "")], 13);
            PopulateColumn(sheet1, [.. countries.Select(e => e.CountryId.ToString())], 14);
            PopulateColumn(sheet1, [.. cities.Select(e => e.CityName ?? "".ToString())], 16);
            PopulateColumn(sheet1, [.. cities.Select(e => e.CityId.ToString())], 17);
            PopulateColumn(sheet1, [.. cities.Select(e => e.CountryId.ToString() ?? "")], 18);
            //PopulateColumn(sheet1, [.. entities.Select(e => e.EntityName ?? "".ToString())], 22);
            //PopulateColumn(sheet1, [.. entities.Select(e => e.TenantId.ToString())], 23);

            ApplyDropdown(sheet1, "CountryNameRange", "B", 13, 100);

            DisableParentEntityForIsChild(sheet1, "G", "H", 100);

            ApplyDropdown(sheet1, "IsChildRange", "G", 20, 100);

            //PopulateColumn(sheet1, [.. entities.Select(e => e.EntityName ?? "".ToString())], 22);

            //sheet1.Workbook.Names.Add("ParentEntityRange", sheet1.Cells[2, 22, entities.Count + 1, 22]);

            AddFormula(sheet1, "C", "B", 13, 14, countries.Count);

            ApplyParamValueDropdown(sheet1, cities);

            AddFormula(sheet1, "E", "D", 16, 17, cities.Count);
            //AddFormula(sheet1, "I", "H", 22, 23, entities.Count);
        }

        /// <summary>
        /// Populates the lists template worksheet.
        /// </summary>
        /// <param name="sheet2">The worksheet to populate.</param>
        /// <param name="entities">List of entities.</param>
        public void ListsTemplate(ExcelWorksheet sheet2)
        {
            // Defines list header columns
            string[] headers = ["ListName*"];
            // Populates header row
            for (int i = 0; i < headers.Length; i++)
            {
                sheet2.Cells[1, i + 1].Value = headers[i];
            }

            // Add description for required fields
            sheet2.Cells[2, 5].Value = Symbol;
            sheet2.Cells[1, 5].Value = "Field Description";

            // Makes required field indicator bold
            sheet2.Cells[2, 5].Style.Font.Bold = true;

            // Makes required field indicator red for visibility
            sheet2.Cells[2, 5].Style.Font.Color.SetColor(Color.Red);

            // Sets up reference columns for data validation
            //sheet2.Cells[1, 10].Value = "EntityName";
            //sheet2.Cells[1, 11].Value = "TenantId";

            // Populates reference columns with data
            //PopulateColumn(sheet2, [.. entities.Select(e => e.EntityName ?? "")], 10);
            //PopulateColumn(sheet2, [.. entities.Select(e => e.TenantId.ToString())], 11);

            // Applies dropdown validation to EntityName column

        }
        /// <summary>
        /// Populates the list item template worksheet.
        /// </summary>
        /// <param name="sheet3">The worksheet to populate.</param>
        /// <param name="listItem">List of managed list items.</param>

        public void ListIteamTemplate(ExcelWorksheet sheet3, List<ManagedListGetModel> listItem)
        {
            string[] listHeaders = ["ListName*", "ItemName*", "ListId*"];

            for (int i = 0; i < listHeaders.Length; i++)
            {
                sheet3.Cells[1, i + 1].Value = listHeaders[i];
            }
            // Add description for required fields
            sheet3.Cells[2, 4].Value = Symbol;
            sheet3.Cells[2, 4].Style.Font.Bold = true;  // Make text bold
            sheet3.Cells[2, 4].Style.Font.Color.SetColor(Color.Red); // Make text red for visibility
            //List page
            sheet3.Cells[1, 11].Value = "ListName";
            sheet3.Cells[1, 12].Value = "ListIds";

            //List page
            PopulateColumn(sheet3, [.. listItem.Select(e => e.ListName ?? "")], 11);
            PopulateColumn(sheet3, [.. listItem.Select(e => e.ListId.ToString())], 12);

            //List page
            ApplyDropdown(sheet3, "ListNameRange", "A", 11, 100);

            //List page
            AddFormula(sheet3, "C", "A", 11, 12, 100);
        }

        /// <summary>
        /// Populates the customer parameter template worksheet.
        /// </summary>
        /// <param name="sheet4">The worksheet to populate.</param>
        /// <param name="dataTypes">List of data types.</param>
        /// <param name="conditions">List of conditions.</param>
        /// <param name="entities">List of entities.</param>

        public void CustomerParameterTemplate(ExcelWorksheet sheet4, List<DataTypeModel> dataTypes, List<ConditionModel> conditions/*, List<EntityModel> entities*/)
        {
            // Sanitize DataType names
            dataTypes.ForEach(dataType => dataType.DataTypeName = SanitizeRangeName(dataType.DataTypeName ?? ""));

            string[] headers = ["ParameterName*", "ParameterType*", "ParameterTypeId*", "Factor*", "FactorOrder*", "FactorOrderId*", "Conditions*", "ConditionsId*", "Entity", "TenantIds", "Field Description"];
            for (int i = 0; i < headers.Length; i++)
            {
                sheet4.Cells[1, i + 1].Value = headers[i];
            }

            // Add auxiliary columns
            sheet4.Cells[1, 16].Value = "DataTypeName";
            sheet4.Cells[1, 17].Value = "DataTypeId";
            sheet4.Cells[1, 19].Value = "ConditionValue";
            sheet4.Cells[1, 20].Value = "ConditionId";
            sheet4.Cells[1, 22].Value = "EntityName";
            sheet4.Cells[1, 23].Value = "TenantId";
            sheet4.Cells[1, 24].Value = "FactorOptions";
            sheet4.Cells[1, 25].Value = "FactorOrderValues";

            // Populate dropdown options
            string[] factorOptions = ["True", "False"];
            for (int i = 0; i < factorOptions.Length; i++)
            {
                sheet4.Cells[i + 2, 24].Value = factorOptions[i];
            }

            string[] factorOrderOptions = ["Ascending", "Descending"];
            for (int i = 0; i < factorOrderOptions.Length; i++)
            {
                sheet4.Cells[i + 2, 25].Value = factorOrderOptions[i];
            }

            // Apply conditional formatting to "FactorOrder" and "Conditions" columns
            for (int row = 2; row <= 100; row++)
            {
                // Gray out "FactorOrder" column (Column E)
                var factorOrderCell = sheet4.Cells[row, 5];
                var condition = $"$D{row}=\"False\""; // Check if Factor (Column D) is False
                var cfRule = sheet4.ConditionalFormatting.AddExpression(factorOrderCell);
                cfRule.Style.Fill.BackgroundColor.Color = Color.LightGray; // Gray color
                cfRule.Formula = condition;

                // Gray out "Conditions" column (Column G)
                var conditionsCell = sheet4.Cells[row, 7];
                var cfRule2 = sheet4.ConditionalFormatting.AddExpression(conditionsCell);
                cfRule2.Style.Fill.BackgroundColor.Color = Color.LightGray;
                cfRule2.Formula = condition;
            }

            // Add description for required fields
            sheet4.Cells[2, 11].Value = Symbol;
            sheet4.Cells[2, 11].Style.Font.Bold = true;  // Make text bold
            sheet4.Cells[2, 11].Style.Font.Color.SetColor(Color.Red); // Make text red for visibility

            // Populate auxiliary columns with data
            PopulateColumn(sheet4, [.. dataTypes.Select(d => d.DataTypeName ?? "")], 16);
            PopulateColumn(sheet4, [.. dataTypes.Select(d => d.DataTypeId.ToString())], 17);
            PopulateColumn(sheet4, [.. conditions.Select(c => c.ConditionValue ?? "")], 19);
            PopulateColumn(sheet4, [.. conditions.Select(c => c.ConditionId.ToString())], 20);
            //PopulateColumn(sheet4, [.. entities.Select(e => e.EntityName ?? "")], 22);
            //PopulateColumn(sheet4, [.. entities.Select(e => e.TenantId.ToString())], 23);

            // Apply dropdown validations
            ApplyDropdown(sheet4, "DataTypeNameRange", "B", 16, 100);
            ApplyDropdownWithCondition(sheet4, "FactorOrderRange", "E", 25, factorOrderOptions.Length, "D"); // Dependent on Factor (Column D)
            ApplyDropdownWithCondition(sheet4, "ConditionValueRange", "G", 19, conditions.Count, "D");    // Dependent on Factor (Column D)
            ApplyDropdown(sheet4, "FactorRange", "D", 24, 100);
            ApplyDropdown(sheet4, "EntityNameRange", "I", 22, 100);


            // Add dependent formulas
            AddFormula(sheet4, "C", "B", 16, 17, 100);
            AddFormula(sheet4, "H", "G", 19, 20, 100);
            AddFormula(sheet4, "J", "I", 22, 23, 100);

            for (int row = 2; row <= 100; row++)
            {
                sheet4.Cells[row, 6].Formula = $"IF(E{row}=\"Ascending\", \"Asc\", IF(E{row}=\"Descending\", \"Des\", \"\"))";
            }
            // Hide the specified columns
            sheet4.Column(3).Hidden = true;  // ParameterTypeId
            sheet4.Column(6).Hidden = true;  // FactorOrderId
            sheet4.Column(8).Hidden = true;  // ConditionsId
            sheet4.Column(10).Hidden = true; // TenantIds
        }
        /// <summary>
        /// Populates the product parameter template worksheet.
        /// </summary>
        /// <param name="sheet5">The worksheet to populate.</param>
        /// <param name="dataTypes">List of data types.</param>
        /// <param name="conditions">List of conditions.</param>
        /// <param name="entities">List of entities.</param>

        public void ProductParameterTemplate(ExcelWorksheet sheet5, List<DataTypeModel> dataTypes, List<ConditionModel> conditions /*List<EntityModel> entities*/)
        {
            // Sanitize DataType names
            dataTypes.ForEach(dataType => dataType.DataTypeName = SanitizeRangeName(dataType.DataTypeName ?? ""));

            string[] headers = ["ParameterName*", "ParameterType*", "ParameterTypeId*", "IsMandatory", "Field Description"];
            for (int i = 0; i < headers.Length; i++)
            {
                sheet5.Cells[1, i + 1].Value = headers[i];
            }

            // Add auxiliary columns
            sheet5.Cells[1, 16].Value = "DataTypeName";
            sheet5.Cells[1, 17].Value = "DataTypeId";
            sheet5.Cells[1, 19].Value = "ConditionValue";
            sheet5.Cells[1, 20].Value = "ConditionId";
            sheet5.Cells[1, 22].Value = "EntityName";
            sheet5.Cells[1, 23].Value = "TenantId";
            sheet5.Cells[1, 24].Value = "FactorOptions";
            sheet5.Cells[1, 25].Value = "FactorOrderValues";

            // Populate dropdown options
            string[] isMandatoryOptions = ["True", "False"];
            for (int i = 0; i < isMandatoryOptions.Length; i++)
            {
                sheet5.Cells[i + 2, 24].Value = isMandatoryOptions[i];
            }
            var mandatoryRange = sheet5.Cells[2, 24, 3, 24];
            sheet5.Workbook.Names.Add("IsMandatoryRange", mandatoryRange);

            // Add description for required fields
            sheet5.Cells[2, 5].Value = Symbol;
            sheet5.Cells[2, 5].Style.Font.Bold = true;  // Make text bold
            sheet5.Cells[2, 5].Style.Font.Color.SetColor(Color.Red); // Make text red for visibility

            // Populate auxiliary columns with data
            PopulateColumn(sheet5, [.. dataTypes.Select(d => d.DataTypeName ?? "")], 16);
            PopulateColumn(sheet5, [.. dataTypes.Select(d => d.DataTypeId.ToString())], 17);
            PopulateColumn(sheet5, [.. conditions.Select(c => c.ConditionValue ?? "")], 19);
            PopulateColumn(sheet5, [.. conditions.Select(c => c.ConditionId.ToString())], 20);
            //PopulateColumn(sheet5, [.. entities.Select(e => e.EntityName ?? "")], 22);
            //PopulateColumn(sheet5, [.. entities.Select(e => e.TenantId.ToString())], 23);

            // Apply dropdown validations
            ApplyDropdown(sheet5, "DataTypeNameRange", "B", 16, 100);
            //ApplyDropdownWithCondition(sheet5, "FactorOrderRange", "E", 25, factorOrderOptions.Length, "D"); // Dependent on Factor (Column D)
            //ApplyDropdownWithCondition(sheet5, "ConditionValueRange", "G", 19, conditions.Count, "D");    // Dependent on Factor (Column D)
            //ApplyDropdown(sheet5, "FactorRange", "D", 24, 100);
            //ApplyDropdown(sheet5, "EntityNameRange", "I", 22, 100);


            // Add dependent formulas
            AddFormula(sheet5, "C", "B", 16, 17, 100);
            //AddFormula(sheet5, "H", "G", 19, 20, 100);
            //AddFormula(sheet5, "J", "I", 22, 23, 100);
            ApplyDropdown(sheet5, "IsMandatoryRange", "D", 24, 100);

            for (int row = 2; row <= 100; row++)
            {
                sheet5.Cells[row, 6].Formula = $"IF(E{row}=\"Ascending\", \"Asc\", IF(E{row}=\"Descending\", \"Des\", \"\"))";
            }
            // Hide the specified columns
            //sheet5.Column(3).Hidden = true;  // ParameterTypeId
            //sheet5.Column(6).Hidden = true;  // FactorOrderId
            //sheet5.Column(8).Hidden = true;  // ConditionsId
            //sheet5.Column(10).Hidden = true; // TenantIds
        }
        /// <summary>
        /// Populates the factor template worksheet.
        /// </summary>
        /// <param name="sheet6">The worksheet to populate.</param>
        /// <param name="parameter">List of parameters.</param>
        /// <param name="conditionsList">List of conditions.</param>
        /// <param name="managedList">List of managed lists.</param>

        public static void FactorTemplate(
     ExcelWorksheet sheet6,
     List<ParameterListModel> parameter,
     List<ConditionModel> conditionsList,
     List<ManagedListGetModel> managedList)
        {
            // Defines the column headers for the template
            string[] headers = ["FactorName*", "Parameter*", "ParameterId*", "Condition*", "ConditionId*", "Value1*", "Value2", "Field Description"];

            // Populates the header row with defined headers
            for (int i = 0; i < headers.Length; i++)
            {
                sheet6.Cells[1, i + 1].Value = headers[i];
            }

            // Adds description for required fields
            sheet6.Cells[2, 8].Value = "* Fields marked with an asterisk are required.";
            sheet6.Cells[2, 8].Style.Font.Bold = true;
            sheet6.Cells[2, 8].Style.Font.Color.SetColor(Color.Red);

            // Adds internal reference column headers
            sheet6.Cells[1, 11].Value = "ParameterName";
            sheet6.Cells[1, 12].Value = "ParameterId";
            sheet6.Cells[1, 13].Value = "TenantId";
            sheet6.Cells[1, 14].Value = "ConditionValue";
            sheet6.Cells[1, 15].Value = "ConditionId";
            sheet6.Cells[1, 17].Value = "ListName";

            // Populates internal reference columns
            PopulateColumn(sheet6, [.. parameter.Select(d => d.ParameterName ?? "")], 11);
            PopulateColumn(sheet6, [.. parameter.Select(d => d.ParameterId.ToString())], 12);
            PopulateColumn(sheet6, [.. parameter.Select(d => d.TenantId.ToString() ?? "")], 13);
            PopulateColumn(sheet6, [.. conditionsList.Select(c => c.ConditionValue ?? "")], 14);
            PopulateColumn(sheet6, [.. conditionsList.Select(c => c.ConditionId.ToString())], 15);
            PopulateColumn(sheet6, [.. managedList.Select(c => c.ListName ?? "")], 17);

            // Applies dropdown validation
            ApplyDropdown(sheet6, "parameterRange", "B", 11, 500);
            ApplyDropdown(sheet6, "ConditionValueRange", "D", 14, 100);
            ApplyDropdown(sheet6, "ListValueRange", "G", 17, 100);

            // Adds formulas for lookup of ParameterId and ConditionId
            AddFormula(sheet6, "C", "B", 11, 12, 200);
            AddFormula(sheet6, "E", "D", 14, 15, 100);

            // Handles validation logic for Value1 column
            HandleValue1Column(sheet6, "D", "G", 100, [.. managedList.Select(c => c.ListName ?? "")]);

            // Auto-fit columns for better readability
            sheet6.Cells[sheet6.Dimension.Address].AutoFitColumns();
        }

        /// <summary>
        /// Populates the category template worksheet.
        /// </summary>
        /// <param name="sheet7">The worksheet to populate.</param>
        /// <param name="entities">List of entities.</param>

        public void CategoryTemplate(ExcelWorksheet sheet7/*, List<EntityModel> entities*/)
        {
            string[] headers = ["CategoryName*", "CatDescription*", "EntityName*", "TenantId*", "Field Description"];
            for (int i = 0; i < headers.Length; i++)
            {
                sheet7.Cells[1, i + 1].Value = headers[i];
            }

            // Add description for required fields
            sheet7.Cells[2, 5].Value = Symbol;
            sheet7.Cells[2, 5].Style.Font.Bold = true;  // Make text bold
            sheet7.Cells[2, 5].Style.Font.Color.SetColor(Color.Red); // Make text red for visibility

            sheet7.Cells[1, 10].Value = "EntityName";
            sheet7.Cells[1, 11].Value = "TenantId";

            //PopulateColumn(sheet7, [.. entities.Select(e => e.EntityName ?? "")], 10);
            //PopulateColumn(sheet7, [.. entities.Select(e => e.TenantId.ToString())], 11);

            ApplyDropdown(sheet7, "EntityNameRange", "C", 10, 100);

            AddFormula(sheet7, "D", "C", 10, 11, 100);

            sheet7.Column(4).Hidden = true;
        }
        /// <summary>
        /// Populates the information template worksheet with product data, dropdowns, and validation rules.
        /// </summary>
        /// <param name="sheet8">The worksheet to populate.</param>
        /// <param name="entities">List of entities to populate the entity dropdowns.</param>
        /// <param name="categories">List of categories to populate the category dropdowns.</param>
        public void InfoTemplate(ExcelWorksheet sheet8/*, List<EntityModel> entities*/, List<CategoryListModel> categories)
        {
            // Defines the column headers for the product information template.
            string[] headers = ["Code*", "StreamName*", "Category*", "CategoryId*", "Entity*", "TenantId*", "StreamImage", "Narrative", "Description"];

            // Populates the header row with the defined column names.
            for (int i = 0; i < headers.Length; i++)
            {
                sheet8.Cells[1, i + 1].Value = headers[i];
            }
            sheet8.Cells[1, 11].Value = "Field Description";
            // Adds a visual indicator for required fields in the description column.
            sheet8.Cells[2, 11].Value = Symbol;
            // Applies bold formatting to the required field symbol.
            sheet8.Cells[2, 11].Style.Font.Bold = true;
            // Applies red color to the symbol for high visibility.
            sheet8.Cells[2, 11].Style.Font.Color.SetColor(Color.Red);

            // Formats the first column (Code) as text to preserve leading zeros.
            sheet8.Column(1).Style.Numberformat.Format = "@";

            // Creates hidden reference columns for Entity data validation.
            sheet8.Cells[1, 13].Value = "EntityName";
            sheet8.Cells[1, 14].Value = "TenantId";
            // Creates hidden reference columns for Category data validation.
            sheet8.Cells[1, 16].Value = "CategoryName";
            sheet8.Cells[1, 17].Value = "CategoryId";

            // Populates the hidden Entity reference columns with data.
            //PopulateColumn(sheet8, [.. entities.Select(e => e.EntityName ?? "")], 13);
            //PopulateColumn(sheet8, [.. entities.Select(e => e.TenantId.ToString())], 14);
            // Populates the hidden Category reference columns with data.
            PopulateColumn(sheet8, [.. categories.Select(e => e.CategoryName ?? "".ToString())], 16);
            PopulateColumn(sheet8, [.. categories.Select(e => e.CategoryId.ToString())], 17);

            // Sets up the column to handle product image uploads or references.
            PopulateImageColumn(sheet8, 7);

            // Applies dropdown validation to the Entity column using the hidden reference data.
            ApplyDropdown(sheet8, "EntityNameRange", "E", 13, 100);
            // Applies dropdown validation to the Category column using the hidden reference data.
            ApplyDropdown(sheet8, "CategoryNameRange", "C", 16, 100);

            // Adds a formula to auto-populate TenantId based on the selected EntityName.
            //AddFormula(sheet8, "F", "E", 13, 14, entities.Count);
            //// Adds a formula to auto-populate CategoryId based on the selected CategoryName.
            //AddFormula(sheet8, "D", "C", 16, 17, entities.Count);
        }

        /// <summary>
        /// Populates the details template worksheet with product parameters and dynamic validation.
        /// </summary>
        /// <param name="sheet9">The worksheet to populate.</param>
        /// <param name="product">List of products for dropdown validation.</param>
        /// <param name="entities">List of entities for dropdown validation.</param>
        /// <param name="parameters">List of parameters for dropdown validation.</param>
        /// <param name="factors">List of factors for dynamic value dropdowns.</param>
        public void DetailsTemplate(ExcelWorksheet sheet9, List<ProductListModel> product/*, List<EntityModel> entities*/, List<ParameterListModel> parameters, List<FactorListModel> factors)
        {
            // Defines the column headers for the product details template.
            string[] headers = ["ProductName*", "ProductId*", "EntityName*", "TenantId*", "ParameterName*", "ParameterId*", "ParamValue*", "DisplayOrder*", "Category*", "Field Description"];

            // Populates the header row with the defined column names.
            for (int i = 0; i < headers.Length; i++)
            {
                sheet9.Cells[1, i + 1].Value = headers[i];
            }

            // Adds a visual indicator for required fields in the description column.
            sheet9.Cells[2, 10].Value = Symbol;
            // Applies bold formatting to the required field symbol.
            sheet9.Cells[2, 10].Style.Font.Bold = true;
            // Applies red color to the symbol for high visibility.
            sheet9.Cells[2, 10].Style.Font.Color.SetColor(Color.Red);

            // Creates hidden reference columns for Product data validation.
            sheet9.Cells[1, 13].Value = "ProductName";
            sheet9.Cells[1, 14].Value = "ProductId";
            sheet9.Cells[1, 15].Value = "ProductTenantId";
            // Creates hidden reference columns for Entity data validation.
            sheet9.Cells[1, 16].Value = "EntityName";
            sheet9.Cells[1, 17].Value = "TenantId";
            // Creates hidden reference columns for Parameter data validation.
            sheet9.Cells[1, 19].Value = "ParameterName";
            sheet9.Cells[1, 20].Value = "ParameterId";
            // Creates a hidden reference column for Category options.
            sheet9.Cells[1, 22].Value = "categoryOptions";
            // Creates hidden reference columns for Factor value data.
            sheet9.Cells[1, 24].Value = "Value1";
            sheet9.Cells[1, 25].Value = "Value2";
            sheet9.Cells[1, 26].Value = "ParameterId";

            // Defines the boolean options for the Category dropdown.
            string[] categoryOptions = ["True", "False"];
            // Populates the hidden Category options reference column.
            for (int i = 0; i < categoryOptions.Length; i++)
            {
                sheet9.Cells[i + 2, 22].Value = categoryOptions[i];
            }

            // Populates the hidden Product reference columns with data.
            PopulateColumn(sheet9, [.. product.Select(e => e.ProductName ?? "")], 13);
            PopulateColumn(sheet9, [.. product.Select(e => e.ProductId.ToString())], 14);
            PopulateColumn(sheet9, [.. product.Select(e => e.TenantId.ToString())], 15);
            // Populates the hidden Entity reference columns with data.
            //PopulateColumn(sheet9, [.. entities.Select(e => e.EntityName ?? "")], 16);
            //PopulateColumn(sheet9, [.. entities.Select(e => e.TenantId.ToString())], 17);
            // Populates the hidden Parameter reference columns with data.
            PopulateColumn(sheet9, [.. parameters.Select(e => e.ParameterName ?? "")], 19);
            PopulateColumn(sheet9, [.. parameters.Select(e => e.ParameterId.ToString())], 20);
            // Populates the hidden Factor value reference columns with data.
            PopulateColumn(sheet9, [.. factors.Select(e => e.Value1 ?? "".ToString())], 24);
            PopulateColumn(sheet9, [.. factors.Select(e => e.Value2 ?? "".ToString())], 25);
            PopulateColumn(sheet9, [.. factors.Select(e => e.ParameterId?.ToString() ?? "")], 26);

            // Applies dropdown validation to the ProductName column.
            ApplyDropdown(sheet9, "ProductNameRange", "A", 13, 100);
            // Applies dropdown validation to the EntityName column.
            ApplyDropdown(sheet9, "EntityNameRange", "C", 16, 100);
            // Applies dropdown validation to the ParameterName column.
            ApplyDropdown(sheet9, "ParameterNameRange", "E", 19, 100);
            // Applies dropdown validation to the Category column.
            ApplyDropdown(sheet9, "categoryRange", "I", 22, 100);

            // Creates a dynamic dropdown for ParamValue that filters factors based on the selected ParameterId.
            ApplyParamValueDropdownDetails(sheet9, factors);

            // Adds a formula to auto-populate ProductId based on the selected ProductName.
            AddFormula(sheet9, "B", "A", 13, 14, 100);
            // Adds a formula to auto-populate TenantId based on the selected EntityName.
            AddFormula(sheet9, "D", "C", 16, 17, 100);
            // Adds a formula to auto-populate ParameterId based on the selected ParameterName.
            AddFormula(sheet9, "F", "E", 19, 20, 100);
        }

        /// <summary>
        /// Populates the rules template worksheet with expression building components and VBA automation.
        /// </summary>
        /// <param name="sheet10">The worksheet to populate.</param>
        /// <param name="parameter">List of parameters for dropdown validation.</param>
        /// <param name="conditions">List of conditions for operator dropdowns.</param>
        /// <param name="factors">List of factors for dynamic value dropdowns.</param>
        /// 
        public static void EruleMasterRulesTemplate(ExcelWorksheet sheet10)
        {
            // Creates a VBA worksheet change event to automate expression building
            StringBuilder changeEvent = new();
            changeEvent.Append("Private Sub Worksheet_Change(ByVal Target As Range)\n")
                .Append("     If Target.Column >= 3 And Target.Column <= 8 And Target.Value <> \"\" Then\n")
                .Append("          If Target.Column = 4 Then\n")
                .Append("               Rules.Cells(Target.Row, 17).Formula = \"=IF(\"\"\" & Target.Value & \"\"\" = \"\"\"\", \"\"\"\", VLOOKUP(\"\"\" & Target.Value & \"\"\", S:T, 2, FALSE))\"\n")
                .Append("          End If\n")
                .Append("          Rules.Cells(Target.Row, 9).Value = Rules.Cells(Target.Row, 9).Value + \" \" + Target.Value\n")
                .Append("          Target.Value = \"\"\n")
                .Append("     End If\n")
                .Append("End Sub");

            // Injects the VBA code into the worksheet's module
            sheet10.CodeModule.Code = changeEvent.ToString();

            // Define headers same as DownloadTemplateEruleMaster
            string[] headers = ["RuleName*", "RuleDescription*", "IsActive*"];
            for (int i = 0; i < headers.Length; i++)
            {
                sheet10.Cells[1, i + 1].Value = headers[i];
            }

            // Set IsActive options
            string[] isActiveOptions = ["True", "False"];
            for (int i = 0; i < isActiveOptions.Length; i++)
            {
                sheet10.Cells[i + 2, 20].Value = isActiveOptions[i]; // column 20 (T)
            }

            // Field Description column
            sheet10.Cells[1, 5].Value = "Field Description";
            sheet10.Cells[2, 5].Value = "* Fields marked with an asterisk are required.";
            sheet10.Cells[2, 5].Style.Font.Bold = true;
            sheet10.Cells[2, 5].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Apply dropdown range for IsActive
            var isActiveRange = sheet10.Cells[2, 20, 3, 20];
            sheet10.Workbook.Names.Add("IsActiveRange", isActiveRange);
            ApplyDropdown(sheet10, "IsActiveRange", "C", 20, 100);
            // Auto-fit columns
            sheet10.Cells[sheet10.Dimension.Address].AutoFitColumns();
        }


        public void RulesTemplate(ExcelWorksheet sheet10, List<ParameterListModel> parameter, List<ConditionModel> conditions, List<FactorListModel> factors)
        {
            // Creates a VBA worksheet change event to automate expression building.
            StringBuilder changeEvent = new();
            changeEvent.Append("Private Sub Worksheet_Change(ByVal Target As Range)\n")
                .Append("     If Target.Column >= 3 And Target.Column <= 8 And Target.Value <> \"\" Then\n")
                .Append("          If Target.Column = 4 Then\n")
                .Append("               ERules.Cells(Target.Row, 17).Formula = \"=IF(\"\"\" & Target.Value & \"\"\" = \"\"\"\", \"\"\"\", VLOOKUP(\"\"\" & Target.Value & \"\"\", S:T, 2, FALSE))\"\n")
                .Append("          End If\n")
                .Append("          ERules.Cells(Target.Row, 9).Value = ERules.Cells(Target.Row, 9).Value + \" \" + Target.Value\n")
                .Append("          Target.Value = \"\"\n")
                .Append("     End If\n")
                .Append("End Sub");
            // Injects the VBA code into the worksheet's module.
            sheet10.CodeModule.Code = changeEvent.ToString();

            // Defines the column headers for the rules expression builder.
            string[] headers = ["RuleName*", "RuleDescription*", "OpenParenthesis", "ParameterName", "Operator", "Factor", "LogicalOperator", "CloseParenthesis"];
            // Populates the header row with the defined column names.
            for (int i = 0; i < headers.Length; i++)
            {
                sheet10.Cells[1, i + 1].Value = headers[i];
            }

            // Creates hidden auxiliary columns for data validation and reference.
            sheet10.Cells[1, 18].Value = "OpenParenthesisValue";
            sheet10.Cells[1, 19].Value = "ParameterNameValue";
            sheet10.Cells[1, 20].Value = "ParameterIdValue";
            sheet10.Cells[1, 21].Value = "ConditionValue";
            sheet10.Cells[1, 22].Value = "Value1";
            sheet10.Cells[1, 23].Value = "Value2";
            sheet10.Cells[1, 24].Value = "FactorParameterId";
            sheet10.Cells[1, 25].Value = "LogicalOperatorValue";
            sheet10.Cells[1, 26].Value = "CloseParenthesisValue";
            // Defines the column for the auto-generated expression.
            sheet10.Cells[1, 9].Value = "Expression*";
            // Defines the column for field descriptions.
            sheet10.Cells[1, 10].Value = "Field Description";
            // Defines a helper column for ParameterId.
            sheet10.Cells[1, 17].Value = "ParameterId";

            // Adds a visual indicator for required fields in the description column.
            sheet10.Cells[2, 10].Value = Symbol;
            // Applies bold formatting to the required field symbol.
            sheet10.Cells[2, 10].Style.Font.Bold = true;
            // Applies red color to the symbol for high visibility.
            sheet10.Cells[2, 10].Style.Font.Color.SetColor(Color.Red);

            // Defines the option for the Open Parenthesis dropdown.
            string[] openParenthesisOptions = ["("];
            // Populates the hidden Open Parenthesis reference column.
            for (int i = 0; i < openParenthesisOptions.Length; i++)
            {
                sheet10.Cells[i + 2, 18].Value = openParenthesisOptions[i];
            }

            // Defines the options for the Logical Operator dropdown.
            string[] logicalOperatorOptions = ["And", "Or"];
            // Populates the hidden Logical Operator reference column.
            for (int i = 0; i < logicalOperatorOptions.Length; i++)
            {
                sheet10.Cells[i + 2, 25].Value = logicalOperatorOptions[i];
            }

            // Defines the option for the Close Parenthesis dropdown.
            string[] CloseParenthesisOptions = [")"];
            // Populates the hidden Close Parenthesis reference column.
            for (int i = 0; i < CloseParenthesisOptions.Length; i++)
            {
                sheet10.Cells[i + 2, 26].Value = CloseParenthesisOptions[i];
            }

            // Populates the hidden Parameter reference columns with data.
            PopulateColumn(sheet10, [.. parameter.Select(d => d.ParameterName ?? "")], 19);
            PopulateColumn(sheet10, [.. parameter.Select(d => d.ParameterId.ToString())], 20);
            // Populates the hidden Condition reference column with data.
            PopulateColumn(sheet10, [.. conditions.Select(c => c.ConditionValue ?? "")], 21);
            // Populates the hidden Factor value reference columns with data.
            PopulateColumn(sheet10, [.. factors.Select(c => c.Value1 ?? "".ToString())], 22);
            PopulateColumn(sheet10, [.. factors.Select(c => c.Value2 ?? "".ToString())], 23);
            PopulateColumn(sheet10, [.. factors.Select(c => c.ParameterId.ToString() ?? "")], 24);

            // Applies dropdown validation to the Open Parenthesis column.
            ApplyDropdownrule(sheet10, "OpenParenthesisRange", "C", 18, openParenthesisOptions.Length);
            // Applies dropdown validation to the ParameterName column.
            ApplyDropdownrule(sheet10, "ParameterNameRange", "D", 19, parameter.Count);
            // Applies dropdown validation to the Operator (Condition) column.
            ApplyDropdownrule(sheet10, "ConditionRange", "E", 21, conditions.Count);
            // Applies dropdown validation to the Logical Operator column.
            ApplyDropdownrule(sheet10, "logicalOperatorRange", "G", 25, logicalOperatorOptions.Length);
            // Applies dropdown validation to the Close Parenthesis column.
            ApplyDropdownrule(sheet10, "CloseParenthesisRange", "H", 26, CloseParenthesisOptions.Length);
            // Adds a formula to auto-populate ParameterId based on the selected ParameterName.
            AddFormula(sheet10, "Q", "D", 19, 20, parameter.Count);
            // Creates a dynamic dropdown for Factor values that filters based on the selected Parameter.
            ApplyFactorValueDropdown(sheet10, factors);
        }

        /// <summary>
        /// Populates the eCards template worksheet with rule-based card definitions and VBA automation.
        /// </summary>
        /// <param name="sheet11">The worksheet to populate.</param>
        /// <param name="erule">List of eRules for dropdown validation.</param>
        public void ECardsTemplate(ExcelWorksheet sheet11, List<EruleListModel> erule)
        {
            // Creates a VBA worksheet change event to automate expression building for eCards.
            StringBuilder changeEvent = new();
            changeEvent.Append("Private Sub Worksheet_Change(ByVal Target As Range)\n")
                .Append("     If Target.Column >= 3 And Target.Column <= 6 And Target.Value <> \"\" Then\n")
                .Append("          ECards.Cells(Target.Row, 7).Value = ECards.Cells(Target.Row, 7).Value + \" \" + Target.Value\n")
                .Append("          Target.Value = \"\"\n")
                .Append("     End If\n")
                .Append("End Sub");
            // Injects the VBA code into the worksheet's module.
            sheet11.CodeModule.Code = changeEvent.ToString();

            // Defines the column headers for the eCards expression builder.
            string[] headers = ["CardName*", "CardDescription*", "OpenParenthesis", "RuleName", "Function", "CloseParenthesis"];
            // Populates the header row with the defined column names.
            for (int i = 0; i < headers.Length; i++)
            {
                sheet11.Cells[1, i + 1].Value = headers[i];
            }

            // Defines the column for the auto-generated expression.
            sheet11.Cells[1, 7].Value = "Expression*";
            // Defines the column for field descriptions.
            sheet11.Cells[1, 8].Value = "Field Description";
            // Creates hidden auxiliary columns for data validation.
            sheet11.Cells[1, 11].Value = "OpenParenthesisValue";
            sheet11.Cells[1, 12].Value = "EruleName";
            sheet11.Cells[1, 13].Value = "EruleId";
            sheet11.Cells[1, 15].Value = "LogicalOperatorValue";
            sheet11.Cells[1, 17].Value = "CloseParenthesisValue";

            // Adds a visual indicator for required fields in the description column.
            sheet11.Cells[2, 8].Value = Symbol;
            // Applies bold formatting to the required field symbol.
            sheet11.Cells[2, 8].Style.Font.Bold = true;
            // Applies red color to the symbol for high visibility.
            sheet11.Cells[2, 8].Style.Font.Color.SetColor(Color.Red);

            // Defines the option for the Open Parenthesis dropdown.
            string[] openParenthesisOptions = ["("];
            // Populates the hidden Open Parenthesis reference column.
            for (int i = 0; i < openParenthesisOptions.Length; i++)
            {
                sheet11.Cells[i + 2, 11].Value = openParenthesisOptions[i];
            }

            // Defines the options for the Logical Operator (Function) dropdown.
            string[] logicalOperatorOptions = ["And", "Or"];
            // Populates the hidden Logical Operator reference column.
            for (int i = 0; i < logicalOperatorOptions.Length; i++)
            {
                sheet11.Cells[i + 2, 15].Value = logicalOperatorOptions[i];
            }

            // Defines the option for the Close Parenthesis dropdown.
            string[] CloseParenthesisOptions = [")"];
            // Populates the hidden Close Parenthesis reference column.
            for (int i = 0; i < CloseParenthesisOptions.Length; i++)
            {
                sheet11.Cells[i + 2, 17].Value = CloseParenthesisOptions[i];
            }

            // Populates the hidden eRule ID reference column with data.
            PopulateColumn(sheet11, [.. erule.Select(d => d.EruleId.ToString())], 13);

            // Applies dropdown validation to the Open Parenthesis column.
            ApplyDropdownrule(sheet11, "OpenParenthesisRange", "C", 11, openParenthesisOptions.Length);
            // Applies dropdown validation to the RuleName column.
            ApplyDropdownrule(sheet11, "erulesNameRange", "D", 12, erule.Count);
            // Applies dropdown validation to the Function (Logical Operator) column.
            ApplyDropdownrule(sheet11, "logicalOperatorRange", "E", 15, logicalOperatorOptions.Length);
            // Applies dropdown validation to the Close Parenthesis column.
            ApplyDropdownrule(sheet11, "CloseParenthesisRange", "F", 17, CloseParenthesisOptions.Length);
        }


        public static void ECardsTemplates(ExcelWorksheet sheet11, List<EruleListModel> erule)
        {
            // Create VBA worksheet change event
            StringBuilder changeEvent = new();
            changeEvent.Append("Private Sub Worksheet_Change(ByVal Target As Range)\n")
                .Append("     If Target.Column >= 3 And Target.Column <= 6 And Target.Value <> \"\" Then\n")
                .Append("          ECards.Cells(Target.Row, 7).Value = ECards.Cells(Target.Row, 7).Value + \" \" + Target.Value\n")
                .Append("          Target.Value = \"\"\n")
                .Append("     End If\n")
                .Append("End Sub");

            sheet11.CodeModule.Code = changeEvent.ToString();


            string[] headers = ["CardName*", "CardDescription*", "ExpressionShown*"];
            for (int i = 0; i < headers.Length; i++)
            {
                sheet11.Cells[1, i + 1].Value = headers[i];
            }

            // Field Description
            sheet11.Cells[1, 5].Value = "Field Description";

            // Hidden supporting columns
            sheet11.Cells[1, 11].Value = "OpenParenthesisValue";
            sheet11.Cells[1, 12].Value = "EruleName";
            sheet11.Cells[1, 13].Value = "EruleId";
            sheet11.Cells[1, 15].Value = "LogicalOperatorValue";
            sheet11.Cells[1, 17].Value = "CloseParenthesisValue";

            // Required fields description
            sheet11.Cells[2, 5].Value = "* Fields marked with an asterisk are required.";
            sheet11.Cells[2, 5].Style.Font.Bold = true;
            sheet11.Cells[2, 5].Style.Font.Color.SetColor(Color.Red);


            string[] openParenthesisOptions = ["("];
            for (int i = 0; i < openParenthesisOptions.Length; i++)
                sheet11.Cells[i + 2, 11].Value = openParenthesisOptions[i];

            string[] logicalOperatorOptions = ["And", "Or"];
            for (int i = 0; i < logicalOperatorOptions.Length; i++)
                sheet11.Cells[i + 2, 15].Value = logicalOperatorOptions[i];

            string[] closeParenthesisOptions = [")"];
            for (int i = 0; i < closeParenthesisOptions.Length; i++)
                sheet11.Cells[i + 2, 17].Value = closeParenthesisOptions[i];

            PopulateColumn(sheet11, [.. erule.Select(d => d.EruleName!.ToString())], 12);

            // (Dropdown assignment commented out in your DownloadTemplate, so kept same)
            //ApplyDropdown(sheet11, "OpenParenthesisRange", "C", 11, openParenthesisOptions.Length);
            //ApplyDropdown(sheet11, "erulesNameRange", "D", 12, erule.Count);
            //ApplyDropdown(sheet11, "logicalOperatorRange", "E", 15, logicalOperatorOptions.Length);
            //ApplyDropdown(sheet11, "CloseParenthesisRange", "F", 17, closeParenthesisOptions.Length);

            // Auto-fit columns
            sheet11.Cells[sheet11.Dimension.Address].AutoFitColumns();
        }

        /// <summary>
        /// Populates the PCards template worksheet with product-card relationships and expression building logic.
        /// </summary>
        /// <param name="sheet12">The worksheet to populate.</param>
        /// <param name="ecard">List of eCards for dropdown validation.</param>
        /// <param name="product">List of products for dropdown validation.</param>
        public void PCardsTemplate(ExcelWorksheet sheet12, List<EcardListModel> ecard, List<ProductListModel> product)
        {
            // Creates a VBA worksheet change event to automate expression building for product cards.
            StringBuilder changeEvent = new();
            changeEvent.Append("Private Sub Worksheet_Change(ByVal Target As Range)\n")
                .Append("     If Target.Column >= 5 And Target.Column <= 8 And Target.Value <> \"\" Then\n")
                .Append("          PCards.Cells(Target.Row, 9).Value = PCards.Cells(Target.Row, 9).Value + \" \" + Target.Value\n")
                .Append("          Target.Value = \"\"\n")
                .Append("     End If\n")
                .Append("End Sub");
            // Injects the VBA code into the worksheet's module to handle expression concatenation.
            sheet12.CodeModule.Code = changeEvent.ToString();

            // Defines the column headers for the product cards template.
            string[] headers = ["Product Card Name*", "Description*", "Product", "ProductId", "Open Parenthesis", "Cards", "Function", "CloseParenthesis"];
            // Populates the header row with the defined column names.
            for (int i = 0; i < headers.Length; i++)
            {
                sheet12.Cells[1, i + 1].Value = headers[i];
            }

            // Defines the column for the auto-generated expression.
            sheet12.Cells[1, 9].Value = "Expression*";
            // Defines the column for field descriptions and validation indicators.
            sheet12.Cells[1, 10].Value = "Field Description";
            // Creates hidden auxiliary columns for Product data validation.
            sheet12.Cells[1, 12].Value = "ProductName";
            sheet12.Cells[1, 13].Value = "ProductIdValue";
            // Creates hidden auxiliary columns for expression component validation.
            sheet12.Cells[1, 15].Value = "OpenParenthesisValue";
            sheet12.Cells[1, 17].Value = "ECardName";
            sheet12.Cells[1, 18].Value = "ECardId";
            sheet12.Cells[1, 20].Value = "LogicalOperatorValue";
            sheet12.Cells[1, 22].Value = "CloseParenthesisValue";

            // Adds a visual indicator for required fields in the description column.
            sheet12.Cells[2, 10].Value = Symbol;
            // Applies bold formatting to the required field symbol.
            sheet12.Cells[2, 10].Style.Font.Bold = true;
            // Applies red color to the symbol for high visibility.
            sheet12.Cells[2, 10].Style.Font.Color.SetColor(Color.Red);

            // Defines the option for the Open Parenthesis dropdown.
            string[] openParenthesisOptions = ["("];
            // Populates the hidden Open Parenthesis reference column.
            for (int i = 0; i < openParenthesisOptions.Length; i++)
            {
                sheet12.Cells[i + 2, 15].Value = openParenthesisOptions[i];
            }

            // Defines the options for the Logical Operator (Function) dropdown.
            string[] logicalOperatorOptions = ["And", "Or"];
            // Populates the hidden Logical Operator reference column.
            for (int i = 0; i < logicalOperatorOptions.Length; i++)
            {
                sheet12.Cells[i + 2, 20].Value = logicalOperatorOptions[i];
            }

            // Defines the option for the Close Parenthesis dropdown.
            string[] CloseParenthesisOptions = [")"];
            // Populates the hidden Close Parenthesis reference column.
            for (int i = 0; i < CloseParenthesisOptions.Length; i++)
            {
                sheet12.Cells[i + 2, 22].Value = CloseParenthesisOptions[i];
            }

            // Populates the hidden Product reference columns with data.
            PopulateColumn(sheet12, [.. product.Select(d => d.ProductName ?? "".ToString())], 12);
            PopulateColumn(sheet12, [.. product.Select(d => d.ProductId.ToString())], 13);
            // Populates the hidden eCard reference columns with data.
            PopulateColumn(sheet12, [.. ecard.Select(d => d.EcardName ?? "")], 17);
            PopulateColumn(sheet12, [.. ecard.Select(d => d.EcardId.ToString())], 18);

            // Applies dropdown validation to the Product column.
            ApplyDropdownrule(sheet12, "ProductNameRange", "C", 12, product.Count);
            // Applies dropdown validation to the Open Parenthesis column.
            ApplyDropdownrule(sheet12, "OpenParenthesisRange", "E", 15, openParenthesisOptions.Length);
            // Applies dropdown validation to the Cards (eCard) column.
            ApplyDropdownrule(sheet12, "ecardNameRange", "F", 17, ecard.Count);
            // Applies dropdown validation to the Function (Logical Operator) column.
            ApplyDropdownrule(sheet12, "logicalOperatorRange", "G", 20, logicalOperatorOptions.Length);
            // Applies dropdown validation to the Close Parenthesis column.
            ApplyDropdownrule(sheet12, "CloseParenthesisRange", "H", 22, CloseParenthesisOptions.Length);

            // Adds a formula to auto-populate ProductId based on the selected ProductName.
            AddFormula(sheet12, "D", "C", 12, 13, 100);
        }
        public static void PCardsTemplates(ExcelWorksheet sheet12, List<EcardListModel> ecard, List<ProductListModel> product)
        {
            // Create VBA worksheet change event (same logic as ECards)
            StringBuilder changeEvent = new();
            changeEvent.Append("Private Sub Worksheet_Change(ByVal Target As Range)\n")
                .Append("     If Target.Column >= 3 And Target.Column <= 6 And Target.Value <> \"\" Then\n")
                .Append("          PCards.Cells(Target.Row, 7).Value = PCards.Cells(Target.Row, 7).Value + \" \" + Target.Value\n")
                .Append("          Target.Value = \"\"\n")
                .Append("     End If\n")
                .Append("End Sub");

            sheet12.CodeModule.Code = changeEvent.ToString();

            string[] headers = [
                "StreamCardName*",
        "StreamCardDescription*",
        "StreamName*",
        "StreamId*",
        "ExpressionShown*"
            ];

            for (int i = 0; i < headers.Length; i++)
            {
                sheet12.Cells[1, i + 1].Value = headers[i];
            }

            // Field Description
            sheet12.Cells[1, 6].Value = "Field Description";

            // Hidden supporting columns
            sheet12.Cells[1, 10].Value = "OpenParenthesisValue";
            sheet12.Cells[1, 11].Value = "ECardName";

            sheet12.Cells[1, 12].Value = "StreamName";
            sheet12.Cells[1, 13].Value = "StreamId";
            sheet12.Cells[1, 15].Value = "LogicalOperatorValue";
            sheet12.Cells[1, 17].Value = "CloseParenthesisValue";

            // Required fields description
            sheet12.Cells[2, 6].Value = "* Fields marked with an asterisk are required.";
            sheet12.Cells[2, 6].Style.Font.Bold = true;
            sheet12.Cells[2, 6].Style.Font.Color.SetColor(Color.Red);

            // Dropdown supporting values
            string[] openParenthesisOptions = ["("];
            for (int i = 0; i < openParenthesisOptions.Length; i++)
                sheet12.Cells[i + 2, 10].Value = openParenthesisOptions[i];

            string[] logicalOperatorOptions = ["And", "Or"];
            for (int i = 0; i < logicalOperatorOptions.Length; i++)
                sheet12.Cells[i + 2, 15].Value = logicalOperatorOptions[i];

            string[] closeParenthesisOptions = [")"];
            for (int i = 0; i < closeParenthesisOptions.Length; i++)
                sheet12.Cells[i + 2, 17].Value = closeParenthesisOptions[i];

            // Populate ERules list in hidden column
            PopulateColumn(sheet12, [.. ecard.Select(d => d.EcardName!.ToString())], 11);
            PopulateColumn(sheet12, [.. product.Select(d => d.ProductName ?? "".ToString())], 12);
            PopulateColumn(sheet12, [.. product.Select(d => d.ProductId.ToString())], 13);
            // Populates the hidden eCard reference columns with data.

            // (Dropdown assignment commented out same as ECard)
            //ApplyDropdown(sheet11, "OpenParenthesisRange", "C", 11, openParenthesisOptions.Length);
            ApplyDropdown(sheet12, "ecardNameRange", "C", 12, ecard.Count);
            AddFormula(sheet12, "D", "C", 12, 13, 100);

            //ApplyDropdown(sheet11, "logicalOperatorRange", "E", 15, logicalOperatorOptions.Length);
            //ApplyDropdown(sheet11, "CloseParenthesisRange", "F", 17, closeParenthesisOptions.Length);

            // Auto-fit columns
            sheet12.Cells[sheet12.Dimension.Address].AutoFitColumns();
        }

        /// <summary>
        /// Creates and applies dynamic dropdown validation for Factor values based on the selected Parameter.
        /// </summary>
        /// <param name="sheet10">The worksheet to modify.</param>
        /// <param name="factors">List of factors providing the dropdown values.</param>
        private static void ApplyFactorValueDropdown(ExcelWorksheet sheet10, List<FactorListModel> factors)
        {
            // Gets the workbook to create named ranges.
            var workbook = sheet10.Workbook;
            // Initializes a dictionary to map Parameter IDs to their corresponding factor values.
            var paramValueMap = new Dictionary<int, List<string>>();

            // Step 1: Map ParameterId to combined Value1 and Value2 for dropdown
            // Iterates through all factors to build the parameter-to-values mapping.
            foreach (var factor in factors)
            {
                // Retrieves the Parameter ID for the current factor.
                int paramId = (int)factor.ParameterId!;
                // Combines Value1 and Value2 into a single display string, handling empty Value2.
                string valueCombination = factor.Value2 == "" ? factor.Value1! : $"{factor.Value1} + {factor.Value2}";

                // Checks if the Parameter ID already exists in the dictionary.
                if (!paramValueMap.TryGetValue(paramId, out List<string>? value))
                {
                    // Creates a new list for the Parameter ID if it doesn't exist.
                    paramValueMap[paramId] = [valueCombination];
                }
                else
                {
                    value.Add(valueCombination);
                }
            }

            // Step 2: Store Factor values in hidden columns
            // Starts at a high column index (30) to store lists of values out of sight.
            int startColumn = 30;
            // Iterates through each Parameter ID and its list of values in the map.
            foreach (var param in paramValueMap)
            {
                // Creates a unique name for the named range for this Parameter ID.
                string rangeName = $"Fact_{param.Key}";
                // Starts populating data from row 2 (skipping the header).
                int rowIndex = 2;

                // Populates the hidden column with each value for the current Parameter ID.
                foreach (var value in param.Value)
                {
                    sheet10.Cells[rowIndex, startColumn].Value = value;
                    rowIndex++;
                }

                // Defines the Excel range containing the values for this Parameter ID.
                var range = sheet10.Cells[2, startColumn, rowIndex - 1, startColumn];
                // Creates a named range that can be referenced by data validation.
                workbook.Names.Add(rangeName, range);

                // Moves to the next hidden column for the next Parameter ID's values.
                startColumn++;
            }

            // Step 3: Apply dynamic dropdown using INDIRECT()
            // Applies dynamic data validation to rows 2 through 100.
            for (int row = 2; row <= 100; row++) // Assume up to 100 rows for now
            {
                // Adds list validation to the Factor column (F) for the current row.
                var validation = sheet10.DataValidations.AddListValidation($"F{row}"); // FactorValue column

                // Sets the formula to dynamically reference the named range based on the ParameterId in column Q.
                validation.Formula.ExcelFormula = $"INDIRECT(\"Fact_\"&Q{row})"; // Q column = ParameterId
                                                                                 // Configures the validation to show an error message.
                validation.ShowErrorMessage = true;
                // Sets the error style to prevent invalid entries.
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                // Sets the title for the error dialog box.
                validation.ErrorTitle = "Invalid Selection";
                // Sets the message displayed when an invalid selection is made.
                validation.Error = "Please select a valid ParamValue.";
            }
        }

        /// <summary>
        /// Creates and applies dynamic dropdown validation for Parameter values in the details worksheet.
        /// </summary>
        /// <param name="sheet9">The worksheet to modify.</param>
        /// <param name="factors">List of factors providing the dropdown values.</param>
        private static void ApplyParamValueDropdownDetails(ExcelWorksheet sheet9, List<FactorListModel> factors)
        {
            // Gets the workbook to create named ranges.
            var workbook = sheet9.Workbook;
            // Initializes a dictionary to map Parameter IDs to their corresponding factor values.
            var paramValueMap = new Dictionary<int, List<string>>();

            // Step 1: Map ParameterId to combined Value1 and Value2 for dropdown
            // Iterates through all factors to build the parameter-to-values mapping.
            foreach (var factor in factors)
            {
                // Retrieves the Parameter ID for the current factor.
                int paramId = (int)factor.ParameterId!;
                // Combines Value1 and Value2 into a single display string, handling empty Value2.
                string valueCombination = factor.Value2 == "" ? factor.Value1! : $"{factor.Value1} + {factor.Value2}";

                // Checks if the Parameter ID already exists in the dictionary.
                if (!paramValueMap.TryGetValue(paramId, out List<string>? value))
                {
                    // Creates a new list for the Parameter ID if it doesn't exist.
                    paramValueMap[paramId] = [valueCombination];
                }
                else
                {
                    value.Add(valueCombination);
                }
            }

            // Step 2: Store param values in hidden columns
            // Starts at a high column index (30) to store lists of values out of sight.
            int startColumn = 30;
            // Iterates through each Parameter ID and its list of values in the map.
            foreach (var param in paramValueMap)
            {
                // Creates a unique name for the named range for this Parameter ID.
                string rangeName = $"Param_{param.Key}";
                // Starts populating data from row 2 (skipping the header).
                int rowIndex = 2;

                // Populates the hidden column with each value for the current Parameter ID.
                foreach (var value in param.Value)
                {
                    sheet9.Cells[rowIndex, startColumn].Value = value;
                    rowIndex++;
                }

                // Defines the Excel range containing the values for this Parameter ID.
                var range = sheet9.Cells[2, startColumn, rowIndex - 1, startColumn];
                // Creates a named range that can be referenced by data validation.
                workbook.Names.Add(rangeName, range);

                // Moves to the next hidden column for the next Parameter ID's values.
                startColumn++;
            }

            // Step 3: Apply dynamic dropdown using INDIRECT()
            // Applies dynamic data validation to rows 2 through 100.
            for (int row = 2; row <= 100; row++) // Assume up to 100 rows for now
            {
                // Adds list validation to the ParamValue column (G) for the current row.
                var validation = sheet9.DataValidations.AddListValidation($"G{row}"); // ParamValue column

                // Sets the formula to dynamically reference the named range based on the ParameterId in column F.
                validation.Formula.ExcelFormula = $"INDIRECT(\"Param_\"&F{row})"; // F column = ParameterId
                                                                                  // Configures the validation to show an error message.
                validation.ShowErrorMessage = true;
                // Sets the error style to prevent invalid entries.
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                // Sets the title for the error dialog box.
                validation.ErrorTitle = "Invalid Selection";
                // Sets the message displayed when an invalid selection is made.
                validation.Error = "Please select a valid ParamValue.";
            }
        }
        /// <summary>
        /// Populates an image column in the worksheet with base64-encoded images.
        /// </summary>
        /// <param name="sheet8">The worksheet to modify.</param>
        /// <param name="columnIndex">The index of the column.</param>
        private static void PopulateImageColumn(ExcelWorksheet sheet8, int columnIndex)
        {
            // Iterates through each row in the worksheet starting from row 2
            for (int i = 2; i <= sheet8.Dimension.End.Row; i++)
            {
                // Retrieves the image URL from the current cell
                var imageUrl = sheet8.Cells[i, columnIndex].Text;
                // Processes only non-empty URLs
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    // Converts the image URL to base64 encoding
                    var base64Image = ConvertImageUrlToBase64(imageUrl);
                    // Sets the cell value to the base64-encoded image string
                    sheet8.Cells[i, columnIndex].Value = base64Image;
                }
            }
        }

        /// <summary>
        /// Converts an image URL to a base64 string.
        /// </summary>
        /// <param name="imageUrl">The image URL.</param>
        /// <returns>The base64-encoded string of the image.</returns>
        private static string ConvertImageUrlToBase64(string imageUrl)
        {
            // Creates HttpClient instance for downloading image data
            using var httpClient = new HttpClient();
            // Downloads image bytes from the provided URL
            var imageBytes = httpClient.GetByteArrayAsync(imageUrl).Result;
            // Converts byte array to Base64 string format
            return Convert.ToBase64String(imageBytes);
        }

        /// <summary>
        /// Handles the Value1 column for managed lists and applies dropdowns based on conditions.
        /// </summary>
        /// <param name="sheet6">The worksheet to modify.</param>
        /// <param name="conditionColumn">The condition column.</param>
        /// <param name="value1Column">The Value1 column.</param>
        /// <param name="rowCount">The number of rows to process.</param>
        /// <param name="managedList">The list of managed values.</param>
        private static void HandleValue1Column(ExcelWorksheet sheet6, string conditionColumn, string value1Column, int rowCount, List<string> managedList)
        {
            // Defines named range reference for dropdown values
            string managedListRange = "ListValueRange";
            // Specifies blank cell reference for empty dropdowns
            string blankCell = "Z1";

            // Ensures blank cell contains no value
            sheet6.Cells[blankCell].Value = "";

            // Processes each row from row 2 to specified row count
            for (int row = 2; row <= rowCount + 1; row++)
            {
                // Constructs cell address for Value1 column
                string value1Cell = $"{value1Column}{row}";
                // Constructs cell address for condition column
                string conditionCell = $"{conditionColumn}{row}";

                // Retrieves existing validation for the Value1 cell
                var existingValidation = sheet6.DataValidations[value1Cell];
                // Removes existing validation if present
                if (existingValidation != null)
                {
                    sheet6.DataValidations.Remove(existingValidation);
                }

                // Creates new list validation for Value1 cell
                var value1Validation = sheet6.DataValidations.AddListValidation(value1Cell);

                // Applies validation based on managed list content
                if (managedList.Count != 0)
                {
                    // Sets formula to show managed list when condition is met
                    value1Validation.Formula.ExcelFormula = $"IF(OR(${conditionCell}=\"In List\", ${conditionCell}=\"Not In List\"), {managedListRange}, \"\")";
                }
                else
                {
                    // Sets formula to show blank dropdown when condition is met
                    value1Validation.Formula.ExcelFormula = $"IF(OR(${conditionCell}=\"In List\", ${conditionCell}=\"Not In List\"), {blankCell}, \"\")";
                }

                // Configures validation error message settings
                value1Validation.ShowErrorMessage = true;
                value1Validation.ErrorTitle = "Invalid Selection";
                value1Validation.Error = "Please select a valid list value.";
            }

            // Disables Value2 column based on conditions
            DisableValue2ForRange(sheet6, "D", "G", 100);
        }

        /// <summary>
        /// Disables the Value2 column for rows where the condition is not met.
        /// </summary>
        /// <param name="sheet6">The worksheet to modify.</param>
        /// <param name="conditionColumn">The condition column.</param>
        /// <param name="targetColumn">The target column to disable.</param>
        /// <param name="rowCount">The number of rows to process.</param>
        private static void DisableValue2ForRange(ExcelWorksheet sheet6, string conditionColumn, string targetColumn, int rowCount)
        {
            // Processes each row from row 2 to specified row count
            for (int row = 2; row <= rowCount + 1; row++)
            {
                // Constructs condition cell address
                string conditionCell = $"{conditionColumn}{row}";
                // Constructs target cell address
                string targetCell = $"{targetColumn}{row}";

                // Retrieves existing validation for target cell
                var existingValidation = sheet6.DataValidations[targetCell];
                // Removes existing validation if present
                if (existingValidation != null)
                {
                    sheet6.DataValidations.Remove(existingValidation);
                }

                // Adds conditional formatting to visually disable the cell
                var conditionFormatting = sheet6.ConditionalFormatting.AddExpression(sheet6.Cells[row, targetColumn[0] - 'A' + 1]);
                // Sets conditional formatting formula
                conditionFormatting.Formula = $"NOT(${conditionCell}=\"Range\")";
                // Configures visual styling for disabled state
                conditionFormatting.Style.Fill.PatternType = ExcelFillStyle.Solid;
                conditionFormatting.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                conditionFormatting.Style.Font.Color.SetColor(Color.DarkGray);

                // Adds custom validation to restrict input
                var validation = sheet6.DataValidations.AddCustomValidation(targetCell);
                // Sets validation formula to allow input only when condition is "Range"
                validation.Formula.ExcelFormula = $"IF(${conditionCell}=\"Range\",TRUE,FALSE)";
                // Configures validation error message settings
                validation.ShowErrorMessage = true;
                validation.ErrorTitle = "Invalid Input";
                validation.Error = "Input is not allowed unless the condition is 'Range'.";
            }
        }

        /// <summary>
        /// Applies a dropdown with a condition to a worksheet column.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="rangeName">The name of the range for the dropdown.</param>
        /// <param name="column">The column to apply the dropdown to.</param>
        /// <param name="dataColumnIndex">The data column index.</param>
        /// <param name="dataCount">The number of data items.</param>
        /// <param name="dependentColumn">The dependent column for the condition.</param>
        private static void ApplyDropdownWithCondition(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int dataCount, string dependentColumn)
        {
            // Defines the range containing dropdown values
            var range = sheet.Cells[2, dataColumnIndex, dataCount + 1, dataColumnIndex];
            // Creates named range for dropdown reference
            sheet.Workbook.Names.Add(rangeName, range);

            // Applies dropdown validation to rows 2 through 100
            for (int row = 2; row <= 100; row++)
            {
                // Adds list validation to current cell
                var validation = sheet.DataValidations.AddListValidation($"{column}{row}");
                // Sets conditional formula to disable dropdown when Factor is False
                validation.Formula.ExcelFormula = $"IF(${dependentColumn}{row}=\"False\", \"\", {rangeName})";
                // Configures validation error settings
                validation.ShowErrorMessage = true;
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                validation.ErrorTitle = "Invalid Selection";
                validation.Error = "This field is disabled because Factor is set to False.";
            }
        }

        /// <summary>
        /// Disables the parent entity column if the entity is not a child.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="conditionColumn">The condition column.</param>
        /// <param name="targetColumn">The target column to disable.</param>
        /// <param name="rowCount">The number of rows to process.</param>
        private static void DisableParentEntityForIsChild(ExcelWorksheet sheet, string conditionColumn, string targetColumn, int rowCount)
        {
            // Processes each row from row 2 to specified row count
            for (int row = 2; row <= rowCount + 1; row++)
            {
                // Constructs condition cell address
                string conditionCell = $"{conditionColumn}{row}";
                // Constructs target cell address
                string targetCell = $"{targetColumn}{row}";

                // Adds conditional formatting to visually indicate disabled state
                var conditionFormatting = sheet.ConditionalFormatting.AddExpression(sheet.Cells[targetCell]);
                // Sets formatting formula based on condition
                conditionFormatting.Formula = $"NOT(${conditionCell}=\"True\")";
                // Configures visual styling for disabled cells
                conditionFormatting.Style.Fill.PatternType = ExcelFillStyle.Solid;
                conditionFormatting.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                conditionFormatting.Style.Font.Color.SetColor(Color.DarkGray);

                // Adds list validation with conditional dropdown
                var validation = sheet.DataValidations.AddListValidation(targetCell);
                // Sets formula to show dropdown only when IsChild is True
                validation.Formula.ExcelFormula = $"IF(${conditionCell}=\"True\", ParentEntityRange, \"\")";
                // Configures validation error message settings
                validation.ShowErrorMessage = true;
                validation.ErrorTitle = "Invalid Selection";
                validation.Error = "Select a valid Parent Entity when IsChild is True.";
            }
        }

        /// <summary>
        /// Applies a parameter value dropdown to a worksheet column.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="factors">List of city models as factors.</param>
        private static void ApplyParamValueDropdown(ExcelWorksheet sheet, List<CityModel> factors)
        {
            // Gets workbook reference for named range creation
            var workbook = sheet.Workbook;
            // Initializes dictionary to map Parameter IDs to value lists
            var paramValueMap = new Dictionary<int, List<string>>();

            // Builds mapping of Parameter IDs to their corresponding values
            foreach (var factor in factors)
            {
                // Retrieves Parameter ID from factor
                int paramId = (int)factor.CountryId!;
                // Gets value combination for dropdown
                string valueCombination = factor.CityName!;

                // Adds value to appropriate Parameter ID list
                if (!paramValueMap.TryGetValue(paramId, out List<string>? value))
                {
                    paramValueMap[paramId] = [valueCombination];
                }
                else
                {
                    value.Add(valueCombination);
                }
            }

            // Stores parameter values in hidden columns for dropdown reference
            int startColumn = 30;
            foreach (var param in paramValueMap)
            {
                // Creates named range identifier
                string rangeName = $"Param_{param.Key}";
                int rowIndex = 2;

                // Populates hidden column with parameter values
                foreach (var value in param.Value)
                {
                    sheet.Cells[rowIndex, startColumn].Value = value;
                    rowIndex++;
                }

                // Defines range for named range creation
                var range = sheet.Cells[2, startColumn, rowIndex - 1, startColumn];
                // Creates named range for dropdown reference
                workbook.Names.Add(rangeName, range);

                startColumn++;
            }

            // Applies dynamic dropdown validation to rows 2 through 100
            for (int row = 2; row <= 100; row++)
            {
                // Adds list validation to ParamValue column
                var validation = sheet.DataValidations.AddListValidation($"D{row}");
                // Sets formula to dynamically reference named range based on ParameterId
                validation.Formula.ExcelFormula = $"INDIRECT(\"Param_\"&C{row})";
                // Configures validation error settings
                validation.ShowErrorMessage = true;
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                validation.ErrorTitle = "Invalid Selection";
                validation.Error = "Please select a valid ParamValue.";
            }
        }

        /// <summary>
        /// Populates a worksheet column with the specified values.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="values">The values to populate.</param>
        /// <param name="columnIndex">The column index to populate.</param>
        private static void PopulateColumn(ExcelWorksheet sheet, string[] values, int columnIndex)
        {
            // Handles empty values array by setting single blank cell
            if (values.Length == 0)
            {
                sheet.Cells[2, columnIndex].Value = "";
                return;
            }

            // Populates column with provided values starting from row 2
            for (int i = 0; i < values.Length; i++)
            {
                sheet.Cells[i + 2, columnIndex].Value = values[i];
            }
        }

        //private void PopulateColumn(ExcelWorksheet sheet, string[] values, int columnIndex)
        //{
        //    for (int i = 0; i < values.Length; i++)
        //    {
        //        sheet.Cells[i + 2, columnIndex].Value = values[i];
        //    }
        //}

        //private void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int maxRows)
        //{
        //    // Detect non-empty values
        //    int lastRow = sheet.Cells[sheet.Dimension.Start.Row, dataColumnIndex, sheet.Dimension.End.Row, dataColumnIndex]
        //        .Where(c => c.Value != null).Count();

        //    // Ensure at least one blank row is available for dropdowns
        //    if (lastRow == 0)
        //    {
        //        sheet.Cells[2, dataColumnIndex].Value = ""; // Add a blank value
        //        lastRow = 2;
        //    }

        //    var range = sheet.Cells[2, dataColumnIndex, lastRow, dataColumnIndex]; // Adjust range to only filled cells
        //    sheet.Workbook.Names.Add(rangeName, range);

        //    for (int row = 2; row <= maxRows; row++)
        //    {
        //        var validation = sheet.DataValidations.AddListValidation($"{column}{row}");
        //        validation.Formula.ExcelFormula = rangeName;
        //        validation.ShowErrorMessage = true;
        //        validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
        //        validation.ErrorTitle = "Invalid Selection";
        //        validation.Error = "Please select a valid option.";
        //    }
        //}

        /// <summary>
        /// Applies a dropdown to a worksheet column.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="rangeName">The name of the range for the dropdown.</param>
        /// <param name="column">The column to apply the dropdown to.</param>
        /// <param name="dataColumnIndex">The data column index.</param>
        /// <param name="maxRows">The maximum number of rows to apply the dropdown to.</param>

        /// <summary>
        /// Applies a dropdown to a worksheet column.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="rangeName">The name of the range for the dropdown.</param>
        /// <param name="column">The column to apply the dropdown to.</param>
        /// <param name="dataColumnIndex">The data column index.</param>
        /// <param name="maxRows">The maximum number of rows to apply the dropdown to.</param>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int maxRows)
        {
            int startRow = 2;

            // Ensure sheet has at least 2 rows
            int lastRow;
            if (sheet.Dimension == null)
            {
                lastRow = startRow; // no data, just use row 2
                sheet.Cells[startRow, dataColumnIndex].Value = ""; // add placeholder
            }
            else
            {
                // Find last row with value in the data column
                lastRow = sheet.Cells[startRow, dataColumnIndex, sheet.Dimension.End.Row, dataColumnIndex]
                            .Where(c => c.Value != null).Select(c => c.Start.Row).DefaultIfEmpty(startRow).Max();
            }

            // Ensure lastRow >= startRow
            if (lastRow < startRow)
                lastRow = startRow;

            // Create named range
            var range = sheet.Cells[startRow, dataColumnIndex, lastRow, dataColumnIndex];
            sheet.Workbook.Names.Add(rangeName, range);

            // Apply dropdown validation
            for (int row = startRow; row <= maxRows; row++)
            {
                var validation = sheet.DataValidations.AddListValidation($"{column}{row}");
                validation.Formula.ExcelFormula = rangeName;
                validation.ShowErrorMessage = true;
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                validation.ErrorTitle = "Invalid Selection";
                validation.Error = "Please select a valid option.";
            }
        }

        /// <summary>
        /// Applies a dropdown rule to a worksheet column.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="rangeName">The name of the range for the dropdown.</param>
        /// <param name="column">The column to apply the dropdown to.</param>
        /// <param name="dataColumnIndex">The data column index.</param>
        /// <param name="dataCount">The number of data items.</param>
        private static void ApplyDropdownrule(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int dataCount)
        {
            // Declare variable for Excel range
            ExcelRange range;
            // Check if data exists
            if (dataCount > 0)
            {
                // Create range from row 2 to dataCount + 1
                range = sheet.Cells[2, dataColumnIndex, dataCount + 1, dataColumnIndex];
            }
            else
            {
                // Add empty value to ensure dropdown has at least one option
                sheet.Cells[2, dataColumnIndex].Value = "";
                // Create single cell range
                range = sheet.Cells[2, dataColumnIndex, 2, dataColumnIndex];
            }
            // Add named range to workbook
            sheet.Workbook.Names.Add(rangeName, range);

            // Apply dropdown validation to rows 2 through 100
            for (int row = 2; row <= 100; row++)
            {
                // Create list validation for current cell
                var validation = sheet.DataValidations.AddListValidation($"{column}{row}");
                // Set formula to reference named range
                validation.Formula.ExcelFormula = rangeName;
                // Enable error message display
                validation.ShowErrorMessage = true;
                // Set validation style to stop invalid entries
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                // Set error dialog title
                validation.ErrorTitle = "Invalid Selection";
                // Set error message content
                validation.Error = "Please select a valid option.";
            }
        }

        /// <summary>
        /// Adds a formula to a worksheet column.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="resultColumn">The result column for the formula.</param>
        /// <param name="lookupColumn">The lookup column for the formula.</param>
        /// <param name="dataStartColumn">The data start column index.</param>
        /// <param name="idColumn">The ID column index.</param>
        /// <param name="dataCount">The number of data items.</param>
        private static void AddFormula(ExcelWorksheet sheet, string resultColumn, string lookupColumn, int dataStartColumn, int idColumn, int dataCount)
        {
            // Get address string for VLOOKUP range
            string rangeAddress = sheet.Cells[2, dataStartColumn, dataCount + 1, idColumn].Address;

            // Apply formula to each row from 2 to 100
            for (int row = 2; row <= 100; row++)
            {
                // Set VLOOKUP formula with empty check
                sheet.Cells[row, resultColumn[0] - 'A' + 1].Formula = $"IF({lookupColumn}{row}=\"\", \"\", VLOOKUP({lookupColumn}{row}, {rangeAddress}, 2, FALSE))";
            }
        }

        /// <summary>
        /// Sanitizes a range name for use in Excel.
        /// </summary>
        /// <param name="name">The name to sanitize.</param>
        /// <returns>The sanitized range name.</returns>
        private static string SanitizeRangeName(string name)
        {
            // Replace spaces with underscores
            string sanitized = name.Replace(" ", "_");

            // Replace any non-alphanumeric characters (except underscore) with underscores
            sanitized = MyRegex().Replace(sanitized, "_");

            // Check if name starts with digit
            if (char.IsDigit(sanitized[0]))
            {
                // Add underscore prefix if name starts with digit
                sanitized = "_" + sanitized;
            }

            // Return sanitized range name
            return sanitized;
        }

        /// <summary>
        /// Downloads a previously imported file by document ID.
        /// </summary>
        /// <param name="documentId">The document ID of the imported file.</param>
        /// <returns>A byte array containing the file data.</returns>
        public async Task<byte[]> DownloadImportedFile(int documentId)
        {
            // Retrieve import document from repository
            ImportDocument importDocumentHistory = _uow.ImportDocumentHistoryRepository.GetById(documentId);

            // Check if file data exists
            if (importDocumentHistory?.FileData == null)
            {
                // Throw exception if file not found
                throw new InvalidOperationException("File not found or has not been uploaded.");
            }

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Create Excel package from file data
            using var package = new ExcelPackage(new MemoryStream(importDocumentHistory.FileData));
            // Process each worksheet in the workbook
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                // Normalize worksheet name for comparison
                string sheetName = worksheet.Name.Trim().ToLower();

                // Process worksheet based on its name
                switch (sheetName)
                {
                    case "entities":
                        // Process Entities worksheet
                        ProcessEntitiesSheet(package.Workbook.Worksheets["Entities"]);
                        break;
                    case "lists":
                        // Process Lists worksheet
                        ProcessListsSheet(package.Workbook.Worksheets["Lists"]);
                        break;
                    case "listitem":
                        // Process ListItem worksheet
                        ProcessListItemsSheet(package.Workbook.Worksheets["ListItem"]);
                        break;
                    case "customer-parameter":
                        // Process Customer-Parameter worksheet
                        ProcessParameterCustomerSheet(package.Workbook.Worksheets["Customer-Parameter"]);
                        break;
                    case "product-parameter":
                        // Process Product-Parameter worksheet
                        ProcessParameterProductSheet(package.Workbook.Worksheets["Product-Parameter"]);
                        break;
                    case "factors":
                        // Process Factors worksheet
                        ProcessFactorSheet(package.Workbook.Worksheets["Factors"]);
                        break;
                    case "category":
                        // Process Category worksheet
                        ProcessCategorySheet(package.Workbook.Worksheets["Category"]);
                        break;
                    case "info":
                        // Process Info worksheet
                        ProcessInfoSheet(package.Workbook.Worksheets["Info"]);
                        break;
                    case "details":
                        // Process Details worksheet
                        ProcessDetailsSheet(package.Workbook.Worksheets["Details"]);
                        break;
                    case "rules":
                        // Process Rules worksheet
                        ProcessEruleSheet(package.Workbook.Worksheets["Rules"]);
                        break;
                    case "ecards":
                        // Process ECards worksheet
                        ProcessECardSheet(package.Workbook.Worksheets["ECards"]);
                        break;
                    case "pcards":
                        // Process PCards worksheet
                        ProcessPCardSheet(package.Workbook.Worksheets["PCards"]);
                        break;
                    default:
                        // Skip unknown worksheets
                        break;
                }
            }

            // Save modified Excel package to memory stream
            using var modifiedStream = new MemoryStream();
            // Save package asynchronously
            await package.SaveAsAsync(modifiedStream);
            // Return byte array of modified file
            return modifiedStream.ToArray();
        }
        /// <summary>
        /// Processes the "Entities" worksheet for validation and formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>
        private static void ProcessEntitiesSheet(ExcelWorksheet worksheet)
        {
            // Check if worksheet is null and return if true
            if (worksheet == null) return;

            // Get the total row count in the worksheet
            int rowCount = GetRowCount(worksheet);
            // Set the error message header in column 11
            worksheet.Cells[1, 11].Value = "Error Message";

            // Increment row count to include potential new rows
            rowCount++;
            // Iterate through each row starting from row 2 (skipping header)
            for (int row = 2; row <= rowCount; row++)
            {
                // Retrieve cell values for validation
                var code = worksheet.Cells[row, 1].Text;
                var entityName = worksheet.Cells[row, 2].Text;
                var countryName = worksheet.Cells[row, 3].Text;
                var countryIdText = worksheet.Cells[row, 4].Text;
                var cityName = worksheet.Cells[row, 5].Text;
                var cityIdText = worksheet.Cells[row, 6].Text;
                var address = worksheet.Cells[row, 7].Text;

                // Initialize list to track error fields
                List<string> errorFields = [];

                // Validate required fields and data types
                if (string.IsNullOrWhiteSpace(code)) errorFields.Add("Code");
                if (string.IsNullOrWhiteSpace(entityName)) errorFields.Add("EntityName");
                if (string.IsNullOrWhiteSpace(countryName)) errorFields.Add("CountryName");
                if (string.IsNullOrWhiteSpace(countryIdText) || !int.TryParse(countryIdText, out _)) errorFields.Add("CountryId");
                if (string.IsNullOrWhiteSpace(cityName)) errorFields.Add("CityName");
                if (string.IsNullOrWhiteSpace(cityIdText) || !int.TryParse(cityIdText, out _)) errorFields.Add("CityId");
                if (string.IsNullOrWhiteSpace(address)) errorFields.Add("Address");

                // Check if any errors were found
                if (errorFields.Count > 0)
                {
                    // Highlight all columns in the row with error styling
                    for (int col = 1; col <= 10; col++)
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }

                    // Set error message describing missing/invalid fields
                    worksheet.Cells[row, 11].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 11].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }

        /// <summary>
        /// Processes the "Lists" worksheet for validation and formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>
        private static void ProcessListsSheet(ExcelWorksheet worksheet)
        {
            // Check if worksheet is null and return if true
            if (worksheet == null) return;

            // Get the total row count in the worksheet
            int rowCount = GetRowCount(worksheet);
            // Set the error message header in column 4
            worksheet.Cells[1, 4].Value = "Error Message";
            // Style the error message column headers
            for (int row = 1; row <= rowCount; row++)
            {
                worksheet.Cells[row, 4].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            }
            // Increment row count to include potential new rows
            rowCount++;
            // Iterate through each row starting from row 2 (skipping header)
            for (int row = 2; row <= rowCount; row++)
            {
                // Retrieve cell values for validation
                var listName = worksheet.Cells[row, 1].Text;
                var tenantIdText = worksheet.Cells[row, 3].Text;

                // Initialize list to track error fields
                List<string> errorFields = [];

                // Validate required fields and data types
                if (string.IsNullOrWhiteSpace(listName)) errorFields.Add("ListName");
                if (string.IsNullOrWhiteSpace(tenantIdText) || !int.TryParse(tenantIdText, out _)) errorFields.Add("TenantId");

                // Check if any errors were found
                if (errorFields.Count > 0)
                {
                    // Highlight all columns in the row with error styling
                    for (int col = 1; col <= 3; col++)
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }

                    // Set error message describing missing/invalid fields
                    worksheet.Cells[row, 4].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 4].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }

        /// <summary>
        /// Processes the "ListItems" worksheet for validation and formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>
        private static void ProcessListItemsSheet(ExcelWorksheet worksheet)
        {
            // Check if worksheet is null and return if true
            if (worksheet == null) return;

            // Get the total row count in the worksheet
            int rowCount = GetRowCount(worksheet);
            // Set the error message header in column 4
            worksheet.Cells[1, 4].Value = "Error Message";
            // Style the error message column headers
            for (int row = 1; row <= rowCount; row++)
            {
                worksheet.Cells[row, 4].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            }
            // Increment row count to include potential new rows
            rowCount++;
            // Iterate through each row starting from row 2 (skipping header)
            for (int row = 2; row <= rowCount; row++)
            {
                // Retrieve cell values for validation
                var itemName = worksheet.Cells[row, 2].Text;
                var listIdText = worksheet.Cells[row, 3].Text;

                // Initialize list to track error fields
                List<string> errorFields = [];

                // Validate required fields and data types
                if (string.IsNullOrWhiteSpace(itemName)) errorFields.Add("ItemName");
                if (string.IsNullOrWhiteSpace(listIdText) || !int.TryParse(listIdText, out _)) errorFields.Add("ListId");

                // Check if any errors were found
                if (errorFields.Count > 0)
                {
                    // Highlight all columns in the row with error styling
                    for (int col = 1; col <= 3; col++)
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }

                    // Set error message describing missing/invalid fields
                    worksheet.Cells[row, 4].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 4].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }

        /// <summary>
        /// Processes the "ParameterCustomer" worksheet for validation and formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>
        private static void ProcessParameterCustomerSheet(ExcelWorksheet worksheet)
        {
            // Check if worksheet is null and return if true
            if (worksheet == null) return;

            // Get the total row count in the worksheet
            int rowCount = GetRowCount(worksheet);
            // Set the error message header in column 11
            worksheet.Cells[1, 11].Value = "Error Message";
            // Style the error message column headers
            for (int row = 1; row <= rowCount; row++)
            {
                worksheet.Cells[row, 11].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[row, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 11].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            }
            // Increment row count to include potential new rows
            rowCount++;
            // Iterate through each row starting from row 2 (skipping header)
            for (int row = 2; row <= rowCount; row++)
            {
                // Retrieve cell values for validation
                var ParameterName = worksheet.Cells[row, 1].Text;
                var DataTypeId = worksheet.Cells[row, 3].Text;
                var HasFactors = worksheet.Cells[row, 4].Text;
                var TenantId = worksheet.Cells[row, 10].Text;
                var ConditionId = worksheet.Cells[row, 8].Text;
                var FactorOrder = worksheet.Cells[row, 6].Text;

                // Initialize list to track error fields
                List<string> errorFields = [];

                // Validate required fields and data types
                if (string.IsNullOrWhiteSpace(ParameterName)) errorFields.Add("ParameterName");
                if (string.IsNullOrWhiteSpace(DataTypeId) || !int.TryParse(DataTypeId, out _)) errorFields.Add("ParameterType");
                if (string.IsNullOrWhiteSpace(HasFactors)) errorFields.Add("HasFactors");
                if (string.IsNullOrWhiteSpace(TenantId) || !int.TryParse(TenantId, out _)) errorFields.Add("TenantId");
                // Parse HasFactors boolean and validate conditional fields if true
                bool factors = bool.TryParse(HasFactors, out bool hasFactors) && hasFactors;
                if (factors)
                {
                    if (string.IsNullOrWhiteSpace(FactorOrder)) errorFields.Add("FactorOrder");
                    if (string.IsNullOrWhiteSpace(ConditionId) || !int.TryParse(ConditionId, out _)) errorFields.Add("ConditionId");
                }

                // Check if any errors were found
                if (errorFields.Count > 0)
                {
                    // Highlight all columns in the row with error styling
                    for (int col = 1; col <= 10; col++)
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }

                    // Set error message describing missing/invalid fields
                    worksheet.Cells[row, 11].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 11].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }

        /// <summary>
        /// Processes the "ParameterProduct" worksheet for validation and formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>
        private static void ProcessParameterProductSheet(ExcelWorksheet worksheet)
        {
            // Check if worksheet is null and return if true
            if (worksheet == null) return;

            // Get the total row count in the worksheet
            int rowCount = GetRowCount(worksheet);
            // Set the error message header in column 11
            worksheet.Cells[1, 11].Value = "Error Message";
            // Style the error message column headers
            for (int row = 1; row <= rowCount; row++)
            {
                worksheet.Cells[row, 11].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[row, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 11].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            }
            // Increment row count to include potential new rows
            rowCount++;
            // Iterate through each row starting from row 2 (skipping header)
            for (int row = 2; row <= rowCount; row++)
            {
                // Retrieve cell values for validation
                var ParameterName = worksheet.Cells[row, 1].Text;
                var DataTypeId = worksheet.Cells[row, 3].Text;
                var HasFactors = worksheet.Cells[row, 4].Text;
                var TenantId = worksheet.Cells[row, 10].Text;
                var ConditionId = worksheet.Cells[row, 8].Text;
                var FactorOrder = worksheet.Cells[row, 6].Text;

                // Initialize list to track error fields
                List<string> errorFields = [];

                // Validate required fields and data types
                if (string.IsNullOrWhiteSpace(ParameterName)) errorFields.Add("ParameterName");
                if (string.IsNullOrWhiteSpace(DataTypeId) || !int.TryParse(DataTypeId, out _)) errorFields.Add("ParameterType");
                if (string.IsNullOrWhiteSpace(HasFactors)) errorFields.Add("HasFactors");
                if (string.IsNullOrWhiteSpace(TenantId) || !int.TryParse(TenantId, out _)) errorFields.Add("TenantId");
                // Parse HasFactors boolean and validate conditional fields if true
                bool factors = bool.TryParse(HasFactors, out bool hasFactors) && hasFactors;
                if (factors)
                {
                    if (string.IsNullOrWhiteSpace(FactorOrder)) errorFields.Add("FactorOrder");
                    if (string.IsNullOrWhiteSpace(ConditionId) || !int.TryParse(ConditionId, out _)) errorFields.Add("ConditionId");
                }
                // Check if any errors were found
                if (errorFields.Count > 0)
                {
                    // Highlight all columns in the row with error styling
                    for (int col = 1; col <= 10; col++)
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }

                    // Set error message describing missing/invalid fields
                    worksheet.Cells[row, 11].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 11].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }
        /// <summary>
        /// Processes the "Factor" worksheet for validation or formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>

        private static void ProcessFactorSheet(ExcelWorksheet worksheet)
        {
            if (worksheet == null) return;

            int rowCount = GetRowCount(worksheet);
            worksheet.Cells[1, 9].Value = "Error Message";
            for (int row = 1; row <= rowCount; row++)
            {
                worksheet.Cells[row, 9].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[row, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 9].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            }
            rowCount++;
            for (int row = 2; row <= rowCount; row++)
            {
                var FactorName = worksheet.Cells[row, 1].Text;
                var ParameterId = worksheet.Cells[row, 3].Text;
                var ConditionId = worksheet.Cells[row, 5].Text;
                var Value1 = worksheet.Cells[row, 6].Text;
                var Value01 = worksheet.Cells[row, 7].Text;
                var Value2 = worksheet.Cells[row, 8].Text;

                List<string> errorFields = [];

                if (string.IsNullOrWhiteSpace(FactorName)) errorFields.Add("FactorName");
                if (string.IsNullOrWhiteSpace(ParameterId)) errorFields.Add("ParameterId");
                if (string.IsNullOrWhiteSpace(ConditionId)) errorFields.Add("ConditionId");
                if (string.IsNullOrWhiteSpace(Value1)) errorFields.Add("Value1");
                if (ConditionId == "2")
                {
                    if (string.IsNullOrWhiteSpace(Value2)) errorFields.Add("Value2");
                }
                if (ConditionId == "12" || ConditionId == "13")
                {
                    if (string.IsNullOrWhiteSpace(Value01)) errorFields.Add("Value1");
                    //Value1 = worksheet.Cells[row, 7].Text;
                }
                if (errorFields.Count > 0)
                {
                    for (int col = 1; col <= 8; col++) // Highlight all columns
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }

                    worksheet.Cells[row, 9].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 9].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }
        /// <summary>
        /// Processes the "Category" worksheet for validation or formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>

        private static void ProcessCategorySheet(ExcelWorksheet worksheet)
        {
            if (worksheet == null) return;

            int rowCount = GetRowCount(worksheet);
            worksheet.Cells[1, 5].Value = "Error Message";
            for (int row = 1; row <= rowCount; row++)
            {
                worksheet.Cells[row, 5].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            }
            rowCount++;
            for (int row = 2; row <= rowCount; row++)
            {
                var CategoryName = worksheet.Cells[row, 1].Text;
                var CatDescription = worksheet.Cells[row, 2].Text;
                var TenantId = worksheet.Cells[row, 4].Text;

                List<string> errorFields = [];

                if (string.IsNullOrWhiteSpace(CategoryName)) errorFields.Add("ParameterName");
                if (string.IsNullOrWhiteSpace(CatDescription)) errorFields.Add("CatDescription");
                if (string.IsNullOrWhiteSpace(TenantId) || !int.TryParse(TenantId, out _)) errorFields.Add("TenantId");

                if (errorFields.Count > 0)
                {
                    for (int col = 1; col <= 4; col++) // Highlight all columns
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }

                    worksheet.Cells[row, 5].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 5].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }

        /// <summary>
        /// Processes the "Info" worksheet for validation or formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>

        private static void ProcessInfoSheet(ExcelWorksheet worksheet)
        {
            if (worksheet == null) return;

            int rowCount = GetRowCount(worksheet);
            worksheet.Cells[1, 10].Value = "Error Message";
            for (int row = 1; row <= rowCount; row++)
            {
                worksheet.Cells[row, 10].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[row, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 10].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            }
            rowCount++;
            for (int row = 2; row <= rowCount; row++)
            {
                var Code = worksheet.Cells[row, 1].Text;
                var ProductName = worksheet.Cells[row, 2].Text;
                var CategoryId = worksheet.Cells[row, 4].Text;
                var TenantId = worksheet.Cells[row, 6].Text;
                var imageUrl = worksheet.Cells[row, 7].Text;
                //var Narrative = worksheet.Cells[row, 8].Text;
                //var Description = worksheet.Cells[row, 9].Text;

                List<string> errorFields = [];

                if (string.IsNullOrWhiteSpace(Code)) errorFields.Add("ParameterName");
                if (string.IsNullOrWhiteSpace(CategoryId) || !int.TryParse(CategoryId, out _)) errorFields.Add("CategoryId");
                if (string.IsNullOrWhiteSpace(ProductName)) errorFields.Add("ProductName");
                if (string.IsNullOrWhiteSpace(TenantId) || !int.TryParse(TenantId, out _)) errorFields.Add("TenantId");
                if (string.IsNullOrWhiteSpace(imageUrl)) errorFields.Add("imageUrl");

                if (errorFields.Count > 0)
                {
                    for (int col = 1; col <= 9; col++) // Highlight all columns
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }
                    worksheet.Cells[row, 10].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 10].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }
        /// <summary>
        /// Processes the "Details" worksheet for validation or formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>

        private static void ProcessDetailsSheet(ExcelWorksheet worksheet)
        {
            if (worksheet == null) return;

            int rowCount = GetRowCount(worksheet);
            worksheet.Cells[1, 10].Value = "Error Message";
            for (int row = 1; row <= rowCount; row++)
            {
                worksheet.Cells[row, 10].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[row, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 10].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            }
            rowCount++;
            for (int row = 2; row <= rowCount; row++)
            {
                var productId = worksheet.Cells[row, 2].Text;
                var tenantId = worksheet.Cells[row, 4].Text;
                var parameterId = worksheet.Cells[row, 6].Text;
                var paramValue = worksheet.Cells[row, 7].Text;
                var DisplayOrder = worksheet.Cells[row, 8].Text;
                var IsRequired = worksheet.Cells[row, 9].Text;

                List<string> errorFields = [];

                if (string.IsNullOrWhiteSpace(productId) || !int.TryParse(productId, out _)) errorFields.Add("productId");
                if (string.IsNullOrWhiteSpace(tenantId) || !int.TryParse(tenantId, out _)) errorFields.Add("tenantId");
                if (string.IsNullOrWhiteSpace(parameterId) || !int.TryParse(parameterId, out _)) errorFields.Add("parameterId");
                if (string.IsNullOrWhiteSpace(paramValue)) errorFields.Add("paramValue");
                if (string.IsNullOrWhiteSpace(DisplayOrder)) errorFields.Add("DisplayOrder");
                if (string.IsNullOrWhiteSpace(IsRequired) || !bool.TryParse(IsRequired, out _)) errorFields.Add("IsRequired");

                if (errorFields.Count > 0)
                {
                    for (int col = 1; col <= 9; col++) // Highlight all columns
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }

                    worksheet.Cells[row, 10].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 10].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }
        /// <summary>
        /// Processes the "Erule" worksheet for validation or formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>

        private static void ProcessEruleSheet(ExcelWorksheet worksheet)
        {
            if (worksheet == null) return;

            int rowCount = GetRowCount(worksheet);
            worksheet.Cells[1, 10].Value = "Error Message";
            for (int row = 1; row <= rowCount; row++)
            {
                worksheet.Cells[row, 10].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[row, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 10].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            }
            rowCount++;
            for (int row = 2; row <= rowCount; row++)
            {
                var EruleName = worksheet.Cells[row, 1].Text;
                var EruleDesc = worksheet.Cells[row, 2].Text;
                var ExpShown = worksheet.Cells[row, 9].Text;

                List<string> errorFields = [];

                if (string.IsNullOrWhiteSpace(EruleName)) errorFields.Add("EruleName");
                if (string.IsNullOrWhiteSpace(EruleDesc)) errorFields.Add("EruleDesc");
                if (string.IsNullOrWhiteSpace(ExpShown)) errorFields.Add("ExpShown");

                if (errorFields.Count > 0)
                {
                    for (int col = 1; col <= 9; col++) // Highlight all columns
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }

                    worksheet.Cells[row, 10].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 10].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }
        /// <summary>
        /// Processes the "ECard" worksheet for validation or formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>

        private static void ProcessECardSheet(ExcelWorksheet worksheet)
        {
            if (worksheet == null) return;

            int rowCount = GetRowCount(worksheet);
            worksheet.Cells[1, 8].Value = "Error Message";
            for (int row = 1; row <= rowCount; row++)
            {
                worksheet.Cells[row, 8].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            }
            rowCount++;
            for (int row = 2; row <= rowCount; row++)
            {
                var EcardName = worksheet.Cells[row, 1].Text;
                var EcardDesc = worksheet.Cells[row, 2].Text;
                var ExpShown = worksheet.Cells[row, 7].Text;

                List<string> errorFields = [];

                if (string.IsNullOrWhiteSpace(EcardName)) errorFields.Add("EcardName");
                if (string.IsNullOrWhiteSpace(EcardDesc)) errorFields.Add("EcardDesc");
                if (string.IsNullOrWhiteSpace(ExpShown)) errorFields.Add("ExpShown");

                if (errorFields.Count > 0)
                {
                    for (int col = 1; col <= 7; col++) // Highlight all columns
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }

                    worksheet.Cells[row, 8].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 8].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }

        /// <summary>
        /// Processes the "PCard" worksheet for validation or formatting.
        /// </summary>
        /// <param name="worksheet">The worksheet to process.</param>

        private static void ProcessPCardSheet(ExcelWorksheet worksheet)
        {
            if (worksheet == null) return;
            int rowCount = GetRowCount(worksheet);
            worksheet.Cells[1, 10].Value = "Error Message";
            for (int row = 1; row <= rowCount; row++)
            {
                worksheet.Cells[row, 10].Style.Font.Color.SetColor(Color.Red);
                worksheet.Cells[row, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 10].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
            }
            rowCount++;
            for (int row = 2; row <= rowCount; row++)
            {
                var PcardName = worksheet.Cells[row, 1].Text;
                var PcardDesc = worksheet.Cells[row, 2].Text;
                var ProductId = worksheet.Cells[row, 4].Text;
                var Expshown = worksheet.Cells[row, 9].Text;

                List<string> errorFields = [];

                if (string.IsNullOrWhiteSpace(PcardName)) errorFields.Add("PcardName");
                if (string.IsNullOrWhiteSpace(PcardDesc)) errorFields.Add("PcardDesc");
                if (string.IsNullOrWhiteSpace(Expshown)) errorFields.Add("Expshown");
                if (string.IsNullOrWhiteSpace(ProductId) || !int.TryParse(ProductId, out _)) errorFields.Add("ProductId");

                if (errorFields.Count > 0)
                {
                    for (int col = 1; col <= 9; col++) // Highlight all columns
                    {
                        worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Red);
                        worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                    }

                    worksheet.Cells[row, 10].Value = string.Join(", ", errorFields) + " is missing or invalid.";
                    worksheet.Cells[row, 10].Style.Font.Color.SetColor(Color.Red);
                }
            }
        }
        [GeneratedRegex(@"\s+(AND|OR)\s+", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex3();
        [System.Text.RegularExpressions.GeneratedRegex(@"[^A-Za-z0-9_]")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
    }
}