using System.ComponentModel;
using AutoMapper;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Entities;
using EligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace EligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing product parameters.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProductParamService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="entityService">The entity service instance.</param>
    /// <param name="productService">The product service instance.</param>
    /// <param name="parameterService">The parameter service instance.</param>
    /// <param name="factorService">The factor service instance.</param>
    public class ProductParamService(IUnitOfWork uow, IMapper mapper/*, IEntityService entityService*/, IProductService productService, IParameterService parameterService, IFactorService factorService) : IProductParamservice
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
        /// The entity service instance for entity operations.
        /// </summary>
        //private readonly IEntityService _entityService = entityService;

        /// <summary>
        /// The product service instance for product operations.
        /// </summary>
        private readonly IProductService _productService = productService;

        /// <summary>
        /// The parameter service instance for parameter operations.
        /// </summary>
        private readonly IParameterService _parameterService = parameterService;

        /// <summary>
        /// The factor service instance for factor operations.
        /// </summary>
        private readonly IFactorService _factorService = factorService;

        /// <summary>
        /// Adds a new product parameter to the database.
        /// </summary>
        /// <param name="model">The ProductParamAddUpdateModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(ProductParamAddUpdateModel model)
        {
            // Checks if TenantId is zero
            if (model.TenantId == 0)
            {
                // Throws exception if TenantId is zero
                throw new InvalidOperationException("The TenantId does not exist in the database.");
            }
            // Checks if a record with the same ProductId and ParameterId already exists
            var existingEntity = await _uow.ProductParamRepository.Query().AnyAsync(p => p.ProductId == model.ProductId && p.ParameterId == model.ParameterId && p.TenantId == model.TenantId);
            // Validates if entity already exists
            if (existingEntity)
            {
                // Throws exception if duplicate record found
                throw new InvalidOperationException("A record with the same Product Name and Parameter Name already exists.");
            }
            // Maps the model to ProductParam entity
            var productParamEntities = _mapper.Map<ProductParam>(model);
            // Sets the creation timestamp
            productParamEntities.CreatedByDateTime = DateTime.UtcNow;
            // Sets the first update timestamp
            productParamEntities.UpdatedByDateTime = DateTime.UtcNow;
            // Sets the second update timestamp
            productParamEntities.UpdatedByDateTime = DateTime.UtcNow;
            // Adds the entity to the repository
            _uow.ProductParamRepository.Add(productParamEntities);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all product parameters for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A task representing the asynchronous operation, with a list of ProductParamListModel.</returns>
        public async Task<List<ProductParamListModel>> GetAll(int tenantId)
        {
            // Queries product parameters with joins to product and parameter tables
            var productParams = from pp in _uow.ProductParamRepository.Query()
                                join p in _uow.ProductRepository.Query() on pp.ProductId equals p.ProductId
                                join param in _uow.ParameterRepository.Query() on pp.ParameterId equals param.ParameterId
                                where pp.TenantId == tenantId
                                select new ProductParamListModel
                                {
                                    CreatedBy = pp.CreatedBy,
                                    CreatedByDateTime = pp.CreatedByDateTime,
                                    ProductId = pp.ProductId,
                                    ParameterId = pp.ParameterId,
                                    DisplayOrder = pp.DisplayOrder,
                                    TenantId = pp.TenantId,
                                    ParamValue = pp.ParamValue,
                                    IsRequired = pp.IsRequired,
                                    ProductName = p.ProductName,
                                    ParameterName = param.ParameterName,
                                    UpdatedBy = pp.UpdatedBy,
                                    UpdatedByDateTime = pp.UpdatedByDateTime
                                };

            // Returns the list of product parameters
            return await productParams.ToListAsync();
        }

        /// <summary>
        /// Gets a product parameter by product ID, parameter ID, and entity ID.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="parameterId">The parameter ID.</param>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>The ProductParamListModel for the specified IDs.</returns>
        public ProductParamListModel GetById(int productId, int parameterId, int tenantId)
        {
            // Finds the product parameter by composite key
            var product = _uow.ProductParamRepository.Query().FirstOrDefault(pp => pp.ProductId == productId && pp.ParameterId == parameterId && pp.TenantId == tenantId);
            // Maps the entity to list model and returns
            return _mapper.Map<ProductParamListModel>(product);
        }

        /// <summary>
        /// Gets a product parameter by product ID and entity ID.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>The ProductParamListModel for the specified product and entity ID.</returns>
        public ProductParamListModel GetByProductId(int productId, int tenantId)
        {
            // Queries product parameter with joins to product and parameter tables
            var product = (from pp in _uow.ProductParamRepository.Query()
                           join p in _uow.ProductRepository.Query() on pp.ProductId equals p.ProductId
                           join param in _uow.ParameterRepository.Query() on pp.ParameterId equals param.ParameterId
                           where pp.ProductId == productId && pp.TenantId == tenantId
                           select new ProductParamListModel
                           {
                               ProductId = pp.ProductId,
                               ParameterId = pp.ParameterId,
                               DisplayOrder = pp.DisplayOrder,
                               TenantId = pp.TenantId,
                               ParamValue = pp.ParamValue,
                               IsRequired = pp.IsRequired,
                               ProductName = p.ProductName,
                               ParameterName = param.ParameterName
                           }).FirstOrDefault();
            // Maps the entity to list model and returns
            return _mapper.Map<ProductParamListModel>(product);
        }

        /// <summary>
        /// Updates an existing product parameter.
        /// </summary>
        /// <param name="model">The ProductParamAddUpdateModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(ProductParamAddUpdateModel model)
        {
            // Checks if TenantId is zero
            if (model.TenantId == 0)
            {
                // Throws exception if TenantId is zero
                throw new InvalidOperationException("The TenantId does not exist in the database.");
            }

            // Finds the existing product parameter
            var Item = await _uow.ProductParamRepository.Query().FirstOrDefaultAsync(pp => pp.ProductId == model.ProductId && pp.ParameterId == model.ParameterId) ?? throw new InvalidOperationException("Cannot update the parameter and product value.");
            // Maps the model to existing entity
            var productparamEnitties = _mapper.Map<ProductParamAddUpdateModel, ProductParam>(model, Item);
            // Sets the first update timestamp
            productparamEnitties.UpdatedByDateTime = DateTime.UtcNow;
            // Sets the second update timestamp
            productparamEnitties.UpdatedByDateTime = DateTime.UtcNow;
            // Updates the entity in the repository
            _uow.ProductParamRepository.Update(_mapper.Map<ProductParamAddUpdateModel, ProductParam>(model, Item));
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a product parameter by product ID, parameter ID, and entity ID.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="parameterId">The parameter ID.</param>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int productId, int parameterId, int tenantId)
        {
            // Finds the product parameter to delete
            var Item = await _uow.ProductParamRepository.Query().FirstOrDefaultAsync(pm => pm.ProductId == productId && pm.ParameterId == parameterId && pm.TenantId == tenantId);
            // Removes the entity from the repository
            if (Item != null)
            {
                _uow.ProductParamRepository.Remove(Item);
            }
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple product parameters by their product IDs, parameter IDs, and entity ID.
        /// </summary>
        /// <param name="productIds">A list of product IDs.</param>
        /// <param name="parameterIds">A list of parameter IDs.</param>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MultipleDelete(List<int> productIds, List<int> parameterIds, int tenantId)
        {
            // Finds all product parameters matching the criteria
            var items = _uow.ProductParamRepository.Query()
                .Where(pm => productIds.Contains(pm.ProductId)
                          && parameterIds.Contains(pm.ParameterId)
                          && pm.TenantId == tenantId)
                .ToList();

            // Removes the range of entities from the repository
            _uow.ProductParamRepository.RemoveRange(items);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Imports product parameter details from an Excel file into the database.
        /// </summary>
        /// <param name="fileStream">The file stream containing the Excel data to import.</param>
        /// <param name="createdBy">The user who is performing the import operation.</param>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A task that represents the asynchronous operation, with a string message describing the result.</returns>
        public async Task<string> ImportDetails(Stream fileStream, string createdBy, int tenantId)
        {
            // Sets the EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new Excel package from the file stream
            using var package = new ExcelPackage(fileStream);
            // Gets the first worksheet
            var worksheet = package.Workbook.Worksheets[0];
            // Gets the row count of the worksheet
            int rowCount = GetRowCount(worksheet);
            // Initializes a list for product parameter models
            var models = new List<ProductParam>();
            // Initializes counter for skipped records
            int skippedRecordsCount = 0;
            // Initializes counter for inserted records
            int insertedRecordsCount = 0;
            // Initializes counter for duplicated records
            int dublicatedRecordsCount = 0;
            // Initializes the result message
            var resultMessage = "";

            // Begins try block for error handling
            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    // Returns message for empty file
                    return resultMessage = "Uploaded File Is Empty";
                }
                // Reads data from the Excel file into models
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Gets product ID from cell
                    var productId = worksheet.Cells[row, 2].Text;
                    // Gets entity ID from cell
                    var tenantIdFromFile = worksheet.Cells[row, 4].Text;
                    // Gets parameter ID from cell
                    var parameterId = worksheet.Cells[row, 6].Text;
                    // Gets parameter value from cell
                    var paramValue = worksheet.Cells[row, 7].Text;
                    // Gets display order from cell
                    var DisplayOrder = worksheet.Cells[row, 8].Text;
                    // Gets is required flag from cell
                    var IsRequired = worksheet.Cells[row, 9].Text;

                    // Check if required fields are empty
                    if (!int.TryParse(productId, out _) || !int.TryParse(tenantIdFromFile, out _) || !int.TryParse(parameterId, out _) || string.IsNullOrEmpty(paramValue) || string.IsNullOrEmpty(DisplayOrder) || !bool.TryParse(IsRequired, out _))
                    {
                        // Increments skipped records counter
                        skippedRecordsCount++;
                        // Skips the record if any required field is missing
                        continue;
                    }

                    // Creates a new product parameter model
                    var model = new ProductParam
                    {
                        ProductId = int.TryParse(productId, out int ProductId) ? ProductId : 0,
                        TenantId = int.TryParse(tenantIdFromFile, out int TenantId) ? TenantId : 0,
                        ParameterId = int.TryParse(parameterId, out int ParameterId) ? ParameterId : 0,
                        ParamValue = paramValue,
                        CreatedBy = createdBy,
                        DisplayOrder = int.TryParse(DisplayOrder, out int displayOrder) ? displayOrder : 0,
                        IsRequired = bool.TryParse(IsRequired, out bool isRequired) && isRequired,
                    };
                    // Adds the model to the list
                    models.Add(model);
                }

                // Processes each model for insertion
                foreach (var model in models)
                {
                    // Checks if a duplicate record exists
                    var existingEntity = await _uow.ProductParamRepository.Query().AnyAsync(p => p.ProductId == model.ProductId && p.ParameterId == model.ParameterId && p.TenantId == tenantId);
                    // Validates if entity already exists
                    if (existingEntity)
                    {
                        // Increments duplicated records counter
                        dublicatedRecordsCount++;
                        // Skips the duplicate record
                        continue;
                    }

                    // Sets the update timestamp
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    // Sets the creation timestamp
                    model.CreatedByDateTime = DateTime.UtcNow;
                    // Sets the update timestamp again
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    // Adds the mapped entity to the repository
                    _uow.ProductParamRepository.Add(_mapper.Map<ProductParam>(model));
                    // Increments inserted records counter
                    insertedRecordsCount++;
                }

                // Commits the changes to the database
                await _uow.CompleteAsync();

                // Builds result message based on operation results
                if (insertedRecordsCount > 0)
                {
                    // Adds success message
                    resultMessage = $"{models.Count} Details Inserted Successfully.";
                }
                // Adds skipped records message if any
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $" {skippedRecordsCount} records were not inserted because of missing required field.";
                }
                // Adds duplicated records message if any
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $" {dublicatedRecordsCount} record with the same Product Name and Parameter Name already exists.";
                }
            }
            // Catches any exceptions during import
            catch (Exception ex)
            {
                // Sets error message
                resultMessage = "Error On Details Page = " + ex.Message;
            }
            // Returns the result message
            return resultMessage;
        }

        /// <summary>
        /// Gets the number of rows with data in the specified worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet to evaluate.</param>
        /// <returns>The number of rows with data.</returns>
        static int GetRowCount(ExcelWorksheet worksheet)
        {
            // Gets the last row of the worksheet
            int lastRow = worksheet.Dimension.End.Row;
            // Initializes row counter
            //int rowCount = 0;
            // Initializes last non-empty row tracker
            int lastNonEmptyRow = 0;

            // Iterates through all rows to find non-empty ones
            for (int row = 2; row <= lastRow; row++)
            {
                // Checks if any of the first three cells have data
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text);

                // Updates last non-empty row if data found
                if (hasData)
                {
                    lastNonEmptyRow = row;
                }
            }

            // Returns the count of non-empty rows
            return lastNonEmptyRow;
        }

        /// <summary>
        /// Downloads an Excel template for product parameters.
        /// </summary>
        /// <param name="tenantId">The entity ID for which to generate the template.</param>
        /// <returns>A task that represents the asynchronous operation, with the Excel file as a byte array.</returns>
        public async Task<byte[]> DownloadTemplate(int tenantId)
        {
            // Gets all entities
            //List<EntityModel> entities = _entityService.GetAll();
            // Gets all products for the entity
            List<ProductListModel> product = _productService.GetAll(tenantId);
            // Gets all parameters for the entity
            List<ParameterListModel> parameters = _parameterService.GetAll(tenantId);
            // Gets all factors for the entity
            List<FactorListModel> factors = _factorService.GetAll(tenantId);

            // Sets the EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new Excel package
            using var package = new ExcelPackage();
            // Adds a new worksheet named "Details"
            var sheet = package.Workbook.Worksheets.Add("Details");

            // Defines headers for the template
            string[] headers = ["ProductName*", "ProductId*", "EntityName*", "TenantId*", "ParameterName*", "ParameterId*", "ParamValue*", "DisplayOrder*", "Category*", "Field Description"];
            // Sets header values in the first row
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }

            // Adds description for required fields
            sheet.Cells[2, 10].Value = "* Fields marked with an asterisk are required.";
            // Makes the description text bold
            sheet.Cells[2, 10].Style.Font.Bold = true;
            // Makes the description text red
            sheet.Cells[2, 10].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Sets header for product name reference column
            sheet.Cells[1, 13].Value = "ProductName";
            // Sets header for product ID reference column
            sheet.Cells[1, 14].Value = "ProductId";
            // Sets header for product entity ID reference column
            sheet.Cells[1, 15].Value = "ProductTenantId";

            // Sets header for entity name reference column
            sheet.Cells[1, 16].Value = "EntityName";
            // Sets header for entity ID reference column
            sheet.Cells[1, 17].Value = "TenantId";

            // Sets header for parameter name reference column
            sheet.Cells[1, 19].Value = "ParameterName";
            // Sets header for parameter ID reference column
            sheet.Cells[1, 20].Value = "ParameterId";

            // Sets header for category options reference column
            sheet.Cells[1, 22].Value = "categoryOptions";

            // Sets header for value1 reference column
            sheet.Cells[1, 24].Value = "Value1";
            // Sets header for value2 reference column
            sheet.Cells[1, 25].Value = "Value2";
            // Sets header for parameter ID reference column for factors
            sheet.Cells[1, 26].Value = "ParameterId";

            // Defines category options for dropdown
            string[] categoryOptions = ["True", "False"];
            // Populates category options in the reference column
            for (int i = 0; i < categoryOptions.Length; i++)
            {
                sheet.Cells[i + 2, 22].Value = categoryOptions[i];
            }

            // Populates static columns with data
            PopulateColumn(sheet, [.. product.Select(e => e.ProductName ?? "")], 13);
            PopulateColumn(sheet, [.. product.Select(e => e.TenantId.ToString())], 14);
            PopulateColumn(sheet, [.. product.Select(e => e.TenantId.ToString())], 15);
            //PopulateColumn(sheet, [.. entities.Select(e => e.EntityName ?? "")], 16);
            //PopulateColumn(sheet, [.. entities.Select(e => e.TenantId.ToString())], 17);
            PopulateColumn(sheet, [.. parameters.Select(e => e.ParameterName ?? "")], 19);
            PopulateColumn(sheet, [.. parameters.Select(e => e.ParameterId.ToString())], 20);
            PopulateColumn(sheet, [.. factors.Select(e => e.Value1 != null ? e.Value1.ToString() : "")], 24);
            PopulateColumn(sheet, [.. factors.Select(e => e.Value2 != null ? e.Value2.ToString() : "")], 25);
            PopulateColumn(sheet, [.. factors.Select(e => e.ParameterId.ToString() ?? "")], 26);

            // Applies dropdowns to the template columns
            ApplyDropdown(sheet, "ProductNameRange", "A", 13, 100);
            ApplyDropdown(sheet, "EntityNameRange", "C", 16, 100);
            ApplyDropdown(sheet, "ParameterNameRange", "E", 19, 100);
            ApplyDropdown(sheet, "categoryRange", "I", 22, 100);

            // Creates dynamic dropdown for ParamValue column based on ParameterId
            ApplyParamValueDropdown(sheet, factors);

            // Applies formulas to auto-populate IDs based on names
            AddFormula(sheet, "B", "A", 13, 14, 100);
            AddFormula(sheet, "D", "C", 16, 17, 100);
            AddFormula(sheet, "F", "E", 19, 20, 100);

            // Auto-fits all columns
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            // Returns the Excel package as byte array
            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// Applies a dynamic dropdown for the ParamValue column based on ParameterId and factors.
        /// </summary>
        /// <param name="sheet">The Excel worksheet to apply the dropdown to.</param>
        /// <param name="factors">The list of factors.</param>
        private static void ApplyParamValueDropdown(ExcelWorksheet sheet, List<FactorListModel> factors)
        {
            // Gets the workbook from the worksheet
            var workbook = sheet.Workbook;
            // Creates a dictionary to map ParameterId to factor values
            var paramValueMap = new Dictionary<int, List<string>>();

            // Step 1: Map ParameterId to combined Value1 and Value2 for dropdown
            foreach (var factor in factors)
            {
                // Gets the parameter ID from the factor
                int paramId = (int)factor.ParameterId!;
                // Combines Value1 and Value2 for display
                string valueCombination = factor.Value2 == "" ? factor.Value1 ?? "" : $"{factor.Value1} + {factor.Value2}";

                // Adds the value combination to the dictionary
                if (!paramValueMap.TryGetValue(paramId, out List<string>? value))
                {
                    paramValueMap[paramId] = [valueCombination];
                }
                else
                {
                    value.Add(valueCombination);
                }
            }

            // Step 2: Store param values in hidden columns
            int startColumn = 30;
            foreach (var param in paramValueMap)
            {
                // Creates a named range for each parameter
                string rangeName = $"Param_{param.Key}";
                int rowIndex = 2;

                // Populates the hidden column with factor values
                foreach (var value in param.Value)
                {
                    sheet.Cells[rowIndex, startColumn].Value = value;
                    rowIndex++;
                }

                // Creates a range for the values
                var range = sheet.Cells[2, startColumn, rowIndex - 1, startColumn];
                // Adds the named range to the workbook
                workbook.Names.Add(rangeName, range);

                startColumn++;
            }

            // Step 3: Apply dynamic dropdown using INDIRECT()
            for (int row = 2; row <= 100; row++)
            {
                // Adds data validation to the ParamValue column
                var validation = sheet.DataValidations.AddListValidation($"G{row}");
                // Sets the formula to dynamically reference the appropriate range
                validation.Formula.ExcelFormula = $"INDIRECT(\"Param_\"&F{row})";
                // Enables error message display
                validation.ShowErrorMessage = true;
                // Sets the error style to stop
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                // Sets the error title
                validation.ErrorTitle = "Invalid Selection";
                // Sets the error message
                validation.Error = "Please select a valid ParamValue.";
            }
        }

        /// <summary>
        /// Populates a column in the worksheet with the provided values.
        /// </summary>
        /// <param name="sheet">The Excel worksheet to populate.</param>
        /// <param name="values">The values to populate in the column.</param>
        /// <param name="columnIndex">The column index to populate.</param>
        private static void PopulateColumn(ExcelWorksheet sheet, string[] values, int columnIndex)
        {
            // Handles empty values array
            if (values.Length == 0)
            {
                // Adds a blank value to ensure dropdown works
                sheet.Cells[2, columnIndex].Value = "";
                return;
            }

            // Populates the column with values
            for (int i = 0; i < values.Length; i++)
            {
                sheet.Cells[i + 2, columnIndex].Value = values[i];
            }
        }

        /// <summary>
        /// Applies a dropdown list to a column in the worksheet.
        /// </summary>
        /// <param name="sheet">The Excel worksheet to apply the dropdown to.</param>
        /// <param name="rangeName">The name of the range for the dropdown.</param>
        /// <param name="column">The column letter to apply the dropdown to.</param>
        /// <param name="dataColumnIndex">The column index containing the dropdown values.</param>
        /// <param name="maxRows">The maximum number of rows to apply the dropdown to.</param>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int maxRows)
        {
            // Detect non-empty values in the data column
            int lastRow = sheet.Cells[sheet.Dimension.Start.Row, dataColumnIndex, sheet.Dimension.End.Row, dataColumnIndex]
                .Where(c => c.Value != null).Count();

            // Ensure at least one blank row is available for dropdowns
            if (lastRow == 0)
            {
                sheet.Cells[2, dataColumnIndex].Value = "";
                lastRow = 2;
            }

            // Creates a range for the dropdown values
            var range = sheet.Cells[2, dataColumnIndex, lastRow, dataColumnIndex];
            // Adds the named range to the workbook
            sheet.Workbook.Names.Add(rangeName, range);

            // Applies dropdown validation to each row in the target column
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
        /// Adds a formula to a column in the worksheet for auto-population based on a lookup.
        /// </summary>
        /// <param name="sheet">The Excel worksheet to add the formula to.</param>
        /// <param name="resultColumn">The result column letter.</param>
        /// <param name="lookupColumn">The lookup column letter.</param>
        /// <param name="dataStartColumn">The starting column index for the data range.</param>
        /// <param name="idColumn">The column index for the ID to return.</param>
        /// <param name="dataCount">The number of data rows to include in the lookup.</param>
        private static void AddFormula(ExcelWorksheet sheet, string resultColumn, string lookupColumn, int dataStartColumn, int idColumn, int dataCount)
        {
            // Creates the address for the lookup range
            string rangeAddress = sheet.Cells[2, dataStartColumn, dataCount + 1, idColumn].Address;

            // Applies the VLOOKUP formula to each row in the result column
            for (int row = 2; row <= 100; row++)
            {
                sheet.Cells[row, resultColumn[0] - 'A' + 1].Formula = $"IF({lookupColumn}{row}=\"\", \"\", VLOOKUP({lookupColumn}{row}, {rangeAddress}, 2, FALSE))";
            }
        }

        /// <summary>
        /// Exports product parameter details to an Excel file.
        /// </summary>
        /// <param name="selectedProductIds">A list of selected product IDs to export.</param>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A task that represents the asynchronous operation, with a stream containing the exported Excel file.</returns>
        public async Task<Stream> ExportDetails(List<int> selectedProductIds, int tenantId)
        {
            // Queries product details with joins to related tables
            var productDetails = from productParam in _uow.ProductParamRepository.Query()
                                 join product in _uow.ProductRepository.Query()
                                 on productParam.ProductId equals product.ProductId

                                 join parameter in _uow.ParameterRepository.Query()
                                 on productParam.ParameterId equals parameter.ParameterId

                                 join entity in _uow.EntityRepository.Query()
                                 on productParam.TenantId equals entity.EntityId into entityGroup
                                 from entity in entityGroup.DefaultIfEmpty()

                                 where productParam.TenantId == tenantId

                                 select new ProductParamDescription
                                 {
                                     ProductId = productParam.ProductId,
                                     ProductName = product.ProductName,
                                     TenantId = productParam.TenantId,
                                     EntityName = entity.EntityName ?? "",
                                     ParameterId = productParam.ParameterId,
                                     ParameterName = parameter.ParameterName,
                                     ParamValue = productParam.ParamValue,
                                     DisplayOrder = productParam.DisplayOrder,
                                     IsRequired = productParam.IsRequired
                                 };

            // Optional: Filter by selected product IDs
            if (selectedProductIds != null && selectedProductIds.Count > 0)
            {
                productDetails = productDetails.Where(query => selectedProductIds.Contains(query.ProductId));
            }

            // Executes the query and gets the results
            var productList = await productDetails.ToListAsync();
            // Maps the entities to description models
            var models = _mapper.Map<List<ProductParamDescription>>(productList);

            // Sets the EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new Excel package
            using var package = new ExcelPackage();
            // Adds a new worksheet named "Details"
            var worksheet = package.Workbook.Worksheets.Add("Details");

            // Gets the properties of the description model
            var properties = typeof(ProductParamDescription).GetProperties();
            // Sets the header row with property names
            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
            }

            // Populates the worksheet with data
            for (int row = 0; row < models.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(models[row]);
                }
            }

            // Auto-fits all columns
            worksheet.Cells.AutoFitColumns();

            // Creates a memory stream for the Excel file
            var memoryStream = new MemoryStream();
            // Saves the package to the memory stream
            package.SaveAs(memoryStream);
            // Resets the stream position
            memoryStream.Position = 0;
            // Returns the memory stream
            return memoryStream;
        }
    }
}
