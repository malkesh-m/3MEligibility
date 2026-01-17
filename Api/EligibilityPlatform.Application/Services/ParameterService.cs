using System.ComponentModel;
//using System.Reflection.Metadata;
using AutoMapper;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing parameters.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ParameterService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="dataTypeService">The data type service instance.</param>
    /// <param name="conditionService">The condition service instance.</param>
    /// <param name="entityService">The entity service instance.</param>
    public partial class ParameterService(IUnitOfWork uow, IMapper mapper, IDataTypeService dataTypeService, IConditionService conditionService/*, IEntityService entityService*/) : IParameterService
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
        /// The data type service instance for data type operations.
        /// </summary>
        private readonly IDataTypeService _dataTypeService = dataTypeService;

        /// <summary>
        /// The condition service instance for condition operations.
        /// </summary>
        private readonly IConditionService _conditionService = conditionService;

        /// <summary>
        /// The entity service instance for entity operations.
        /// </summary>
        //private readonly IEntityService _entityService = entityService;

        /// <summary>
        /// Adds a new parameter to the database.
        /// </summary>
        /// <param name="model">The ParameterAddUpdateModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the parameter name already exists in the entity.</exception>
        public async Task Add(ParameterAddUpdateModel model)
        {
            // Checks if parameter name already exists for the entity
            var res = _uow.ParameterRepository.Query().Any(p => p.ParameterName == model.ParameterName && p.TenantId == model.TenantId);
            if (res)
            {
                // Throws exception if parameter name already exists
                throw new Exception("Parameter name already exists in this entity");
            }

            // Maps the incoming model to Parameter entity
            var parameterEntities = _mapper.Map<Parameter>(model);
            // Sets the creation and update timestamps to current UTC time
            parameterEntities.UpdatedByDateTime = DateTime.UtcNow;
            parameterEntities.CreatedByDateTime = DateTime.UtcNow;

            // Adds the parameter entity to the repository
            _uow.ParameterRepository.Add(parameterEntities);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a parameter by its entity ID and parameter ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="id">The parameter ID to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int tenantId, int id)
        {
            // Retrieves the specific parameter by entity ID and parameter ID
            var Item = _uow.ParameterRepository.Query().First(f => f.ParameterId == id && f.TenantId == tenantId);
            var apiParameterMap = _uow.ApiParameterMapsRepository.Query().Where(f => f.ParameterId == Item.ParameterId).ToList();
            if (apiParameterMap.Count != 0)
                _uow.ApiParameterMapsRepository.RemoveRange(apiParameterMap);


            var reletedApiParameter = _uow.ApiParametersRepository.Query().Where(f => f.ParameterName == Item.ParameterName).ToList();
            if (reletedApiParameter.Count != 0)
                _uow.ApiParametersRepository.RemoveRange(reletedApiParameter);


            // Removes the parameter from the repository
            _uow.ParameterRepository.Remove(Item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all parameters for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of ParameterListModel representing all parameters for the entity.</returns>
        public List<ParameterListModel> GetAll(int tenantId)
        {
            // Queries parameters filtered by entity ID and includes computed values
            var parameters = _uow.ParameterRepository.Query()
                .Include(x => x.ComputedValues)
                .Where(f => f.TenantId == tenantId);
            // Maps the parameters to ParameterListModel objects
            return _mapper.Map<List<ParameterListModel>>(parameters);
        }

        /// <summary>
        /// Gets all parameters for a specific entity by entity ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of ParameterListModel for the specified entity.</returns>
        public List<ParameterListModel> GetByEntityId(int tenantId)
        {
            // Queries parameters with computed values and maps to custom model
            var parameters = _uow.ParameterRepository.Query()
                .Where(f => f.TenantId == tenantId)
                .Include(x => x.ComputedValues)
                .Select(p => new ParameterListModel
                {
                    ParameterId = p.ParameterId,
                    ParameterName = p.ParameterName,
                    HasFactors = p.HasFactors,
                    Identifier = p.Identifier,
                    IsKyc = p.IsKyc,
                    IsRequired = p.IsRequired,
                    TenantId = p.TenantId,
                    DataTypeId = p.DataTypeId,
                    ConditionId = p.ConditionId,
                    ValueSource = p.ValueSource,
                    StaticValue = p.StaticValue,
                    FactorOrder = p.FactorOrder,
                    IsMandatory = p.IsMandatory,
                    DataType = p.DataType!.DataTypeName ?? "",
                    CreatedBy = p.CreatedBy,
                    CreatedByDateTime = p.CreatedByDateTime,
                    UpdatedBy = p.UpdatedBy,
                    UpdatedByDateTime = p.UpdatedByDateTime,
                    ComputedValues = _mapper.Map<List<ParameterComputedValueModel>>(p.ComputedValues)
                }).ToList();

            // Maps the parameters to ParameterListModel objects
            return _mapper.Map<List<ParameterListModel>>(parameters);
        }

        /// <summary>
        /// Gets a parameter by its entity ID and parameter ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="id">The parameter ID to retrieve.</param>
        /// <returns>The ParameterListModel for the specified entity and parameter ID.</returns>
        public ParameterListModel GetById(int tenantId, int id)
        {
            // Retrieves the specific parameter by entity ID and parameter ID with computed values
            var parameter = _uow.ParameterRepository.Query().Include(x => x.ComputedValues)
                .First(f => f.ParameterId == id && f.TenantId == tenantId);
            // Maps the parameter to ParameterListModel object
            return _mapper.Map<ParameterListModel>(parameter);
        }

        /// <summary>
        /// Checks the computed value for a parameter.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="parameterId">The parameter ID.</param>
        /// <param name="parameterValue">The value to check.</param>
        /// <returns>A task representing the asynchronous operation, with the computed value as a string if found.</returns>
        public async Task<string?> CheckParameterComputedValue(int tenantId, int parameterId, string parameterValue)
        {
            // Retrieves the parameter with computed values by parameter ID
            var parameter = await _uow.ParameterRepository.Query().Include(x => x.ComputedValues)
                  .FirstOrDefaultAsync(p => p.ParameterId == parameterId);

            // Returns null if parameter or computed values not found
            if (parameter == null || parameter.ComputedValues == null || parameter.ComputedValues.Count == 0)
                return null;

            // Iterates through computed values to find a match
            foreach (var item in parameter.ComputedValues)
            {
                switch (item.ComputedParameterType)
                {
                    case ParameterComputedType.Single:
                        // Checks for exact value match
                        if (!string.IsNullOrWhiteSpace(item.ParameterExactValue) &&
                            item.ParameterExactValue.Equals(parameterValue, StringComparison.OrdinalIgnoreCase))
                        {
                            return item.ComputedValue;
                        }
                        break;

                    case ParameterComputedType.Range:
                        // Checks if value falls within specified range
                        if (decimal.TryParse(parameterValue, out var value) &&
                            decimal.TryParse(item.FromValue, out var from) &&
                            decimal.TryParse(item.ToValue, out var to) && value >= from && value <= to)
                        {
                            return item.ComputedValue;
                        }
                        break;
                }
            }

            // Returns null if no match found
            return null;
        }

        /// <summary>
        /// Updates an existing parameter.
        /// </summary>
        /// <param name="model">The ParameterAddUpdateModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the parameter name already exists in the entity.</exception>
        public async Task Update(ParameterAddUpdateModel model)
        {
            // Checks if parameter name already exists for another parameter in the same entity
            var res = _uow.ParameterRepository.Query().Any(p => p.ParameterName == model.ParameterName && p.ParameterId != model.ParameterId && p.TenantId == model.TenantId);
            if (res)
            {
                // Throws exception if parameter name already exists
                throw new Exception("Parameter name already exists in this entity");
            }

            // Retrieves the existing parameter with computed values
            var parameters = _uow.ParameterRepository.Query()
                .Include(x => x.ComputedValues)
                .FirstOrDefault(f => f.ParameterId == model.ParameterId && f.TenantId == model.TenantId) ?? throw new Exception("Parameter Not exists in this entity");
            var createdBy = parameters.CreatedBy;
            // Maps the updated model to the existing entity
            var parameterEntities = _mapper.Map<ParameterModel, Parameter>(model, parameters);
            parameters.CreatedBy = createdBy;
            // Sets the update timestamp to current UTC time
            parameterEntities.UpdatedByDateTime = DateTime.UtcNow;
            parameterEntities.UpdatedByDateTime = DateTime.UtcNow;

            // Updates the parameter entity in the repository
            _uow.ParameterRepository.Update(parameterEntities);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Exports selected parameters to an Excel file.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="Identifier">The identifier for the parameter type.</param>
        /// <param name="selectedParameterIds">A list of selected parameter IDs to export.</param>
        /// <returns>A task that represents the asynchronous operation, with a stream containing the exported Excel file.</returns>
        public async Task<Stream> ExportParameter(int tenantId, int Identifier, [FromBody] List<int> selectedParameterIds)
        {
            IQueryable<ParameterCsvModel> parameters;
            var sheetName = "";

            // Determines query and sheet name based on identifier
            if (Identifier == 1)
            {
                // Builds query for customer parameters
                parameters = from parameter in _uow.ParameterRepository.Query()
                             join datatype in _uow.DataTypeRepository.Query()
                             on parameter.DataTypeId equals datatype.DataTypeId
                             join entity in _uow.EntityRepository.Query()
                             on parameter.TenantId equals entity.EntityId
                             join condition in _uow.ConditionRepository.Query()
                             on parameter.ConditionId equals condition.ConditionId
                             where parameter.TenantId == tenantId && parameter.Identifier == 1
                             select new ParameterCsvModel
                             {
                                 ParameterId = parameter.ParameterId,
                                 ParameterName = parameter.ParameterName,
                                 HasFactors = parameter.HasFactors,
                                 Identifier = parameter.Identifier,
                                 IsKyc = parameter.IsKyc,
                                 IsRequired = parameter.IsRequired,
                                 TenantId = parameter.TenantId,
                                 EntityName = entity.EntityName,
                                 DataTypeId = parameter.DataTypeId,
                                 DataTypeName = datatype.DataTypeName,
                                 ConditionId = parameter.ConditionId,
                                 ConditionValue = condition.ConditionValue,
                                 FactorOrder = parameter.FactorOrder
                             };
                sheetName = "Customer-Parameters";
            }
            else
            {
                // Builds query for product parameters
                parameters = from parameter in _uow.ParameterRepository.Query()
                             join datatype in _uow.DataTypeRepository.Query()
                             on parameter.DataTypeId equals datatype.DataTypeId
                             join entity in _uow.EntityRepository.Query()
                             on parameter.TenantId equals entity.EntityId
                             join condition in _uow.ConditionRepository.Query()
                             on parameter.ConditionId equals condition.ConditionId
                             where parameter.TenantId == tenantId && parameter.Identifier == 2
                             select new ParameterCsvModel
                             {
                                 ParameterId = parameter.ParameterId,
                                 ParameterName = parameter.ParameterName,
                                 HasFactors = parameter.HasFactors,
                                 Identifier = parameter.Identifier,
                                 IsKyc = parameter.IsKyc,
                                 IsRequired = parameter.IsRequired,
                                 TenantId = parameter.TenantId,
                                 EntityName = entity.EntityName,
                                 DataTypeId = parameter.DataTypeId,
                                 DataTypeName = datatype.DataTypeName,
                                 ConditionId = parameter.ConditionId,
                                 ConditionValue = condition.ConditionValue,
                                 FactorOrder = parameter.FactorOrder
                             };
                sheetName = "Product-Parameters";
            }

            // Filters by selected parameter IDs if provided
            if (selectedParameterIds != null && selectedParameterIds.Count > 0)
            {
                parameters = parameters.Where(query => selectedParameterIds.Contains(query.ParameterId));
            }

            // Executes the query and retrieves results
            var Parameters = await parameters.ToListAsync();
            // Maps the results to CSV models
            var models = _mapper.Map<List<ParameterCsvModel>>(Parameters);

            // Sets up Excel package license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            // Creates worksheet with specified name
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            // Gets properties of ParameterCsvModel for column headers
            var properties = typeof(ParameterCsvModel).GetProperties();
            for (int col = 0; col < properties.Length; col++)
            {
                // Sets column headers
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
            }

            // Populates worksheet with data
            for (int row = 0; row < models.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    // Sets cell values from model properties
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(models[row]);
                }
            }

            // Auto-fits columns for better readability
            worksheet.Cells.AutoFitColumns();

            // Creates memory stream and saves Excel package to it
            var memoryStream = new MemoryStream();
            package.SaveAs(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Imports parameter records from an Excel file into the database.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="fileStream">The file stream containing the Excel data to import.</param>
        /// <param name="Identifier">The identifier for the parameter type.</param>
        /// <param name="createdBy">The user who is performing the import operation.</param>
        /// <returns>A task that represents the asynchronous operation, with a string message describing the result.</returns>
        public async Task<string> ImportEntities(int tenantId, Stream fileStream, int Identifier, string createdBy)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
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

            int rowCount = GetRowCount(worksheet);
            var models = new List<Parameter>();
            int skippedRecordsCount = 0;
            int duplicateRecordsCount = 0;
            int insertedRecordsCount = 0;
            string resultMessage = string.Empty;
            int sameRowCount = 0;


            try
            {
                // Check if worksheet is empty
                if (rowCount <= 0)
                {
                    return "Uploaded file is empty.";
                }
                HashSet<string> excelDuplicateCheck = [];

                // Process each row
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    var parameterName = worksheet.Cells[row, 1].Text?.Trim();
                    var dataTypeId = worksheet.Cells[row, 3].Text?.Trim();
                    var tenantIdStr = tenantId;
                    var isMandatory = worksheet.Cells[row, 4].Text?.Trim();

                    // inside loop


                    string excelKey = $"{parameterName?.ToLower()}_{tenantIdStr}";

                    if (excelDuplicateCheck.Contains(excelKey))
                    {
                        skippedRecordsCount++;
                        continue; // Skip duplicates from Excel itself
                    }

                    excelDuplicateCheck.Add(excelKey);
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(parameterName) ||
                        !int.TryParse(dataTypeId, out int parsedDataTypeId))
                    {
                        skippedRecordsCount++;

                        continue;
                    }

                    // Create parameter model
                    var model = new Parameter
                    {
                        CreatedBy = createdBy,
                        ParameterName = parameterName,
                        DataTypeId = parsedDataTypeId,
                        Identifier = Identifier,
                        IsRequired = bool.TryParse(isMandatory, out bool isRequired) && isRequired,
                        TenantId = tenantIdStr
                    };

                    models.Add(model);
                }

                // All skipped check
                if (skippedRecordsCount == rowCount)
                {
                    return "No new records to insert.";
                }

                // Process valid models for insertion
                foreach (var model in models)
                {
                    bool exists = await _uow.ParameterRepository.Query().AnyAsync(p =>
                        (p.ParameterName == model.ParameterName && p.TenantId == model.TenantId)

                       );

                    if (exists)
                    {
                        duplicateRecordsCount++;
                        continue;
                    }

                    model.CreatedByDateTime = DateTime.UtcNow;
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    model.UpdatedBy = createdBy;

                    var entity = _mapper.Map<Parameter>(model);
                    _uow.ParameterRepository.Add(entity);
                    insertedRecordsCount++;
                }

                // Commit DB changes
                await _uow.CompleteAsync();

                // Build final result message
                List<string> messages = [];

                if (insertedRecordsCount > 0)
                    messages.Add($"{insertedRecordsCount} parameter{(insertedRecordsCount > 1 ? "s" : "")} inserted successfully");

                if (skippedRecordsCount > 0)
                    messages.Add($"{skippedRecordsCount} record{(skippedRecordsCount > 1 ? "s were" : " was")} not inserted because of missing required field");
                if (sameRowCount > 0)
                    messages.Add($"{sameRowCount} record{(sameRowCount > 1 ? "s" : "")} same row");

                if (duplicateRecordsCount > 0)
                    messages.Add($"{duplicateRecordsCount} record{(duplicateRecordsCount > 1 ? "s" : "")} already exist");

                resultMessage = messages.Count > 0
                    ? string.Join(". ", messages) + "."
                    : "No new records to insert.";
            }
            catch (Exception ex)
            {
                resultMessage = "Error on Parameter page: " + ex.Message;
            }

            return resultMessage;
        }



        /// <summary>
        /// Gets the number of rows with data in the specified worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet to evaluate.</param>
        /// <returns>The number of rows with data.</returns>
        static int GetRowCount(ExcelWorksheet worksheet)
        {
            // Gets the last row number from worksheet dimensions
            int lastRow = worksheet.Dimension.End.Row;
            int lastNonEmptyRow = 0;

            // Iterates through rows starting from row 2 (skipping header)
            for (int row = 2; row <= lastRow; row++)
            {
                // Checks if any of the first three columns have data
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text);

                if (hasData)
                {
                    // Updates the last non-empty row tracker
                    lastNonEmptyRow = row;
                }
            }

            // Returns the count of data rows (excluding header)
            return lastNonEmptyRow - 1;
        }

        /// <summary>
        /// Removes multiple parameters by their IDs for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="ids">A list of parameter IDs to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveMultiple(int tenantId, List<int> ids)
        {
            // Fetch all parameters matching the provided IDs and entity
            var parameters = await _uow.ParameterRepository.Query()
                .Where(f => ids.Contains(f.ParameterId) && f.TenantId == tenantId)
                .ToListAsync();

            foreach (var item in parameters)
            {
                if (item == null)
                    continue;

                int paramId = item.ParameterId;
                string paramName = item.ParameterName!;

                // Remove ApiParameterMaps related to this parameter
                var apiParameterMaps = await _uow.ApiParameterMapsRepository.Query()
                    .Where(f => f.ParameterId == paramId)
                    .ToListAsync();

                if (apiParameterMaps.Count != 0)
                    _uow.ApiParameterMapsRepository.RemoveRange(apiParameterMaps);

                // Remove ApiParameters related to this parameter name
                var relatedApiParameters = await _uow.ApiParametersRepository.Query()
                    .Where(f => f.ParameterName == paramName)
                    .ToListAsync();

                if (relatedApiParameters.Count != 0)
                    _uow.ApiParametersRepository.RemoveRange(relatedApiParameters);

                // Finally, remove the parameter itself
                _uow.ParameterRepository.Remove(item);
            }

            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets parameters by product for a specific entity and product ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <returns>A list of ParameterModel for the specified entity and product ID.</returns>
        public List<ParameterModel>? GetParameterByProducts(int tenantId, int productId)
        {
            // Retrieves the product by entity ID and product ID
            var product = _uow.ProductRepository.Query()
                .FirstOrDefault(p => p.TenantId == tenantId && p.ProductId == productId);

            // Returns null if product not found
            if (product == null)
                return null;

            // Retrieves all parameters for the product's entity
            var parameters = _uow.ParameterRepository.Query()
                .Where(p => p.TenantId == product.TenantId)
                .ToList();

            // Maps parameters to ParameterModel objects
            return _mapper.Map<List<ParameterModel>?>(parameters);
        }

        /// <summary>
        /// Downloads an Excel template for parameters.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, with the Excel file as a byte array.</returns>
        public async Task<byte[]> DownloadTemplate()
        {
            // Fetches required data from services
            List<DataTypeModel> dataTypes = _dataTypeService.GetAll();
            List<ConditionModel> conditions = _conditionService.GetAll();
            //List<EntityModel> entities = _entityService.GetAll();

            // Sanitizes data type names for Excel range compatibility
            dataTypes.ForEach(dataType => dataType.DataTypeName = SanitizeRangeName(dataType.DataTypeName ?? ""));

            // Sets up Excel package
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Parameter");

            // Defines column headers
            string[] headers = ["ParameterName*", "ParameterType*", "ParameterTypeId*", "IsMandatory", "Field Description"];
            for (int i = 0; i < headers.Length; i++)
            {
                // Sets header values in first row
                sheet.Cells[1, i + 1].Value = headers[i];
            }

            // Adds auxiliary columns for data validation
            sheet.Cells[1, 16].Value = "DataTypeName";
            sheet.Cells[1, 17].Value = "DataTypeId";
            sheet.Cells[1, 19].Value = "ConditionValue";
            sheet.Cells[1, 20].Value = "ConditionId";
            sheet.Cells[1, 22].Value = "EntityName";
            sheet.Cells[1, 23].Value = "TenantId";
            sheet.Cells[1, 24].Value = "FactorOptions";
            sheet.Cells[1, 25].Value = "FactorOrderValues";

            // Populates factor options dropdown values
            string[] factorOptions = ["True", "False"];
            for (int i = 0; i < factorOptions.Length; i++)
            {
                sheet.Cells[i + 2, 24].Value = factorOptions[i];
            }

            // Populates factor order options dropdown values
            //string[] factorOrderOptions = ["Ascending", "Descending"];
            //for (int i = 0; i < factorOrderOptions.Length; i++)
            //{
            //    sheet.Cells[i + 2, 25].Value = factorOrderOptions[i];
            //}

            //// Applies conditional formatting to gray out dependent fields
            //for (int row = 2; row <= 100; row++)
            //{
            //    // Gray out FactorOrder column when Factor is False
            //    var factorOrderCell = sheet.Cells[row, 5];
            //    var condition = $"$D{row}=\"False\"";
            //    var cfRule = sheet.ConditionalFormatting.AddExpression(factorOrderCell);
            //    cfRule.Style.Fill.BackgroundColor.Color = System.Drawing.Color.LightGray;
            //    cfRule.Formula = condition;

            //    // Gray out Conditions column when Factor is False
            //    var conditionsCell = sheet.Cells[row, 7];
            //    var cfRule2 = sheet.ConditionalFormatting.AddExpression(conditionsCell);
            //    cfRule2.Style.Fill.BackgroundColor.Color = System.Drawing.Color.LightGray;
            //    cfRule2.Formula = condition;
            //}

            // Adds description for required fields
            sheet.Cells[2, 5].Value = "* Fields marked with an asterisk are required.";
            sheet.Cells[2, 5].Style.Font.Bold = true;
            sheet.Cells[2, 5].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Populates auxiliary columns with data
            PopulateColumn(sheet, [.. dataTypes.Select(d => d.DataTypeName ?? "")], 16);
            PopulateColumn(sheet, [.. dataTypes.Select(d => d.DataTypeId.ToString() ?? "")], 17);
            PopulateColumn(sheet, [.. conditions.Select(c => c.ConditionValue ?? "")], 19);
            PopulateColumn(sheet, [.. conditions.Select(c => c.ConditionId.ToString())], 20);
            //PopulateColumn(sheet, [.. entities.Select(e => e.EntityName ?? "")], 22);
            //PopulateColumn(sheet, [.. entities.Select(e => e.TenantId.ToString())], 23);

            // Applies dropdown validations to various columns
            ApplyDropdown(sheet, "DataTypeNameRange", "B", 16, 100);
            //ApplyDropdownWithCondition(sheet, "FactorOrderRange", "E", 25, factorOrderOptions.Length, "D");
            //ApplyDropdownWithCondition(sheet, "ConditionValueRange", "G", 19, conditions.Count, "D");
            //ApplyDropdown(sheet, "FactorRange", "D", 24, 100);
            //ApplyDropdown(sheet, "EntityNameRange", "D", 22, 100);

            // Adds formulas for ID lookups
            AddFormula(sheet, "C", "B", 16, 17, 100);
            //AddFormula(sheet, "E", "D", 22, 23, 100);
            //AddFormula(sheet, "J", "I", 22, 23, 100);
            string[] isMandatoryOptions = ["True", "False"];
            for (int i = 0; i < isMandatoryOptions.Length; i++)
            {
                sheet.Cells[i + 2, 24].Value = isMandatoryOptions[i];
            }
            var mandatoryRange = sheet.Cells[2, 24, 3, 24];
            sheet.Workbook.Names.Add("IsMandatoryRange", mandatoryRange);
            // Adds formula for FactorOrder abbreviation
            for (int row = 2; row <= 100; row++)
            {
                sheet.Cells[row, 6].Formula = $"IF(E{row}=\"Ascending\", \"Asc\", IF(E{row}=\"Descending\", \"Des\", \"\"))";
            }

            // Hides ID columns
            //sheet.Column(3).Hidden = true;
            //sheet.Column(6).Hidden = true;
            //sheet.Column(8).Hidden = true;
            //sheet.Column(10).Hidden = true;
            ApplyDropdown(sheet, "IsMandatoryRange", "D", 24, 100);
            // Auto-fits columns and returns the file as byte array
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// Applies conditional dropdown validation to a worksheet column.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="rangeName">The name of the range for dropdown options.</param>
        /// <param name="column">The column letter to apply validation to.</param>
        /// <param name="dataColumnIndex">The column index containing dropdown data.</param>
        /// <param name="dataCount">The number of data items.</param>
        /// <param name="dependentColumn">The column that controls the dropdown enablement.</param>
        private static void ApplyDropdownWithCondition(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int dataCount, string dependentColumn)
        {
            // Defines the range for dropdown options
            var range = sheet.Cells[2, dataColumnIndex, dataCount + 1, dataColumnIndex];
            sheet.Workbook.Names.Add(rangeName, range);

            // Applies conditional dropdown validation to each row
            for (int row = 2; row <= 100; row++)
            {
                var validation = sheet.DataValidations.AddListValidation($"{column}{row}");
                validation.Formula.ExcelFormula = $"IF(${dependentColumn}{row}=\"False\", \"\", {rangeName})";
                validation.ShowErrorMessage = true;
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                validation.ErrorTitle = "Invalid Selection";
                validation.Error = "This field is disabled because Factor is set to False.";
            }
        }

        /// <summary>
        /// Populates a worksheet column with values.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="values">The values to populate.</param>
        /// <param name="columnIndex">The column index to populate.</param>
        private static void PopulateColumn(ExcelWorksheet sheet, string[] values, int columnIndex)
        {
            // Handles empty values array
            if (values.Length == 0)
            {
                sheet.Cells[2, columnIndex].Value = "";
                return;
            }

            // Populates column with values
            for (int i = 0; i < values.Length; i++)
            {
                sheet.Cells[i + 2, columnIndex].Value = values[i];
            }
        }

        /// <summary>
        /// Applies dropdown validation to a worksheet column.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="rangeName">The name of the range for dropdown options.</param>
        /// <param name="column">The column letter to apply validation to.</param>
        /// <param name="dataColumnIndex">The column index containing dropdown data.</param>
        /// <param name="maxRows">The maximum number of rows to apply validation to.</param>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int maxRows)
        {
            // Determines the last row with data
            int lastRow = sheet.Cells[sheet.Dimension.Start.Row, dataColumnIndex, sheet.Dimension.End.Row, dataColumnIndex]
                .Where(c => c.Value != null).Count();

            // Ensures at least one value exists for dropdown
            if (lastRow == 0)
            {
                sheet.Cells[2, dataColumnIndex].Value = "";
                lastRow = 2;
            }

            // Defines the range and adds it to workbook names
            var range = sheet.Cells[2, dataColumnIndex, lastRow, dataColumnIndex];
            sheet.Workbook.Names.Add(rangeName, range);

            // Applies dropdown validation to each row
            for (int row = 2; row <= maxRows; row++)
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
        /// Adds VLOOKUP formulas to a worksheet column.
        /// </summary>
        /// <param name="sheet">The worksheet to modify.</param>
        /// <param name="resultColumn">The column to place the formula results.</param>
        /// <param name="lookupColumn">The column containing the lookup value.</param>
        /// <param name="dataStartColumn">The starting column of the lookup range.</param>
        /// <param name="idColumn">The column containing the ID values.</param>
        /// <param name="dataCount">The number of data rows.</param>
        private static void AddFormula(ExcelWorksheet sheet, string resultColumn, string lookupColumn, int dataStartColumn, int idColumn, int dataCount)
        {
            // Defines the lookup range address
            string rangeAddress = sheet.Cells[2, dataStartColumn, dataCount + 1, idColumn].Address;

            // Adds VLOOKUP formula to each row
            for (int row = 2; row <= 100; row++)
            {
                sheet.Cells[row, resultColumn[0] - 'A' + 1].Formula = $"IF({lookupColumn}{row}=\"\", \"\", VLOOKUP({lookupColumn}{row}, {rangeAddress}, 2, FALSE))";
            }
        }

        /// <summary>
        /// Sanitizes a string for use as an Excel range name.
        /// </summary>
        /// <param name="name">The name to sanitize.</param>
        /// <returns>The sanitized range name.</returns>
        private static string SanitizeRangeName(string name)
        {
            // Replaces spaces with underscores
            string sanitized = name.Replace(" ", "_");

            // Replaces invalid characters with underscores
            sanitized = MyRegex().Replace(sanitized, "_");

            // Adds underscore prefix if name starts with digit
            if (char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }

            return sanitized;
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"[^A-Za-z0-9_]")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
    }
}