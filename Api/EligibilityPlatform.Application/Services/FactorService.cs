using System.ComponentModel;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Numerics;
using AutoMapper;
using CsvHelper;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing factor operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FactorService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="parameterService">The parameter service instance.</param>
    /// <param name="conditionService">The condition service instance.</param>
    /// <param name="managedListService">The managed list service instance.</param>
    public partial class FactorService(IUnitOfWork uow, IMapper mapper, IParameterService parameterService, IConditionService conditionService, IManagedListService managedListService) : IFactorService
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
        /// The parameter service instance.
        /// </summary>
        private readonly IParameterService _parameterService = parameterService;

        /// <summary>
        /// The condition service instance.
        /// </summary>
        private readonly IConditionService _conditionService = conditionService;

        /// <summary>
        /// The managed list service instance.
        /// </summary>
        private readonly IManagedListService _managedListService = managedListService;

        /// <summary>
        /// Adds a new factor record to the database.
        /// Maps the incoming FactorModel to the Factor entity and saves it asynchronously.
        /// </summary>
        /// <param name="model">The FactorModel containing data to be added to the Factor entity.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the add operation.</exception>
        public async Task Add(FactorAddUpdateModel model)
        {
            var existingFactor = _uow.FactorRepository.Query().Any(p => p.ParameterId == model.ParameterId && p.ConditionId == model.ConditionId && model.Value1 == p.Value1 && model.Value2 == p.Value2 && p.TenantId == model.TenantId);          // Maps the incoming model to Factor entity
            if (existingFactor)
            {
                throw new Exception("Duplicate Record already exist");
            }
            var factorEntites = _mapper.Map<Factor>(model);
            // Sets the update timestamp to current UTC time
            factorEntites.UpdatedByDateTime = DateTime.UtcNow;
            // Sets the creation timestamp to current UTC time
            factorEntites.CreatedByDateTime = DateTime.UtcNow;
            // Sets the update timestamp to current UTC time again (redundant)
            // Adds the factor entity to the repository
            _uow.FactorRepository.Add(factorEntites);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes an existing factor record from the database by its ID.
        /// Retrieves the factor by its ID and entity ID, removes it, and commits the changes asynchronously.
        /// </summary>
        /// <param name="tenantId">The entity ID associated with the factor.</param>
        /// <param name="id">The ID of the factor record to be deleted.</param>
        /// <returns>A Task representing the asynchronous delete operation.</returns>
        /// <exception cref="Exception">Thrown when the factor record is not found or an error occurs during the delete operation.</exception>
        public async Task Delete(int tenantId, int id)
        {
            // Retrieves the factor by ID and entity ID
            var factors = _uow.FactorRepository.Query().First(f => f.FactorId == id && f.TenantId == tenantId);
            // Removes the factor from the repository
            _uow.FactorRepository.Remove(factors);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all factor records from the database for a specific entity.
        /// Maps the data to a list of FactorListModel objects for easier presentation.
        /// </summary>
        /// <param name="tenantId">The entity ID to filter factor records.</param>
        /// <returns>A list of FactorListModel objects containing all factor records for the specified entity.</returns>
        public List<FactorListModel> GetAll(int tenantId)
        {
            // Retrieves all factors for the specified entity
            var factors = _uow.FactorRepository.Query().Where(f => f.TenantId == tenantId);
            // Maps the factors to FactorListModel objects
            return _mapper.Map<List<FactorListModel>>(factors);
        }

        /// <summary>
        /// Retrieves a specific factor record by its ID and entity ID.
        /// Maps the retrieved entity to a FactorListModel object for better presentation.
        /// </summary>
        /// <param name="tenantId">The entity ID associated with the factor.</param>
        /// <param name="id">The ID of the factor record to retrieve.</param>
        /// <returns>The FactorListModel object representing the factor record with the specified ID.</returns>
        /// <exception cref="Exception">Thrown when the factor record is not found.</exception>
        public FactorListModel GetById(int tenantId, int id)
        {
            // Retrieves the factor by ID and entity ID
            var factors = _uow.FactorRepository.Query().First(f => f.FactorId == id && f.TenantId == tenantId);
            // Maps the factor to FactorListModel object
            return _mapper.Map<FactorListModel>(factors);
        }

        /// <summary>
        /// Updates an existing factor record in the database.
        /// Retrieves the factor by its ID and entity ID, maps the updated data from the provided model, and saves the changes asynchronously.
        /// </summary>
        /// <param name="model">The model containing the updated data for the factor record.</param>
        /// <returns>A Task representing the asynchronous update operation.</returns>
        /// <exception cref="Exception">Thrown when the factor record is not found or an error occurs during the update operation.</exception>
        public async Task Update(FactorAddUpdateModel model)
        {
            var existingFactor = _uow.FactorRepository.Query().Any(p => p.ParameterId == model.ParameterId && p.ConditionId == model.ConditionId && model.Value1 == p.Value1 && model.Value2 == p.Value2 && p.TenantId == model.TenantId && p.FactorId != model.FactorId);          // Maps the incoming model to Factor entity
            if (existingFactor)
            {
                throw new Exception("Duplicate Record already exist");
            }
            // Retrieves the factor by ID and entity ID
            var factors = _uow.FactorRepository.Query().First(f => f.FactorId == model.FactorId && f.TenantId == model.TenantId);
            var createdBy = factors.CreatedBy;
            var createdDate = factors.CreatedByDateTime;
            // Maps the model properties to the existing factor entity
            var factorEntites = _mapper.Map<FactorModel, Factor>(model, factors);
            factors.CreatedBy = createdBy;
            factors.CreatedByDateTime = createdDate;
            // Sets the update timestamp to current UTC time
            factorEntites.UpdatedByDateTime = DateTime.UtcNow;

            // Updates the factor in the repository
            _uow.FactorRepository.Update(factorEntites);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Exports selected factor records to an Excel file.
        /// Joins data from the Factor, Parameter, Condition, and Entity tables, filters by selected factor IDs if provided,
        /// and writes the resulting data to an Excel worksheet. 
        /// The Excel file is returned as a stream.
        /// </summary>
        /// <param name="tenantId">The entity ID to filter factor records.</param>
        /// <param name="selectedFactorIds">List of selected factor IDs to filter the data (if provided).</param>
        /// <returns>A Task that represents the asynchronous operation, with a stream containing the exported Excel file.</returns>
        /// <exception cref="Exception">Thrown when any error occurs during the export process.</exception>
        public async Task<Stream> ExportFactors(int tenantId, List<int> selectedFactorIds)
        {
            // Creates a query joining Factor, Parameter, Condition, and Entity tables
            var factors = from factor in _uow.FactorRepository.Query()
                          join parameter in _uow.ParameterRepository.Query()
                          on factor.ParameterId equals parameter.ParameterId

                          join condition in _uow.ConditionRepository.Query()
                          on factor.ConditionId equals condition.ConditionId

                          //join entity in _uow.EntityRepository.Query()
                          //on factor.TenantId equals entity.TenantId into entityGroup
                          //from entity in entityGroup.DefaultIfEmpty() // LEFT JOIN

                          where factor.TenantId == tenantId && parameter.TenantId == tenantId
                          select new FactorModelDescription
                          {
                              FactorId = factor.FactorId,
                              FactorName = factor.FactorName,
                              Note = factor.Note,
                              Value1 = factor.Value1,
                              Value2 = factor.Value2,
                              ConditionId = factor.ConditionId,
                              TenantId = factor.TenantId,
                              //EntityName = entity != null ? entity.EntityName : null,
                              ParameterId = factor.ParameterId,
                              ParameterName = parameter.ParameterName,
                              ConditionValue = condition.ConditionValue
                          };

            // If selectedFactorIds is not null and contains items, filter by IDs
            if (selectedFactorIds != null && selectedFactorIds.Count > 0)
            {
                // Filters factors by selected IDs
                factors = factors.Where(query => selectedFactorIds.Contains(query.FactorId));
            }

            // Executes the query and retrieves results as list
            var entities = await factors.ToListAsync();

            // Maps the entities to FactorModelDescription objects
            var models = _mapper.Map<List<FactorModelDescription>>(factors);

            // Sets the Excel package license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new Excel package
            using var package = new ExcelPackage();
            // Adds a new worksheet named "Factor"
            var worksheet = package.Workbook.Worksheets.Add("Factor");

            // Gets all properties of FactorModelDescription type
            var properties = typeof(FactorModelDescription).GetProperties();
            // Sets column headers using property names
            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
            }

            // Populates worksheet with data from models
            for (int row = 0; row < models.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(models[row]);
                }
            }

            // Auto-fits columns to content
            worksheet.Cells.AutoFitColumns();

            // Creates a memory stream to hold the Excel file
            var memoryStream = new MemoryStream();
            // Saves the package to the memory stream
            package.SaveAs(memoryStream);
            // Resets the stream position to beginning
            memoryStream.Position = 0;

            // Returns the memory stream containing the Excel file
            return memoryStream;
        }

        /// <summary>
        /// Imports factor entities from a CSV file into the database.
        /// Reads the data from the uploaded CSV file, maps each record to a Factor entity, and adds them to the database.
        /// Commits the changes asynchronously.
        /// </summary>
        /// <param name="tenantId">The entity ID to associate with imported factors.</param>
        /// <param name="fileStream">The file stream containing the CSV data to be imported.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the import process.</exception>
        public async Task ImportEntities(int tenantId, Stream fileStream)
        {
            // Creates a stream reader to read the file stream
            using var reader = new StreamReader(fileStream);
            // Creates a CSV reader with invariant culture settings
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            // Reads all records from CSV and converts to list
            var models = csv.GetRecords<FactorModel>().ToList();

            // Processes each model in the list
            foreach (var model in models)
            {
                // Maps the model to Factor entity
                var entity = _mapper.Map<Factor>(model);
                // Resets the FactorId to 0 for new entities
                entity.FactorId = 0;
                // Sets the entity ID
                entity.TenantId = tenantId;
                // Adds the entity to the repository
                _uow.FactorRepository.Add(entity);
            }

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Removes multiple factor records from the database.
        /// Iterates over the provided list of factor IDs, retrieves each factor record, and removes it from the database.
        /// Commits the changes asynchronously.
        /// </summary>
        /// <param name="tenantId">The entity ID associated with the factors.</param>
        /// <param name="ids">A list of factor IDs to be removed.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the removal process.</exception>
        public async Task RemoveMultiple(int tenantId, List<int> ids)
        {
            // Iterates through each ID in the list
            foreach (var id in ids)
            {
                // Retrieves the factor by ID and entity ID
                var item = _uow.FactorRepository.Query().First(f => f.FactorId == id && f.TenantId == tenantId);
                // Checks if the item exists
                if (item != null)
                {
                    // Removes the item from the repository
                    _uow.FactorRepository.Remove(item);
                }
            }
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves a list of values based on the provided parameter ID.
        /// Queries the database to find factors associated with the given parameter ID and returns the concatenated values of `Value1` and `Value2`.
        /// </summary>
        /// <param name="tenantId">The entity ID to filter factors.</param>
        /// <param name="parameterId">The ID of the parameter used to filter the factors.</param>
        /// <returns>A list of concatenated values of `Value1` and `Value2` for the matching factors.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the data retrieval process.</exception>
        public List<string> GetValueByParams(int tenantId, int parameterId)
        {
            // Queries factors by entity ID and parameter ID
            var values = _uow.FactorRepository.Query()
                                     .Where(f => f.TenantId == tenantId && f.ParameterId == parameterId)
                                     // Concatenates Value1 and Value2 with separator
                                     .Select(f =>
                                         (f.Value1 ?? string.Empty) +
                                         (string.IsNullOrEmpty(f.Value2) ? string.Empty : " - " + f.Value2))
                                     // Converts results to list
                                     .ToList();

            // Returns the list of concatenated values
            return values;
        }

        /// <summary>
        /// Retrieves a list of factor records based on the provided condition ID.
        /// Queries the database to find all factors that match the given condition ID and maps them to a list of FactorModel.
        /// </summary>
        /// <param name="tenantId">The entity ID to filter factors.</param>
        /// <param name="conditionId">The ID of the condition used to filter the factors.</param>
        /// <returns>A list of FactorModel representing the factors that match the given condition ID.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the data retrieval process.</exception>
        public List<FactorModel> GetFactorByCondition(int tenantId, int conditionId)
        {
            // Queries factors by entity ID and condition ID
            var factors = _uow.FactorRepository
                     .Query()
                     .Where(factor => factor.TenantId == tenantId && factor.ConditionId == conditionId)
                     // Converts results to list
                     .ToList();
            // Maps factors to FactorModel objects
            return _mapper.Map<List<FactorModel>>(factors);
        }

        /// <summary>
        /// Retrieves a list of factor records based on the provided parameter ID.
        /// Queries the database to find all factors that match the given parameter ID and maps them to a list of FactorModel.
        /// </summary>
        /// <param name="tenantId">The entity ID to filter factors.</param>
        /// <param name="parameterId">The ID of the parameter used to filter the factors.</param>
        /// <returns>A list of FactorModel representing the factors that match the given parameter ID.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the data retrieval process.</exception>
        public List<FactorModel> GetFactorByparameter(int tenantId, int parameterId)
        {
            // Queries factors by entity ID and parameter ID
            var factors = _uow.FactorRepository.Query().Where(factor => factor.TenantId == tenantId && factor.ParameterId == parameterId).ToList();
            // Maps factors to FactorModel objects
            return _mapper.Map<List<FactorModel>>(factors);
        }

        /// <summary>
        /// Imports factor records from an Excel file into the database.
        /// Reads data from the uploaded Excel file, checks for required fields, skips invalid records, and maps valid records to factor entities.
        /// Adds non-duplicate records to the database and commits the changes asynchronously.
        /// </summary>
        /// <param name="tenantId">The entity ID to associate with imported factors.</param>
        /// <param name="fileStream">The file stream containing the Excel data to be imported.</param>
        /// <param name="createdBy">The user who is performing the import operation.</param>
        /// <returns>A Task that represents the asynchronous operation, with a string message describing the result of the import process.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the import process.</exception>
        public async Task<string> ImportFactor(int tenantId, Stream fileStream, string createdBy)
        {
            // Sets the Excel package license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new Excel package from the file stream
            using var package = new ExcelPackage(fileStream);
            // Gets the first worksheet
            var worksheet = package.Workbook.Worksheets[0];
            // Gets the row count of the worksheet
            int rowCount = GetRowCount(worksheet);
            // Initializes a list to store factor models
            var models = new List<Factor>();
            // Counter for skipped records
            int skippedRecordsCount = 0;
            // Counter for duplicated records
            int dublicatedRecordsCount = 0;
            // Counter for inserted records
            int insertedRecordsCount = 0;
            // Result message string
            var resultMessage = "";
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
            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    return resultMessage = "Uploaded File Is Empty";
                }
                // Read data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    try
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

                        // special case
                        if (ConditionId == "12" || ConditionId == "13")
                        {
                            Value1 = worksheet.Cells[row, 7].Text;
                        }

                        // parse safe
                        _ = int.TryParse(ParameterId, out int parameterId);
                        _ = int.TryParse(ConditionId, out int conditionId);

                        var model = new Factor
                        {
                            CreatedBy = createdBy,
                            FactorName = FactorName,
                            ParameterId = parameterId,
                            ConditionId = conditionId,
                            Value1 = Value1,
                            Value2 = Value2,
                            Note = "string"
                        };

                        models.Add(model);
                    }
                    catch
                    {
                        // ignore row errors and continue
                        skippedRecordsCount++;
                        continue;
                    }
                }
                // Processes each model in the list
                foreach (var model in models)
                {
                    // Checks if a factor with the same properties already exists
                    var existingEntity = await _uow.FactorRepository.Query().AnyAsync(p => p.TenantId == tenantId && p.FactorName == model.FactorName && p.ParameterId == model.ParameterId && p.ConditionId == model.ConditionId && p.ConditionId == model.ConditionId && p.Value1 == model.Value1);
                    if (existingEntity)
                    {
                        // Increments duplicated records counter
                        dublicatedRecordsCount++;
                        // Skips adding this record
                        continue;
                    }
                    // Sets the entity ID
                    model.TenantId = tenantId;
                    // Sets the update timestamp to current UTC time
                    // Sets the creation timestamp to current UTC time
                    model.CreatedByDateTime = DateTime.UtcNow;
                    // Sets the update timestamp to current UTC time again (redundant)
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    model.CreatedBy = createdBy;
                    // Maps and adds the model to the repository
                    _uow.FactorRepository.Add(_mapper.Map<Factor>(model));
                    // Increments inserted records counter
                    insertedRecordsCount++;
                }

                // Commits the changes to the database
                await _uow.CompleteAsync();

                // Builds result message based on operation outcomes
                if (insertedRecordsCount > 0)
                {
                    resultMessage = $"{insertedRecordsCount} Factor Inserted Successfully.";
                }
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $" {skippedRecordsCount} records were not inserted because of missing required field.";
                }
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $" {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                // Handles any exceptions during import
                resultMessage = "Error On Factors Page = " + ex.Message;
            }
            // Returns the result message
            return resultMessage;
        }


        /// <summary>
        /// Calculates the number of rows that contain data in the specified worksheet.
        /// The method iterates through all rows from the second row onward and checks if any of the cells in columns 1, 2, or 3 contain data.
        /// It returns the last row that contains data in any of these columns.
        /// </summary>
        /// <param name="worksheet">The worksheet to evaluate for rows with data.</param>
        /// <returns>The index of the last row that contains data in the specified columns (1, 2, or 3).</returns>
        /// <exception cref="Exception">Thrown when an error occurs during row counting.</exception>
        static int GetRowCount(ExcelWorksheet worksheet)
        {
            // Gets the last row index from the worksheet dimension
            int lastRow = worksheet.Dimension.End.Row;
            // Initializes row count to zero
            //int rowCount = 0;
            // Initializes last non-empty row tracker to zero
            int lastNonEmptyRow = 0;

            // Iterates through rows starting from row 2 to the last row
            for (int row = 2; row <= lastRow; row++)
            {
                // Checks if any of the cells in columns 1, 2, or 3 contain non-whitespace data
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text);

                // If data is found in the current row
                if (hasData)
                {
                    // Updates the last non-empty row to the current row index
                    lastNonEmptyRow = row;
                }
            }

            // Returns the last row index that contained data
            return lastNonEmptyRow - 1;
        }

        /// <summary>
        /// Generates an Excel template containing the necessary headers and dropdown options for factor data.
        /// The template includes columns for factor name, parameter, condition, value1, value2, and field descriptions.
        /// The data for parameters, conditions, and managed lists is populated from respective services.
        /// The method returns the template as a byte array, ready to be downloaded as an Excel file.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation, with the Excel file as a byte array.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during template generation.</exception>
        public async Task<byte[]> DownloadTemplate(int tenantId)
        {
            // Retrieves all parameters for the specified entity
            List<ParameterListModel> parameterList = _parameterService.GetAll(tenantId);
            // Retrieves all conditions
            List<ConditionModel> conditionsList = _conditionService.GetAll();
            // Retrieves all managed lists for the specified entity
            List<ManagedListGetModel> managedList = _managedListService.GetAll(tenantId);

            // Sanitizes parameter names for Excel range compatibility
            parameterList.ForEach(dataType => dataType.ParameterName = SanitizeRangeName(dataType.ParameterName ?? ""));

            // Sets EPPlus license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new Excel package
            using var package = new ExcelPackage();
            // Adds a new worksheet named "Factors"
            var sheet = package.Workbook.Worksheets.Add("Factors");

            // Defines the column headers for the template
            string[] headers = ["FactorName*", "Parameter*", "ParameterId*", "Condition*", "ConditionId*", "Value1*", "Value2", "Field Description"];
            // Populates the header row with the defined headers
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }

            // Adds description for required fields in row 2, column 9
            sheet.Cells[2, 8].Value = "* Fields marked with an asterisk are required.";
            // Applies bold formatting to the required fields description
            sheet.Cells[2, 8].Style.Font.Bold = true;
            // Applies red color to the required fields description for visibility
            sheet.Cells[2, 8].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Adds reference column headers for internal data mapping
            sheet.Cells[1, 11].Value = "ParameterName";
            sheet.Cells[1, 12].Value = "ParameterId";
            sheet.Cells[1, 13].Value = "TenantId";
            sheet.Cells[1, 14].Value = "ConditionValue";
            sheet.Cells[1, 15].Value = "ConditionId";
            sheet.Cells[1, 17].Value = "ListName";

            // Populates reference columns with respective data
            PopulateColumn(sheet, [.. parameterList.Select(d => d.ParameterName ?? "")], 11);
            PopulateColumn(sheet, [.. parameterList.Select(d => d.ParameterId.ToString())], 12);
            //PopulateColumn(sheet, [.. parameterList.Select(d => d.TenantId.ToString() ?? "")], 13);
            PopulateColumn(sheet, [.. conditionsList.Select(c => c.ConditionValue ?? "")], 14);
            PopulateColumn(sheet, [.. conditionsList.Select(c => c.ConditionId.ToString())], 15);
            PopulateColumn(sheet, [.. managedList.Select(c => c.ListName ?? "".ToString())], 17);

            // Applies dropdown validation to parameter column using parameter range
            ApplyDropdown(sheet, "parameterRange", "B", 11, 100);
            // Applies dropdown validation to condition column using condition range
            ApplyDropdown(sheet, "ConditionValueRange", "D", 14, 100);
            // Applies dropdown validation to value1 column using list range
            //ApplyDropdown(sheet, "ListValueRange", "G", 17, 100);

            // Adds VLOOKUP formula to populate parameterId based on parameter selection
            AddFormula(sheet, "C", "B", 11, 12, 100);
            // Adds VLOOKUP formula to populate conditionId based on condition selection
            AddFormula(sheet, "E", "D", 14, 15, 100);

            // Handles validation for Value1 column based on condition and managed list
            HandleValue1Column(sheet, "D", "G", 100, [.. managedList.Select(c => c.ListName ?? "")]);

            // Auto-fits all columns to content
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            // Returns the Excel package as a byte array
            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// Adds a VLOOKUP formula to the specified column in the Excel sheet.
        /// The formula looks up values from a specified range and populates the result in the result column.
        /// </summary>
        /// <param name="sheet">The Excel worksheet where the formula will be applied.</param>
        /// <param name="resultColumn">The column where the formula result will be inserted.</param>
        /// <param name="lookupColumn">The column to look up the values for the formula.</param>
        /// <param name="dataStartColumn">The start column of the data range for the VLOOKUP.</param>
        /// <param name="idColumn">The column containing the ID for the lookup.</param>
        /// <param name="dataCount">The number of rows to apply the formula to.</param>
        /// <exception cref="Exception">Thrown when an error occurs while applying the formula.</exception>
        private static void AddFormula(ExcelWorksheet sheet, string resultColumn, string lookupColumn, int dataStartColumn, int idColumn, int dataCount)
        {
            // Creates the range address for VLOOKUP data
            string rangeAddress = sheet.Cells[2, dataStartColumn, dataCount + 1, idColumn].Address;

            // Applies VLOOKUP formula to each row in the result column
            for (int row = 2; row <= 100; row++)
            {
                // Sets formula to lookup value and return corresponding ID, handling empty lookup values
                sheet.Cells[row, resultColumn[0] - 'A' + 1].Formula = $"IF({lookupColumn}{row}=\"\", \"\", VLOOKUP({lookupColumn}{row}, {rangeAddress}, 2, FALSE))";
            }
        }

        /// <summary>
        /// Handles the validation and dropdown population for the Value1 column in the Excel sheet.
        /// Applies a dynamic validation rule to the Value1 column based on the condition column value.
        /// If the condition is "In List" or "Not In List", a dropdown with managed list values is applied.
        /// If the list is empty, a blank dropdown is applied.
        /// </summary>
        /// <param name="sheet">The Excel worksheet containing the Value1 column to be handled.</param>
        /// <param name="conditionColumn">The column that determines the condition for applying the dropdown.</param>
        /// <param name="value1Column">The Value1 column where the dropdown will be applied.</param>
        /// <param name="rowCount">The number of rows in the sheet to process.</param>
        /// <param name="managedList">A list of managed values to be used in the dropdown.</param>
        /// <exception cref="Exception">Thrown when an error occurs while handling the Value1 column.</exception>
        private static void HandleValue1Column(ExcelWorksheet sheet, string conditionColumn, string value1Column, int rowCount, List<string> managedList)
        {
            // Defines the named range for managed list values
            string managedListRange = "ListValueRange";
            // Defines a blank cell reference for empty dropdowns
            string blankCell = "Z1";

            // Ensures the blank cell is empty
            sheet.Cells[blankCell].Value = "";

            // Processes each row from row 2 to specified row count
            for (int row = 2; row <= rowCount + 1; row++)
            {
                // Constructs cell addresses for value1 and condition cells
                string value1Cell = $"{value1Column}{row}";
                string conditionCell = $"{conditionColumn}{row}";

                // Removes any existing validation from the value1 cell to avoid duplicates
                var existingValidation = sheet.DataValidations[value1Cell];
                if (existingValidation != null)
                {
                    sheet.DataValidations.Remove(existingValidation);
                }

                // Adds new list validation to the value1 cell
                var value1Validation = sheet.DataValidations.AddListValidation(value1Cell);

                // Sets validation formula based on managed list availability
                if (managedList.Count != 0)
                {
                    // Uses managed list range if values are available
                    value1Validation.Formula.ExcelFormula = $"IF(OR(${conditionCell}=\"In List\", ${conditionCell}=\"Not In List\"), {managedListRange}, \"\")";
                }
                else
                {
                    // Uses blank cell reference if no managed list values available
                    value1Validation.Formula.ExcelFormula = $"IF(OR(${conditionCell}=\"In List\", ${conditionCell}=\"Not In List\"), {blankCell}, \"\")";
                }

                // Configures validation error messages
                value1Validation.ShowErrorMessage = true;
                value1Validation.ErrorTitle = "Invalid Selection";
                value1Validation.Error = "Please select a valid list value.";
            }

            // Disables Value2 column for non-range conditions
            DisableValue2ForRange(sheet, "D", "G", 100);
        }

        /// <summary>
        /// Populates the specified column in the Excel sheet with the provided array of values.
        /// If the values array is empty, it ensures that at least one blank entry is added to the column.
        /// </summary>
        /// <param name="sheet">The Excel worksheet where the values will be populated.</param>
        /// <param name="values">The array of values to be inserted into the specified column.</param>
        /// <param name="columnIndex">The index of the column to populate.</param>
        /// <exception cref="Exception">Thrown when an error occurs while populating the column.</exception>
        private static void PopulateColumn(ExcelWorksheet sheet, string[] values, int columnIndex)
        {
            // Handles empty values array by adding a single blank entry
            if (values.Length == 0)
            {
                sheet.Cells[2, columnIndex].Value = "";
                return;
            }

            // Populates the column with values from the array
            for (int i = 0; i < values.Length; i++)
            {
                sheet.Cells[i + 2, columnIndex].Value = values[i];
            }
        }

        /// <summary>
        /// Applies a dropdown list validation to the specified column in the Excel sheet.
        /// The dropdown list is populated from a named range, and an error message is displayed if an invalid selection is made.
        /// </summary>
        /// <param name="sheet">The Excel worksheet where the dropdown will be applied.</param>
        /// <param name="rangeName">The name of the range containing the dropdown values.</param>
        /// <param name="column">The column to apply the dropdown validation to.</param>
        /// <param name="dataColumnIndex">The index of the data column that the dropdown references.</param>
        /// <param name="maxRows">The maximum number of rows to apply the dropdown to.</param>
        /// <exception cref="Exception">Thrown when an error occurs while applying the dropdown.</exception>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int maxRows)
        {
            // Detects the last row with data in the specified column
            int lastRow = sheet.Cells[sheet.Dimension.Start.Row, dataColumnIndex, sheet.Dimension.End.Row, dataColumnIndex]
                .Where(c => c.Value != null).Count();

            // Ensures at least one blank row exists for dropdowns
            if (lastRow == 0)
            {
                sheet.Cells[2, dataColumnIndex].Value = "";
                lastRow = 2;
            }

            // Creates a range for the dropdown values
            var range = sheet.Cells[2, dataColumnIndex, lastRow, dataColumnIndex];
            // Adds the range as a named range in the workbook
            sheet.Workbook.Names.Add(rangeName, range);

            // Applies dropdown validation to each row in the specified column
            for (int row = 2; row <= maxRows; row++)
            {
                // Creates list validation for the current cell
                var validation = sheet.DataValidations.AddListValidation($"{column}{row}");
                // Sets the validation formula to use the named range
                validation.Formula.ExcelFormula = rangeName;
                // Configures validation error settings
                validation.ShowErrorMessage = true;
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                validation.ErrorTitle = "Invalid Selection";
                validation.Error = "Please select a valid option.";
            }
        }

        /// <summary>
        /// Disables the Value2 column for rows where the condition column does not meet the "Range" condition.
        /// The method applies conditional formatting to visually disable the Value2 cells and adds data validation to prevent input unless the condition is "Range".
        /// </summary>
        /// <param name="sheet">The Excel worksheet containing the Value2 column to be disabled.</param>
        /// <param name="conditionColumn">The column that holds the condition value to control the disabling of the Value2 column.</param>
        /// <param name="targetColumn">The Value2 column to be disabled.</param>
        /// <param name="rowCount">The number of rows in the sheet to process.</param>
        /// <exception cref="Exception">Thrown when an error occurs while disabling the Value2 column.</exception>
        private static void DisableValue2ForRange(ExcelWorksheet sheet, string conditionColumn, string targetColumn, int rowCount)
        {
            // Processes each row from row 2 to specified row count
            for (int row = 2; row <= rowCount + 1; row++)
            {
                // Constructs cell addresses for condition and target cells
                string conditionCell = $"{conditionColumn}{row}";
                string targetCell = $"{targetColumn}{row}";

                // Removes any existing validation from the target cell
                var existingValidation = sheet.DataValidations[targetCell];
                if (existingValidation != null)
                {
                    sheet.DataValidations.Remove(existingValidation);
                }

                // Applies conditional formatting to visually disable the cell
                var conditionFormatting = sheet.ConditionalFormatting.AddExpression(sheet.Cells[row, targetColumn[0] - 'A' + 1]);
                // Sets conditional formatting formula based on condition value
                conditionFormatting.Formula = $"NOT(${conditionCell}=\"Range\")";
                // Applies gray background color for disabled state
                conditionFormatting.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                conditionFormatting.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                // Applies dark gray text color for disabled state
                conditionFormatting.Style.Font.Color.SetColor(System.Drawing.Color.DarkGray);

                // Adds custom validation to restrict input based on condition
                var validation = sheet.DataValidations.AddCustomValidation(targetCell);
                // Sets validation formula to allow input only when condition is "Range"
                validation.Formula.ExcelFormula = $"IF(${conditionCell}=\"Range\",TRUE,FALSE)";
                // Configures validation error messages
                validation.ShowErrorMessage = true;
                validation.ErrorTitle = "Invalid Input";
                validation.Error = "Input is not allowed unless the condition is 'Range'.";
            }
        }

        /// <summary>
        /// Sanitizes the range name by replacing spaces with underscores and removing any non-alphanumeric characters.
        /// Also ensures that the sanitized name does not start with a digit by prepending an underscore if necessary.
        /// </summary>
        /// <param name="name">The range name to be sanitized.</param>
        /// <returns>The sanitized range name that can be used in Excel.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while sanitizing the range name.</exception>
        private static string SanitizeRangeName(string name)
        {
            // Replaces spaces with underscores
            string sanitized = name.Replace(" ", "_");

            // Removes non-alphanumeric characters using regex
            sanitized = MyRegex().Replace(sanitized, "_");

            // Ensures name doesn't start with a digit by prepending underscore
            if (char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }

            // Returns the sanitized range name
            return sanitized;
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"[^A-Za-z0-9_]")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
    }
}
