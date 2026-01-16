using System.ComponentModel;
using System.Drawing;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using CsvHelper;
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
    /// Service class for managing Pcard (Product Card) records.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PcardService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="ecardService">The ecard service instance.</param>
    /// <param name="productService">The product service instance.</param>
    public partial class PcardService(IUnitOfWork uow, IMapper mapper, IEcardService ecardService, IProductService productService) : IPcardService
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
        /// The ecard service instance for ECard operations.
        /// </summary>
        private readonly IEcardService _ecardService = ecardService;

        /// <summary>
        /// The product service instance for Product operations.
        /// </summary>
        private readonly IProductService _productService = productService;

        /// <summary>
        /// Adds a new Pcard record to the database.
        /// </summary>
        /// <param name="model">The PcardAddUpdateModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation, with a result message.</returns>
        public async Task<string> Add(PcardAddUpdateModel model)
        {
            // Initialize result message
            var resultMessage = "";
            // Get existing Pcards with same product and entity
            var products = _uow.PcardRepository.Query().Where(f => f.ProductId == model.ProductId && f.TenantId == model.TenantId).ToList();
            // Handle null product ID
            model.ProductId = model.ProductId == 0 ? (int?)null : model.ProductId;
            // Map model to entity
            var pcardEntities = _mapper.Map<Pcard>(model);
            try
            {
                // Check for duplicate product associations
                foreach (var product in products)
                {
                    var exits = product.ProductId == model.ProductId;
                    if (exits)
                    {
                        // Return error message if product already associated
                        return resultMessage = $"This product is already associated with another Pcards record. Please select a different product.";
                    }
                }
                // Set timestamps
                pcardEntities.UpdatedByDateTime = DateTime.UtcNow;
                pcardEntities.CreatedByDateTime = DateTime.UtcNow;
                //pcardEntities.UpdatedByDateTime = DateTime.UtcNow;
                // Add entity to repository
                _uow.PcardRepository.Add(pcardEntities);
                // Commit changes to database
                await _uow.CompleteAsync();
                // Set success message
                resultMessage = GlobalcConstants.Success;
            }
            catch (Exception ex)
            {
                // Set error message
                resultMessage = ex.Message;
            }
            // Return result message
            return resultMessage;
        }

        /// <summary>
        /// Deletes a Pcard record by its entity ID and Pcard ID.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="id">The Pcard ID to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int entityId, int id)
        {
            // Find Pcard by ID and entity
            var Item = _uow.PcardRepository.Query().First(f => f.PcardId == id && f.TenantId == entityId);
            // Remove entity from repository
            _uow.PcardRepository.Remove(Item);
            // Commit changes to database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all Pcard records for a specific entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <returns>A list of PcardListModel representing all records for the entity.</returns>
        public List<PcardListModel> GetAll(int entityId)
        {
            // Get Pcards by entity ID
            var pcard = _uow.PcardRepository.Query().Where(f => f.TenantId == entityId);
            // Map entities to models and return
            return _mapper.Map<List<PcardListModel>>(pcard);
        }

        /// <summary>
        /// Gets a Pcard record by its entity ID and Pcard ID.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="id">The Pcard ID to retrieve.</param>
        /// <returns>The PcardListModel for the specified entity and Pcard ID.</returns>
        public PcardListModel GetById(int entityId, int id)
        {
            // Find Pcard by ID and entity
            var pcard = _uow.PcardRepository.Query().First(f => f.PcardId == id && f.TenantId == entityId);
            // Map entity to model and return
            return _mapper.Map<PcardListModel>(pcard);
        }

        /// <summary>
        /// Updates an existing Pcard record.
        /// </summary>
        /// <param name="model">The PcardUpdateModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(PcardUpdateModel model)
        {
            // Find existing Pcard by ID and entity
            var Item = await _uow.PcardRepository.Query().FirstOrDefaultAsync(f => f.PcardId == model.PcardId && f.TenantId == model.TenantId);
            // Check if product is already associated with another Pcard
            var existProduct = _uow.PcardRepository.Query().Any(p => p.ProductId == model.ProductId && p.PcardId != model.PcardId);
            // Initialize result message
            //var resultMessage = "";
            if (existProduct)
            {
                // Throw exception if product already associated
                throw new Exception("This product is already associated with another Pcards record. Please select a different product.");
            }
            // Map updated data to existing entity
            if (Item == null)
            {
                throw new InvalidOperationException("Pcard entity not found in database");
            }
            var createdBy = Item.CreatedBy;
            var pcardEntities = _mapper.Map<PcardUpdateModel, Pcard>(model, Item);
            Item.CreatedBy = createdBy;
            // Set update timestamp
            pcardEntities.UpdatedByDateTime = DateTime.UtcNow;
            // Update entity in repository
            _uow.PcardRepository.Update(pcardEntities);
            // Commit changes to database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Exports selected Pcard records to an Excel file.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="selectedPcardIds">A list of selected Pcard IDs to export.</param>
        /// <returns>A task that represents the asynchronous operation, with a stream containing the exported Excel file.</returns>
        public async Task<Stream> ExportPCards(int entityId, List<int> selectedPcardIds)
        {
            // Query Pcards with joins to related entities
            var Pcards = from pcard in _uow.PcardRepository.Query()
                         join product in _uow.ProductRepository.Query()
                         on pcard.ProductId equals product.ProductId
                         join entity in _uow.EntityRepository.Query()
                         on pcard.TenantId equals entity.EntityId into entityGroup
                         from entity in entityGroup.DefaultIfEmpty() // LEFT JOINs
                         where pcard.TenantId == entityId && product.TenantId == entityId && entity.EntityId == entityId
                         select new PcardCsvModel
                         {
                             PcardId = pcard.PcardId,
                             PcardName = pcard.PcardName,
                             PcardDesc = pcard.PcardDesc,
                             Expression = pcard.Expression,
                             //TenantId = pcard.TenantId,
                             //EntityName = entity != null ? entity.EntityName : null,
                             ProductId = pcard.ProductId,
                             ProductName = product.ProductName,
                             Expshown = pcard.Expshown,
                             Pstatus = pcard.Pstatus
                         };

            // If selectedTenantIds is not null and contains items, filter by IDs
            if (selectedPcardIds != null && selectedPcardIds.Count > 0)
            {
                // Filter by selected Pcard IDs
                Pcards = Pcards.Where(query => selectedPcardIds.Contains(query.PcardId));
            }

            // Execute query and get results
            var PCards = await Pcards.ToListAsync();
            // Map entities to CSV models
            var models = _mapper.Map<List<PcardCsvModel>>(PCards);

            // Set Excel license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Create new Excel package
            using var package = new ExcelPackage();
            // Add worksheet
            var worksheet = package.Workbook.Worksheets.Add("Pcard");

            // Get properties of CSV model
            var properties = typeof(PcardCsvModel).GetProperties();
            // Add headers to worksheet
            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
            }

            // Add data to worksheet
            for (int row = 0; row < models.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(models[row]);
                }
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Create memory stream
            var memoryStream = new MemoryStream();
            // Save package to stream
            package.SaveAs(memoryStream);
            // Reset stream position
            memoryStream.Position = 0;
            // Return stream
            return memoryStream;
        }

        /// <summary>
        /// Imports Pcard records from a CSV file into the database.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="fileStream">The file stream containing the CSV data to import.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ImportEntities(int entityId, Stream fileStream)
        {
            // Create stream reader
            using var reader = new StreamReader(fileStream);
            // Create CSV reader
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            // Read records from CSV
            var models = csv.GetRecords<PcardModel>().ToList();

            // Process each model
            foreach (var model in models)
            {
                // Set entity ID
                model.TenantId = entityId;
                // Map model to entity
                var entity = _mapper.Map<Pcard>(model);
                // Add entity to repository
                _uow.PcardRepository.Add(entity);
            }

            // Commit changes to database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Removes multiple Pcard records by their IDs for a specific entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="ids">A list of Pcard IDs to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveMultiple(int entityId, List<int> ids)
        {
            // Process each ID
            foreach (var id in ids)
            {
                // Find Pcard by ID and entity
                var item = _uow.PcardRepository.Query().First(f => f.PcardId == id && f.TenantId == entityId);
                if (item != null)
                {
                    // Remove entity from repository
                    _uow.PcardRepository.Remove(item);
                }
            }
            // Commit changes to database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Imports Pcard records from an Excel file into the database.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="fileStream">The file stream containing the Excel data to import.</param>
        /// <param name="createdBy">The user who is performing the import operation.</param>
        /// <returns>A task that represents the asynchronous operation, with a string message describing the result.</returns>
        public async Task<string> ImportPCards(int entityId, Stream fileStream, string createdBy)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];

            int rowCount = GetRowCount(worksheet);
            var models = new List<PcardListModel>();

            int skippedRecordsCount = 0;
            int insertedRecordsCount = 0;
            int duplicatedRecordsCount = 0;

            string resultMessage = "";

            // Define expected headers
            string[] requiredHeaders = [
                "StreamCardName*",
        "StreamCardDescription*",
        "StreamName*",
        "StreamId*",
        "ExpressionShown*"
            ];

            // Validate headers
            var excelHeaders = new List<string>();
            for (int col = 1; col <= requiredHeaders.Length; col++)
                excelHeaders.Add(worksheet.Cells[1, col].Text?.Trim()!);

            for (int i = 0; i < requiredHeaders.Length; i++)
            {
                if (!excelHeaders[i].Equals(requiredHeaders[i], StringComparison.OrdinalIgnoreCase))
                    return $"Incorrect file format at Column {i + 1}. Expected '{requiredHeaders[i]}' but found '{excelHeaders[i]}'";
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
       p.ProductName.ToLower() == ProductName.ToLower())
.FirstOrDefaultAsync();

                    if (product == null)
                    {
                        skippedRecordsCount++;
                        resultMessage += $"Provide correct Stream name '{ProductName}' for PCard '{PcardName}'. ";
                        continue;
                    }

                    // Check if PCard already exists
                    var alreadyExist = _uow.PcardRepository.Query().Any(p => p.ProductId == productId);
                    if (alreadyExist)
                    {
                        resultMessage += $"Stream '{ProductName}' for Stream Card '{PcardName}' is already associated with another Stream Card. ";
                        continue;
                    }

                    // Create model
                    models.Add(new PcardListModel
                    {
                        PcardName = PcardName,
                        PcardDesc = PcardDesc,
                        Expshown = ExpressionShown,
                        CreatedBy = createdBy,
                        UpdatedBy = createdBy,
                        ProductId = productId,
                        TenantId = entityId
                    });
                }

                // Process models for insertion
                foreach (var model in models)
                {
                    // Build final expression
                    var finalExpression = await BuildPCardExpressionFromShown(model.Expshown!, _uow);
                    if (finalExpression == null)
                    {
                        skippedRecordsCount++;
                        resultMessage += $"Invalid ExpressionShown for Stream Card '{model.PcardName}'. Please check expression. ";
                        continue;
                    }

                    model.Expression = finalExpression;
                    model.CreatedByDateTime = DateTime.UtcNow;
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    model.UpdatedBy = createdBy;
                    model.IsImport = true;

                    _uow.PcardRepository.Add(_mapper.Map<Pcard>(model));
                    insertedRecordsCount++;
                }

                await _uow.CompleteAsync();

                // Build result message
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
        /// Gets the number of rows with data in the specified worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet to evaluate.</param>
        /// <returns>The number of rows with data.</returns>
        private static int GetRowCount(ExcelWorksheet worksheet)
        {
            // Get last row in worksheet
            int lastRow = worksheet.Dimension.End.Row;
            // Initialize row count
            //int rowCount = 0;
            // Initialize last non-empty row
            int lastNonEmptyRow = 0;

            // Check each row for data
            for (int row = 2; row <= lastRow; row++)
            {
                // Check if row has data in specified columns
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                 !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                 !string.IsNullOrWhiteSpace(worksheet.Cells[row, 4].Text) ||
                 !string.IsNullOrWhiteSpace(worksheet.Cells[row, 5].Text);

                if (hasData)
                {
                    // Track the last row that has data
                    lastNonEmptyRow = row;
                }
            }

            // Return last non-empty row
            return lastNonEmptyRow - 1;
        }

        /// <summary>
        /// Downloads an Excel template for Pcard records.
        /// </summary>
        /// <param name="entityId">The entity ID for which to generate the template.</param>
        /// <returns>A task that represents the asynchronous operation, with the Excel file as a byte array.</returns>
        public async Task<byte[]> DownloadTemplate(int entityId)
        {
            // Fetch all ecards and products for the entity
            List<EcardListModel> ecard = _ecardService.GetAll(entityId);
            List<ProductListModel> product = _productService.GetAll(entityId);

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("PCards");

            // Delegate Excel sheet population to the helper method
            PopulatePCardsTemplate(sheet, ecard, product);

            // Return as byte array
            return await Task.FromResult(package.GetAsByteArray());
        }

        // Helper method: separates template logic
        private static void PopulatePCardsTemplate(ExcelWorksheet sheet, List<EcardListModel> ecard, List<ProductListModel> product)
        {
            // Create VBA Worksheet Change event
            StringBuilder changeEvent = new();
            changeEvent.Append("Private Sub Worksheet_Change(ByVal Target As Range)\n")
                .Append("     If Target.Column >= 3 And Target.Column <= 6 And Target.Value <> \"\" Then\n")
                .Append("          PCards.Cells(Target.Row, 7).Value = PCards.Cells(Target.Row, 7).Value + \" \" + Target.Value\n")
                .Append("          Target.Value = \"\"\n")
                .Append("     End If\n")
                .Append("End Sub");

            if (sheet.CodeModule != null)
            {
                sheet.CodeModule.Code = changeEvent.ToString();
            }
            // Define headers
            string[] headers =
     [
    "StreamCardName*",
    "StreamCardDescription*",
    "StreamName*",
    "StreamId*",
    "ExpressionShown*"
     ];

            for (int i = 0; i < headers.Length; i++)
                sheet.Cells[1, i + 1].Value = headers[i];

            // Field Description column
            sheet.Cells[1, 6].Value = "Field Description";

            // Hidden supporting columns
            sheet.Cells[1, 10].Value = "OpenParenthesisValue";
            sheet.Cells[1, 11].Value = "ECardName";
            sheet.Cells[1, 12].Value = "StreamName";
            sheet.Cells[1, 13].Value = "StreamId";
            sheet.Cells[1, 15].Value = "LogicalOperatorValue";
            sheet.Cells[1, 17].Value = "CloseParenthesisValue";

            // Required fields description
            sheet.Cells[2, 6].Value = "* Fields marked with an asterisk are required.";
            sheet.Cells[2, 6].Style.Font.Bold = true;
            sheet.Cells[2, 6].Style.Font.Color.SetColor(Color.Red);

            // Dropdown supporting values
            string[] openParenthesisOptions = ["("];
            for (int i = 0; i < openParenthesisOptions.Length; i++)
                sheet.Cells[i + 2, 10].Value = openParenthesisOptions[i];

            string[] logicalOperatorOptions = ["And", "Or"];
            for (int i = 0; i < logicalOperatorOptions.Length; i++)
                sheet.Cells[i + 2, 15].Value = logicalOperatorOptions[i];

            string[] closeParenthesisOptions = [")"];
            for (int i = 0; i < closeParenthesisOptions.Length; i++)
                sheet.Cells[i + 2, 17].Value = closeParenthesisOptions[i];

            // Populate hidden reference columns
            PopulateColumn(sheet, [.. ecard.Select(d => d.EcardName ?? "")], 11);
            PopulateColumn(sheet, [.. product.Select(d => d.ProductName ?? "")], 12);
            PopulateColumn(sheet, [.. product.Select(d => d.ProductId.ToString())], 13);

            // Apply dropdowns and formulas
            ApplyDropdown(sheet, "ecardNameRange", "C", 12, ecard.Count);
            AddFormula(sheet, "D", "C", 12, 13, 100);

            // Auto-fit columns
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
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
                // Ensures at least one blank entry for dropdown functionality
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
        /// <param name="dataCount">The number of data rows to include in the dropdown.</param>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int dataCount)
        {
            ExcelRange range;
            // Creates range based on data count
            if (dataCount > 0)
            {
                range = sheet.Cells[2, dataColumnIndex, dataCount + 1, dataColumnIndex];
            }
            else
            {
                // Ensures a blank cell exists for dropdown
                sheet.Cells[2, dataColumnIndex].Value = "";
                range = sheet.Cells[2, dataColumnIndex, 2, dataColumnIndex];
            }
            // Adds named range to workbook
            sheet.Workbook.Names.Add(rangeName, range);

            // Applies dropdown validation to each row in the target column
            for (int row = 2; row <= 100; row++)
            {
                var validation = sheet.DataValidations.AddListValidation($"{column}{row}");
                validation.Formula.ExcelFormula = rangeName;
                validation.ShowErrorMessage = true;
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                validation.ErrorTitle = "Invalid Selection";
                validation.Error = "Please select a valid option.";
            }
        }
        [GeneratedRegex(@"\s+(AND|OR)\s+", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex3();
    }
}
