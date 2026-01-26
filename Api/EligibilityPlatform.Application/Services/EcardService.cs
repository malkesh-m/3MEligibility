using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using MEligibilityPlatform.Application.Services.Inteface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing Ecard operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EcardService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="eruleService">The Erule service instance.</param>
    public partial class EcardService(IUnitOfWork uow, IMapper mapper, IEruleService eruleService, IEruleMasterService eruleMasterService) : IEcardService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;
        private readonly IEruleService _eruleService = eruleService;
        private readonly IEruleMasterService _eruleMasterService = eruleMasterService;

        /// <summary>
        /// Adds a new Ecard to the database.
        /// </summary>
        /// <param name="tenantId">The entity ID associated with the Ecard.</param>
        /// <param name="model">The EcardAddUpdateModel object to be added.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(int tenantId, EcardAddUpdateModel model)
        {
            var existingCard = _uow.EcardRepository.Query().Any(p => p.EcardName == model.EcardName && p.TenantId == tenantId);
            if (existingCard)
            {
                throw new Exception("Ecard with this name already exists");
            }
            // Maps the model to Ecard entity
            var ecardEntites = _mapper.Map<Ecard>(model);
            // Sets the entity ID for the Ecard
            ecardEntites.TenantId = tenantId;
            // Sets the update timestamp to current UTC time
            ecardEntites.UpdatedByDateTime = DateTime.UtcNow;
            // Sets the creation timestamp to current UTC time
            ecardEntites.CreatedByDateTime = DateTime.UtcNow;
            // Adds the Ecard entity to the repository
            _uow.EcardRepository.Add(ecardEntites);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes an Ecard from the database based on the provided ID.
        /// </summary>
        /// <param name="tenantId">The entity ID associated with the Ecard.</param>
        /// <param name="id">The ID of the Ecard to be deleted.</param>
        /// <returns>A string message indicating the result of the operation.</returns>
        public async Task<string> Delete(int tenantId, int id)
        {
            // Initializes the result message
            var resultMessage = "";
            // Retrieves all PCards for the specified entity
            var pCards = _uow.PcardRepository.Query().Where(f => f.TenantId == tenantId);
            try
            {
                // Checks if the Ecard is being used in any PCard
                foreach (var card in pCards)
                {
                    var exits = card.Expression.Contains(id.ToString());
                    if (exits)
                    {
                        return resultMessage += $" The Ecard cannot be deleted because it is currently being used in one or more Stream Cards.";
                    }
                }

                // Retrieves the Ecard entity by ID from the repository
                var Item = _uow.EcardRepository.Query().First(f => f.EcardId == id && f.TenantId == tenantId);
                // Removes the Ecard entity from the repository
                _uow.EcardRepository.Remove(Item);
                // Commits the changes to the database
                await _uow.CompleteAsync();
                // Sets the success message
                resultMessage = GlobalcConstants.Deleted;
            }
            catch (Exception ex)
            {
                // Sets the error message if an exception occurs
                resultMessage = ex.Message;
            }
            return resultMessage;
        }

        /// <summary>
        /// Retrieves all the Ecards from the database for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID to filter Ecards.</param>
        /// <returns>A list of EcardListModel objects representing all the Ecards.</returns>
        public List<EcardListModel> GetAll(int tenantId)
        {
            // Retrieves all Ecard entities for the specified entity ID
            var ecards = _uow.EcardRepository.Query().Where(f => f.TenantId == tenantId);
            // Maps the Ecard entities to EcardListModel objects and returns the list
            return _mapper.Map<List<EcardListModel>>(ecards);
        }

        /// <summary>
        /// Retrieves a specific Ecard by its ID.
        /// </summary>
        /// <param name="tenantId">The entity ID associated with the Ecard.</param>
        /// <param name="id">The ID of the Ecard to be retrieved.</param>
        /// <returns>An EcardListModel object representing the Ecard with the specified ID.</returns>
        public EcardListModel GetById(int tenantId, int id)
        {
            // Retrieves the Ecard entity by ID from the repository
            var ecards = _uow.EcardRepository.Query().First(f => f.EcardId == id && f.TenantId == tenantId);
            // Maps the Ecard entity to EcardListModel object and returns it
            return _mapper.Map<EcardListModel>(ecards);
        }

        /// <summary>
        /// Updates the details of an existing Ecard in the database.
        /// </summary>
        /// <param name="model">The EcardUpdateModel object containing the updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(EcardUpdateModel model)
        {
            var existingCard = _uow.EcardRepository.Query().Any(p => p.EcardName == model.EcardName && p.TenantId == model.TenantId && p.EcardId != model.EcardId);
            if (existingCard)
            {
                throw new Exception("Ecard with this name already exists");
            }
            // Retrieves the existing Ecard entity from the repository
            var ecards = _uow.EcardRepository.Query().First(r => r.EcardId == model.EcardId && r.TenantId == model.TenantId);
            // Maps the updated model to the existing entity
            var ecardEntites = _mapper.Map<EcardUpdateModel, Ecard>(model, ecards);

            // Sets the update timestamp to current UTC time
            ecardEntites.UpdatedByDateTime = DateTime.UtcNow;
            // Updates the Ecard entity in the repository
            _uow.EcardRepository.Update(ecardEntites);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Removes multiple Ecard entries from the database.
        /// </summary>
        /// <param name="tenantId">The entity ID associated with the Ecards.</param>
        /// <param name="ids">A list of Ecard IDs to be removed.</param>
        /// <returns>A string message indicating the result of the operation.</returns>
        public async Task<string> RemoveMultiple(int tenantId, List<int> ids)
        {
            // Initializes the result message
            var resultMessage = "";
            // Initializes a collection for rules that cannot be deleted
            var notDeletedRules = new HashSet<string>();
            // Initializes a collection for successfully deleted rules
            var deletedRules = new List<int>();
            // Retrieves all PCards for the specified entity
            var pCards = _uow.PcardRepository.Query().Where(f => f.TenantId == tenantId);
            try
            {
                // Processes each Ecard ID for deletion
                foreach (var id in ids)
                {
                    // Retrieves the Ecard entity by ID
                    var item = _uow.EcardRepository.GetById(id);
                    // Checks if the Ecard is being used in any PCard
                    var isInUse = pCards.Any(card => card.Expression.Contains(id.ToString()));
                    if (isInUse)
                    {
                        if (item != null)
                        {
                            // Adds the Ecard name to the not deleted collection
                            notDeletedRules.Add(item.EcardName);
                        }
                    }
                    else
                    {
                        if (item != null)
                        {
                            // Removes the Ecard entity from the repository
                            _uow.EcardRepository.Remove(item);
                            // Adds the ID to the deleted collection
                            deletedRules.Add(id);
                        }
                    }
                }
                // Commits the changes to the database
                await _uow.CompleteAsync();

                // Builds the result message based on the operation outcome
                if (notDeletedRules.Count != 0)
                {
                    var notDeletedMessage = $"The following ECards could not be deleted because they are being used in one or more Stream Cards: {string.Join(", ", notDeletedRules)}.";
                    resultMessage += notDeletedMessage;
                }

                if (deletedRules.Count != 0)
                {
                    var deletedMessage = $"{deletedRules.Count} " + " ECards " + GlobalcConstants.Deleted;
                    resultMessage += " " + deletedMessage;
                }

                if (deletedRules.Count == 0 && notDeletedRules.Count == 0)
                {
                    resultMessage = "No ECards were deleted.";
                }
            }
            catch (Exception ex)
            {
                // Sets the error message if an exception occurs
                resultMessage = ex.Message;
            }
            return resultMessage;
        }

        /// <summary>
        /// Imports a batch of Ecard entries from an Excel file.
        /// </summary>
        /// <param name="tenantId">The entity ID associated with the Ecards.</param>
        /// <param name="fileStream">A stream representing the uploaded Excel file.</param>
        /// <param name="createdBy">The name of the user who created the records.</param>
        /// <returns>A string message indicating the result of the import operation.</returns>
        /// 
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

            finalExpr = "( " + finalExpr.Trim() + " )";

            return finalExpr;
        }
        public async Task<string> ImportECard(int tenantId, Stream fileStream, string createdBy)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];

            int rowCount = GetRowCount(worksheet);
            var models = new List<EcardListModel>();

            int skippedRecordsCount = 0;
            int insertedRecordsCount = 0;
            int dublicatedRecordsCount = 0;

            string resultMessage = "";

            try
            {
                if (rowCount <= 0)
                    return "Uploaded File Is Empty";


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
                foreach (var model in models)
                {
                    // Convert ExpressionShown → Expression (Rule IDs)
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
                        .AnyAsync(p => p.TenantId == tenantId &&
                                       p.EcardName == model.EcardName &&
                                       p.EcardDesc == model.EcardDesc);

                    if (exists)
                    {
                        dublicatedRecordsCount++;
                        continue;
                    }

                    // Prepare entity
                    model.CreatedByDateTime = DateTime.UtcNow;
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    model.UpdatedBy = createdBy;
                    model.IsImport = true;

                    // Save
                    _uow.EcardRepository.Add(_mapper.Map<Ecard>(model));
                    insertedRecordsCount++;
                }

                await _uow.CompleteAsync();

                // Final message
                resultMessage =
                    $"{insertedRecordsCount} inserted. {skippedRecordsCount} skipped. {dublicatedRecordsCount} duplicates.";
            }
            catch (Exception ex)
            {
                resultMessage = "E Card Page = " + ex.Message;
            }

            return resultMessage;
        }

        /// <summary>
        /// Gets the row count of non-empty rows in the worksheet.
        /// </summary>
        /// <param name="worksheet">The Excel worksheet.</param>
        /// <returns>The number of non-empty rows.</returns>
        static int GetRowCount(ExcelWorksheet worksheet)
        {
            // Gets the last row in the worksheet
            int lastRow = worksheet.Dimension.End.Row;
            //int rowCount = 0;
            int lastNonEmptyRow = 0;

            // Iterates through rows to find the last non-empty row
            for (int row = 2; row <= lastRow; row++)
            {
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text);

                if (hasData)
                {
                    lastNonEmptyRow = row;
                }
            }

            return lastNonEmptyRow - 1;
        }

        /// <summary>
        /// Downloads a template for importing Ecard data.
        /// </summary>
        /// <param name="tenantId">The entity ID associated with the template.</param>
        /// <returns>A byte array representing the Excel template file.</returns>
        public async Task<byte[]> DownloadTemplate(int tenantId)
        {
            // Fetches all Erule data for the specified entity
            List<EruleMasterListModel> erule = await _eruleMasterService.GetAll(tenantId);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Cards");

            // Creates VBA project and worksheet change event
            package.Workbook.CreateVBAProject();
            StringBuilder changeEvent = new();
            changeEvent.Append("Private Sub Worksheet_Change(ByVal Target As Range)\n")
                .Append("     If Target.Column >= 3 And Target.Column <= 6 And Target.Value <> \"\" Then\n")
                .Append("          Cards.Cells(Target.Row, 7).Value = Cards.Cells(Target.Row, 7).Value + \" \" + Target.Value\n")
                .Append("          Target.Value = \"\"\n")
                .Append("     End If\n")
                .Append("End Sub");

            sheet.CodeModule.Code = changeEvent.ToString();

            // Defines headers for the template
            string[] headers = ["CardName*", "CardDescription*", "ExpressionShown"];
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }

            // Adds auxiliary columns for template functionality
            //sheet.Cells[1, 7].Value = "Expression*";
            sheet.Cells[1, 5].Value = "Field Description";
            sheet.Cells[1, 11].Value = "OpenParenthesisValue";
            sheet.Cells[1, 12].Value = "EruleName";
            sheet.Cells[1, 13].Value = "EruleId";

            sheet.Cells[1, 15].Value = "LogicalOperatorValue";
            sheet.Cells[1, 17].Value = "CloseParenthesisValue";

            // Adds description for required fields
            sheet.Cells[2, 5].Value = "* Fields marked with an asterisk are required.";
            sheet.Cells[2, 5].Style.Font.Bold = true;
            sheet.Cells[2, 5].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Populates dropdown options for various fields
            string[] openParenthesisOptions = ["("];
            for (int i = 0; i < openParenthesisOptions.Length; i++)
            {
                sheet.Cells[i + 2, 11].Value = openParenthesisOptions[i];
            }

            string[] logicalOperatorOptions = ["And", "Or"];
            for (int i = 0; i < logicalOperatorOptions.Length; i++)
            {
                sheet.Cells[i + 2, 15].Value = logicalOperatorOptions[i];
            }

            string[] CloseParenthesisOptions = [")"];
            for (int i = 0; i < CloseParenthesisOptions.Length; i++)
            {
                sheet.Cells[i + 2, 17].Value = CloseParenthesisOptions[i];
            }

            // Populates Erule ID column for dropdown reference
            //PopulateColumn(sheet, [.. erule.Select(d => d.EruleId.ToString())!], 13);
            PopulateColumn(sheet, [.. erule.Select(d => d.EruleName!.ToString())], 12);

            // Applies dropdown validation to template columns
            //ApplyDropdown(sheet, "OpenParenthesisRange", "C", 11, openParenthesisOptions.Length);
            //ApplyDropdown(sheet, "erulesNameRange", "D", 12, erule.Count);
            //ApplyDropdown(sheet, "logicalOperatorRange", "E", 15, logicalOperatorOptions.Length);
            //ApplyDropdown(sheet, "CloseParenthesisRange", "F", 17, CloseParenthesisOptions.Length);

            // Auto-fits columns for better readability
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// Populates a specified column in the Excel worksheet with values.
        /// </summary>
        /// <param name="sheet">The Excel worksheet.</param>
        /// <param name="values">Array of values to populate.</param>
        /// <param name="columnIndex">Index of the column to populate.</param>
        private static void PopulateColumn(ExcelWorksheet sheet, string[] values, int columnIndex)
        {
            // Ensures at least one blank entry if no values provided
            if (values.Length == 0)
            {
                sheet.Cells[2, columnIndex].Value = "";
                return;
            }

            // Populates the column with provided values
            for (int i = 0; i < values.Length; i++)
            {
                sheet.Cells[i + 2, columnIndex].Value = values[i];
            }
        }

        /// <summary>
        /// Applies a dropdown list to a column in the Excel worksheet.
        /// </summary>
        /// <param name="sheet">The Excel worksheet.</param>
        /// <param name="rangeName">The named range for the dropdown.</param>
        /// <param name="column">The column where the dropdown should be applied.</param>
        /// <param name="dataColumnIndex">The column index containing the dropdown values.</param>
        /// <param name="dataCount">The number of data entries.</param>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int dataCount)
        {
            ExcelRange range;
            // Creates a range for dropdown values
            if (dataCount > 0)
            {
                range = sheet.Cells[2, dataColumnIndex, dataCount + 1, dataColumnIndex];
            }
            else
            {
                sheet.Cells[2, dataColumnIndex].Value = "";
                range = sheet.Cells[2, dataColumnIndex, 2, dataColumnIndex];
            }
            sheet.Workbook.Names.Add(rangeName, range);

            // Applies dropdown validation to each cell in the column
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

        /// <summary>
        /// Exports selected Ecard data to an Excel file.
        /// </summary>
        /// <param name="tenantId">The entity ID associated with the Ecards.</param>
        /// <param name="selectedEcardIds">A list of selected Ecard IDs to be exported.</param>
        /// <returns>A stream containing the Excel file with the Ecard data.</returns>
        public async Task<Stream> ExportECard(int tenantId, List<int> selectedEcardIds)
        {
            // Queries Ecard entities for the specified entity ID
            var Ecards = from ecard in _uow.EcardRepository.Query()
                         where ecard.TenantId == tenantId
                         select new EcardModelDescription
                         {
                             EcardId = ecard.EcardId,
                             EcardName = ecard.EcardName,
                             EcardDesc = ecard.EcardDesc,
                             Expshown = ecard.Expshown,
                             Expression = ecard.Expression
                         };

            // Filters by selected Ecard IDs if provided
            if (selectedEcardIds != null && selectedEcardIds.Count > 0)
            {
                Ecards = Ecards.Where(query => selectedEcardIds.Contains(query.EcardId));
            }

            var Ecard = await Ecards.ToListAsync();

            // Maps Ecard entities to model objects
            var models = _mapper.Map<List<EcardModelDescription>>(Ecard);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Ecards");

            // Adds headers to the worksheet
            var properties = typeof(EcardModelDescription).GetProperties();
            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
            }

            // Populates the worksheet with Ecard data
            for (int row = 0; row < models.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(models[row]);
                }
            }

            // Auto-fits columns for better readability
            worksheet.Cells.AutoFitColumns();

            // Creates a memory stream with the Excel file
            var memoryStream = new MemoryStream();
            package.SaveAs(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        [GeneratedRegex(@"\d+")]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"\d+")]
        private static partial Regex MyRegex1();
        [GeneratedRegex(@"\s+(AND|OR)\s+", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex2();
        [GeneratedRegex(@"\s+(AND|OR)\s+", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex3();
    }
}
