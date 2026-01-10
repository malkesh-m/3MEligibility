using System;
using System.ComponentModel;
using System.Data;
using System.Text;
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
    /// Service class for managing eligibility rules operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EruleService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="parameterService">The parameter service instance.</param>
    /// <param name="conditionService">The condition service instance.</param>
    /// <param name="factorService">The factor service instance.</param>
    public class EruleService(IUnitOfWork uow, IMapper mapper, IParameterService parameterService, IConditionService conditionService, IFactorService factorService) : IEruleService
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
        /// The factor service instance.
        /// </summary>
        private readonly IFactorService _factorService = factorService;

        /// <summary>
        /// Adds a new rule to the system. If the rule already exists with the same `EruleName`, `EruleDesc`, `Expression`, 
        /// `ExpShown`, and `EntityId`, the method will skip adding and no action will be taken.
        /// </summary>
        /// <param name="model">The model containing the rule details to be added.</param>
        /// <returns>Returns a Task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Throws an exception if a rule with the same details already exists in the system.</exception>
        public async Task Add(EruleCreateOrUpdateModel model)
        {
            // Creates a new EruleMaster entity
            var EruleMaster = new EruleMaster
            {
                // Sets the rule name from the model
                EruleName = model.EruleName,
                // Sets the entity ID from the model
                EntityId = model.EntityId,
                // Sets the creation timestamp to current time
                CreatedByDateTime = DateTime.Now,
            };

            // Adds the EruleMaster to the repository
            _uow.EruleMasterRepository.Add(EruleMaster);

            // Commits the changes to the database
            await _uow.CompleteAsync();

            // Maps the model to Erule entity
            var entities = _mapper.Map<Erule>(model);
            // Sets the update timestamp to current UTC time
            entities.UpdatedByDateTime = DateTime.UtcNow;
            // Sets the creation timestamp to current UTC time
            entities.CreatedByDateTime = DateTime.UtcNow;
            // Sets the initial version to 1
            entities.Version = 1;
            // Sets the EruleMaster ID from the newly created master
            entities.EruleMasterId = EruleMaster.Id;

            // Adds the Erule entity to the repository
            _uow.EruleRepository.Add(entities);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Create a new rule with version.
        /// `ExpShown`, and `EntityId`, the method will skip adding and no action will be taken.
        /// </summary>
        /// <param name="model">The model containing the rule details to be added.</param>
        /// <returns>Returns a Task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Throws an exception if a rule with the same details already exists in the system.</exception>
        public async Task Create(EruleCreateModel model)
        {
            // Declares variable for EruleMaster
            EruleMaster eruleMaster = _uow.EruleMasterRepository.Query().FirstOrDefault(f => f.Id == model.EruleMasterId) ?? throw new Exception("Rule does not exist.");

            // Maps the model to Erule entity
            var erule = _mapper.Map<Erule>(model);

            // Retrieves the existing EruleMaster by ID

            ;
            // Throws exception if EruleMaster not found

            // Sets the creation timestamp to current time
            erule.CreatedByDateTime = DateTime.Now;
            // Sets the update timestamp to current time
            erule.UpdatedByDateTime = erule.UpdatedByDateTime = DateTime.Now;

            // Gets the maximum version number for the EruleMaster
            var maxVersion = _uow.EruleRepository.Query()
               .Where(x => x.EruleMasterId == model.EruleMasterId)
               .Select(x => (int?)x.Version)
               .ToList()
               .DefaultIfEmpty(0)
               .Max();

            // Sets the version to max version + 1
            erule.Version = (maxVersion + 1) ?? 1;

            // Sets the EruleMaster reference
            erule.EruleMaster = eruleMaster;

            // Adds the Erule entity to the repository
            _uow.EruleRepository.Add(erule);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing rule.
        /// If the rule is not a draft, a new version number is assigned. If it's a draft, version is set to 0.
        /// </summary>
        /// <param name="model">The model containing rule update data.</param>
        public async Task Update(EruleUpdateModel model)
        {
            // Retrieves the rule by ID and entity ID, including EruleMaster
            var rule = _uow.EruleRepository.Query().Include(e => e.EruleMaster)
                .FirstOrDefault(f => f.EruleId == model.EruleId && f.EntityId == model.EntityId) ?? throw new Exception("Rule does not exist.");

            // Throws exception if rule is already published
            if (rule.IsPublished)
                throw new Exception("Cannot update published rule.");

            // Sets the version from existing rule
            model.Version = rule.Version;
            // Sets the EruleMaster ID from existing rule
            model.EruleMasterId = rule.EruleMasterId;

            // Maps the model properties to the existing rule
            _mapper.Map(model, rule);

            // Sets the update timestamp to current UTC time
            rule.UpdatedByDateTime = DateTime.UtcNow;

            // Updates the rule in the repository
            _uow.EruleRepository.Update(rule);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }


        /// <summary>
        /// Deletes a rule by its ID. Before deletion, it checks if the rule is being used in any eCards. If so, 
        /// the deletion is prevented and an appropriate message is returned.
        /// </summary>
        /// <param name="id">The ID of the rule to be deleted.</param>
        /// <returns>A string indicating whether the rule was deleted or why it could not be deleted.</returns>
        /// <exception cref="Exception">Throws an exception if an error occurs during the deletion process.</exception>
        public async Task<string> Delete(int entityId, int id)
        {
            // Initializes the result message
            var resultMessage = "";
            // Retrieves all eCards for the specified entity
            var eCards = _uow.EcardRepository.Query().ToList();

            try
            {
                // Checks if the rule is being used in any eCard
                foreach (var card in eCards)
                {
                    var exits = card.Expression.Contains(id.ToString());
                    if (exits)
                    {
                        return resultMessage += $" The rule cannot be deleted because it is currently being used in one or more ECards.";
                    }
                }

                // Retrieves the rule entity by ID
                var Item = _uow.EruleRepository.GetById(id);
                // Removes the rule from the repository
                _uow.EruleRepository.Remove(Item);

                // Commits the changes to the database
                await _uow.CompleteAsync();

                // Sets the success message
                resultMessage = GlobalcConstants.Deleted;
            }
            catch (Exception ex)
            {
                // Sets the error message if exception occurs
                resultMessage = ex.Message;
            }

            return resultMessage;
        }

        /// <summary>
        /// Retrieves all rules from the system and maps them to a list of `EruleModel` objects.
        /// </summary>
        /// <returns>A list of `EruleModel` objects representing all the rules in the system.</returns>
        public List<EruleListModel> GetAll(int entityId)
        {
            // Performs a join between EruleMaster and Erule repositories
            var rules = (from master in _uow.EruleMasterRepository.Query().Where(f => f.EntityId == entityId)
                         join rule in _uow.EruleRepository.Query().Where(f => f.EntityId == entityId)
                             on master.Id equals rule.EruleMasterId into gj
                         from rule in gj.DefaultIfEmpty()
                         select new { master, rule })
            .ToList() // Materializes the query
            .Select(x => new EruleListModel
            {
                // Maps EruleMaster properties
                EruleMasterId = x.master.Id,
                EruleName = x.master.EruleName,
                Description = x.master.EruleDesc,
                IsActive = x.master.IsActive,

                // Maps Erule properties with null checks
                EruleId = x.rule?.EruleId ?? 0,
                Expression = x.rule?.Expression ?? "",
                Version = x.rule?.Version,
                IsPublished = x.rule?.IsPublished ?? false,
                ValidFrom = x.rule?.ValidFrom ?? DateTime.MinValue,
                ValidTo = x.rule?.ValidTo,
                ExpShown = x.rule?.ExpShown,
                CreatedBy = x.rule?.CreatedBy,
                CreatedByDateTime = x.rule?.CreatedByDateTime ?? DateTime.MinValue,
                UpdatedBy = x.rule?.UpdatedBy,
                UpdatedByDateTime = x.rule?.UpdatedByDateTime ?? DateTime.MinValue
            })
            .ToList();

            return rules;
        }

        /// <summary>
        /// Retrieves a rule by its ID and maps it to an `EruleModel` object.
        /// </summary>
        /// <param name="id">The ID of the rule to be retrieved.</param>
        /// <returns>An `EruleModel` object representing the rule with the given ID.</returns>
        public EruleListModel GetById(int entityId, int id)
        {
            // Retrieves the rule by ID and entity ID
            var erule = _uow.EruleRepository.Query().FirstOrDefault(f => f.EruleId == id && f.EntityId == entityId);

            // Maps the rule to list model
            var model = _mapper.Map<EruleListModel>(erule);

            // Sets description from EruleMaster
            model.Description = erule?.EruleMaster?.EruleDesc ?? null;

            // Sets rule name from EruleMaster
            model.EruleName = erule?.EruleMaster?.EruleName ?? null;

            return model;
        }

        /// <summary>
        /// Updates an existing rule by creating a new version or draft.
        /// If the rule is not a draft, a new version number is assigned. If it's a draft, version is set to 0.
        /// </summary>
        /// <param name="model">The model containing rule update data.</param>
        public async Task UpdateErule(EruleCreateOrUpdateModel model)
        {
            // Retrieves the existing rule by ID and entity ID
            var existingRule = _uow.EruleRepository.Query()
                .FirstOrDefault(f => f.EruleId == model.EruleId && f.EntityId == model.EntityId) ?? throw new Exception("Original rule not found.");

            // Maps the model to a new Erule entity
            var newRule = _mapper.Map<Erule>(model);

            // Resets the EruleId for new entity
            newRule.EruleId = 0;

            // Sets creation timestamp to current UTC time
            newRule.CreatedByDateTime = DateTime.UtcNow;

            // Sets update timestamp to current UTC time
            newRule.UpdatedByDateTime = DateTime.UtcNow;

            // Sets EruleMaster ID from existing rule
            newRule.EruleMasterId = existingRule.EruleMasterId;

            // Sets version based on publication status
            if (model.IsPublished)
            {
                // Gets maximum version for the EruleMaster
                var maxVersion = _uow.EruleRepository.Query()
                    .Where(x => x.EruleId == existingRule.EruleId || x.EruleMasterId == existingRule.EruleMasterId)
                    .Max(x => x.Version);

                // Sets version to max version + 1
                newRule.Version = maxVersion + 1;
            }
            else
            {
                // Sets version to 0 for drafts
                newRule.Version = 0;
            }

            // Adds the new rule to the repository
            _uow.EruleRepository.Add(newRule);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all rules by EruleMaster ID and entity ID.
        /// </summary>
        /// <param name="eruleMasterId">The EruleMaster ID.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <returns>A list of EruleListModel objects.</returns>
        public async Task<List<EruleListModel>> GetAllByEruleMasterId(int eruleMasterId, int entityId)
        {
            // Retrieves and maps rules by EruleMaster ID and entity ID
            return _mapper.Map<List<EruleListModel>>(await _uow.EruleRepository.Query()
                .Where(x => x.EntityId == entityId && x.EruleMasterId == eruleMasterId).ToListAsync());
        }

        /// <summary>
        /// Publishes a draft rule by assigning the next version number and marking it as not a draft.
        /// </summary>
        /// <param name="draftEruleId">ID of the draft rule to publish.</param>
        /// <param name="entityId">Entity ID associated with the rule.</param>
        public async Task PublishDraftAsync(int draftEruleId, int entityId)
        {
            // Retrieves the draft rule by ID and publication status
            var draftRule = _uow.EruleRepository.Query()
                .FirstOrDefault(x => x.EruleId == draftEruleId && x.IsPublished == true && x.EntityId == entityId) ?? throw new Exception("Draft rule not found.");

            // Gets the maximum version for the entity
            var maxVersion = _uow.EruleRepository.Query()
               .Where(x => x.EntityId == entityId)
               .Max(x => x.Version);

            // Marks the rule as published
            draftRule.IsPublished = false;

            // Sets the version to max version + 1
            draftRule.Version = maxVersion + 1;

            // Sets update timestamp to current UTC time
            draftRule.UpdatedByDateTime = DateTime.UtcNow;

            // Updates the rule in the repository
            _uow.EruleRepository.Update(draftRule);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates the active status of a specific rule based on EruleId and EntityId.
        /// </summary>
        /// <param name="eruleId">The ID of the rule to update.</param>
        /// <param name="entityId">The Entity ID associated with the rule.</param>
        /// <param name="isActive">New status to set for IsActive.</param>
        public async Task UpdateStatusAsync(int eruleId, int entityId, bool isActive)
        {
            // Retrieves the rule by ID and entity ID
            var rule = _uow.EruleRepository.Query()
                .FirstOrDefault(x => x.EruleId == eruleId && x.EntityId == entityId) ?? throw new Exception("Rule not found.");

            // Sets update timestamp to current UTC time
            rule.UpdatedByDateTime = DateTime.UtcNow;

            // Updates the rule in the repository
            _uow.EruleRepository.Update(rule);

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple rules based on a list of IDs. Before deletion, it checks if each rule is being used in any eCards.
        /// If a rule is in use, it will not be deleted, and a message will be generated to indicate the failed deletion.
        /// </summary>
        /// <param name="ids">A list of rule IDs to be deleted.</param>
        /// <returns>A string indicating whether the rules were deleted or why they could not be deleted.</returns>
        /// <exception cref="Exception">Throws an exception if an error occurs during the deletion process.</exception>
        public async Task<string> RemoveMultiple(int entityId, List<int> ids)
        {
            // Initializes the result message
            var resultMessage = "";

            // Retrieves all eCards for the entity
            var eCards = _uow.EcardRepository.Query().Where(f => f.EntityId == entityId);

            // Initializes collections for tracking deleted and not deleted rules
            var notDeletedRules = new HashSet<string>();  // Use HashSet to avoid duplicates
            var deletedRules = new List<int>();

            try
            {
                // Processes each rule ID in the list
                foreach (var id in ids)
                {
                    // Retrieves the rule by ID and entity ID
                    var item = _uow.EruleRepository.Query().First(f => f.EruleId == id && f.EntityId == entityId);

                    // Checks if the rule is being used in any eCard
                    var isInUse = eCards.Any(card => card.Expression.Contains(id.ToString()));

                    if (isInUse)
                    {
                        if (item != null)
                        {
                            // Adds to not deleted rules collection
                            //notDeletedRules.Add(item.EruleName);
                        }
                    }
                    else
                    {
                        if (item != null)
                        {
                            // Removes the rule from repository
                            _uow.EruleRepository.Remove(item);
                            // Adds to deleted rules collection
                            deletedRules.Add(id);
                        }
                    }
                }

                // Commits the changes to the database
                await _uow.CompleteAsync();

                // Constructs message for not deleted rules
                if (notDeletedRules.Count != 0)
                {
                    var notDeletedMessage = $"The following rules could not be deleted because they are being used in one or more ECards: {string.Join(", ", notDeletedRules)}.";
                    resultMessage += notDeletedMessage;
                }

                // Constructs message for deleted rules
                if (deletedRules.Count != 0)
                {
                    var deletedMessage = $"{deletedRules.Count} " + " Rules " + GlobalcConstants.Deleted;
                    resultMessage += " " + deletedMessage;
                }

                // Handles case where no rules were processed
                if (deletedRules.Count == 0 && notDeletedRules.Count == 0)
                {
                    resultMessage = "No rules were deleted.";
                }
            }
            catch (Exception ex)
            {
                // Sets error message if exception occurs
                resultMessage = ex.Message;
            }

            return resultMessage;
        }


        /// <summary>
        /// Imports data from an Excel file, validates the contents, and inserts the valid records into the database.
        /// It skips records with missing required fields, handles duplicate records, and counts the number of inserted, skipped, and duplicate records.
        /// </summary>
        /// <param name="entityId">The entity identifier for which rules are being imported.</param>
        /// <param name="fileStream">The stream of the uploaded Excel file.</param>
        /// <param name="createdBy">The username or identifier of the person who created the records.</param>
        /// <returns>A string message indicating the result of the import operation, including counts of inserted, skipped, and duplicated records.</returns>
        /// <exception cref="Exception">Throws an exception if an error occurs during the import process.</exception>
        public async Task<string> ImportErule(int entityId, Stream fileStream, string createdBy)
        {
            // Sets the ExcelPackage license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new ExcelPackage instance from the file stream
            using var package = new ExcelPackage(fileStream);
            // Gets the first worksheet from the workbook
            var worksheet = package.Workbook.Worksheets[0];
            // Gets the row count from the worksheet
            int rowCount = GetRowCount(worksheet);
            // Initializes a new list for EruleListModel objects
            var models = new List<EruleListModel>();
            // Initializes counter for skipped records
            int skippedRecordsCount = 0;
            // Initializes counter for inserted records
            int insertedRecordsCount = 0;
            // Initializes counter for duplicated records
            int dublicatedRecordsCount = 0;
            // Initializes the result message string
            var resultMessage = "";

            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    // Returns message indicating the file is empty
                    return resultMessage = "Uploaded File Is Empty";
                }
                // Loops through each row in the Excel file starting from row 2
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Gets the value from column 1 (RuleName)
                    var EruleName = worksheet.Cells[row, 1].Text;
                    // Gets the value from column 2 (RuleDescription)
                    var EruleDesc = worksheet.Cells[row, 2].Text;
                    // Gets the value from column 9 (Expression)
                    var Expression = worksheet.Cells[row, 9].Text;

                    // Checks if any required field is empty
                    if (string.IsNullOrEmpty(EruleName) || string.IsNullOrEmpty(EruleDesc) || string.IsNullOrEmpty(Expression))
                    {
                        // Increments the skipped records counter
                        skippedRecordsCount++;
                        // Skips to the next record
                        continue;
                    }

                    // Creates a new EruleListModel instance
                    var model = new EruleListModel
                    {
                        // Sets the Expression property
                        Expression = Expression,
                        // Sets the CreatedBy property
                        CreatedBy = createdBy
                    };
                    // Adds the model to the list
                    models.Add(model);
                }

                // Processes each model in the list
                foreach (var model in models)
                {
                    // Checks if an entity with the same ExpShown already exists
                    var existingEntity = await _uow.EruleRepository.Query().AnyAsync(x => x.EntityId == entityId && x.ExpShown == model.ExpShown);

                    // If entity already exists
                    if (existingEntity)
                    {
                        // Increments the duplicated records counter
                        dublicatedRecordsCount++;
                        // Skips to the next model
                        continue;
                    }

                    // Sets the EntityId property
                    model.EntityId = entityId;
                    // Sets the UpdatedByDateTime property to current UTC time
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    // Sets the CreatedByDateTime property to current UTC time
                    model.CreatedByDateTime = DateTime.UtcNow;
                    // Sets the UpdatedByDateTime property to current UTC time again
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    // Maps the model to Erule entity and adds it to the repository
                    _uow.EruleRepository.Add(_mapper.Map<Erule>(model));
                    // Increments the inserted records counter
                    insertedRecordsCount++;
                }

                // Commits all changes to the database
                await _uow.CompleteAsync();

                // Checks if any records were inserted
                if (insertedRecordsCount > 0)
                {
                    // Adds success message with count of inserted records
                    resultMessage = $"{models.Count} Factor Inserted Successfully.";
                }
                // Checks if any records were skipped
                if (skippedRecordsCount > 0)
                {
                    // Adds message about skipped records
                    resultMessage += $" {skippedRecordsCount} records were not inserted because of missing required field.";
                }
                // Checks if any records were duplicates
                if (dublicatedRecordsCount > 0)
                {
                    // Adds message about duplicate records
                    resultMessage += $" {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                // Sets error message with exception details
                resultMessage = "Error On ERule Page = " + ex.Message;
            }
            // Returns the result message
            return resultMessage;
        }

        /// <summary>
        /// Retrieves the row count from the provided Excel worksheet. It determines the last row with data by checking specific columns.
        /// </summary>
        /// <param name="worksheet">The Excel worksheet from which to get the row count.</param>
        /// <returns>The count of rows with data in the worksheet.</returns>
        static int GetRowCount(ExcelWorksheet worksheet)
        {
            // Gets the last row with data from the worksheet dimension
            int lastRow = worksheet.Dimension.End.Row;
            // Initializes row count to zero
            //int rowCount = 0;
            // Initializes last non-empty row to zero
            int lastNonEmptyRow = 0;

            // Loops through each row starting from row 2
            for (int row = 2; row <= lastRow; row++)
            {
                // Checks if any of the first three columns have data
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text);

                // If the row has data
                if (hasData)
                {
                    // Updates the last non-empty row
                    lastNonEmptyRow = row;
                }
            }

            // Returns the last non-empty row
            return lastNonEmptyRow - 1;
        }

        /// <summary>
        /// Downloads an Excel template that contains predefined headers, drop-down lists, and formulas for user input.
        /// It also populates the template with necessary values from external services such as parameters, conditions, and factors.
        /// </summary>
        /// <param name="entityId">The entity identifier for which the template is being generated.</param>
        /// <returns>A byte array representing the Excel template file.</returns>
        /// <exception cref="Exception">Throws an exception if an error occurs during template generation.</exception>
        public async Task<byte[]> DownloadTemplate(int entityId)
        {
            // Fetches all parameters for the specified entity
            List<ParameterListModel> parameter = _parameterService.GetAll(entityId);
            // Fetches all conditions
            List<ConditionModel> conditions = _conditionService.GetAll();
            // Fetches all factors for the specified entity
            List<FactorListModel> factors = _factorService.GetAll(entityId);

            // Sets the ExcelPackage license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new ExcelPackage instance
            using var package = new ExcelPackage();
            // Adds a new worksheet named "Rules"
            var sheet = package.Workbook.Worksheets.Add("Rules");

            // Creates a VBA project in the workbook
            package.Workbook.CreateVBAProject();
            // Builds the VBA change event code
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

            // Sets the VBA code for the worksheet
            sheet.CodeModule.Code = changeEvent.ToString();

            // Defines the header names
            string[] headers = ["RuleName*", "RuleDescription*", "OpenParenthesis", "ParameterName", "Operator", "Factor", "LogicalOperator", "CloseParenthesis"];
            // Loops through each header and sets it in the first row
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }

            // Sets auxiliary column headers
            sheet.Cells[1, 18].Value = "OpenParenthesisValue";
            sheet.Cells[1, 19].Value = "ParameterNameValue";
            sheet.Cells[1, 20].Value = "ParameterIdValue";
            sheet.Cells[1, 21].Value = "ConditionValue";
            sheet.Cells[1, 22].Value = "Value1";
            sheet.Cells[1, 23].Value = "Value2";
            sheet.Cells[1, 24].Value = "FactorParameterId";
            sheet.Cells[1, 25].Value = "LogicalOperatorValue";
            sheet.Cells[1, 26].Value = "CloseParenthesisValue";
            sheet.Cells[1, 9].Value = "Expression*";
            sheet.Cells[1, 10].Value = "Field Description";
            sheet.Cells[1, 17].Value = "ParameterId";

            // Adds description for required fields
            sheet.Cells[2, 10].Value = "* Fields marked with an asterisk are required.";
            // Makes the text bold
            sheet.Cells[2, 10].Style.Font.Bold = true;
            // Sets the text color to red
            sheet.Cells[2, 10].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Defines open parenthesis options
            string[] openParenthesisOptions = ["("];
            // Populates open parenthesis options in column 18
            for (int i = 0; i < openParenthesisOptions.Length; i++)
            {
                sheet.Cells[i + 2, 18].Value = openParenthesisOptions[i];
            }

            // Defines logical operator options
            string[] logicalOperatorOptions = ["And", "Or"];
            // Populates logical operator options in column 25
            for (int i = 0; i < logicalOperatorOptions.Length; i++)
            {
                sheet.Cells[i + 2, 25].Value = logicalOperatorOptions[i];
            }

            // Defines close parenthesis options
            string[] CloseParenthesisOptions = [")"];
            // Populates close parenthesis options in column 26
            for (int i = 0; i < CloseParenthesisOptions.Length; i++)
            {
                sheet.Cells[i + 2, 26].Value = CloseParenthesisOptions[i];
            }

            // Populates auxiliary columns with data
            PopulateColumn(sheet, [.. parameter.Select(d => d.ParameterName ?? "")], 19);
            PopulateColumn(sheet, [.. parameter.Select(e => e.ParameterId.ToString())], 20);
            PopulateColumn(sheet, [.. conditions.Select(e => e.ConditionValue != null ? e.ConditionValue.ToString() : "")], 21);
            PopulateColumn(sheet, [.. factors.Select(e => e.Value1 != null ? e.Value1.ToString() : "")], 22);
            PopulateColumn(sheet, [.. factors.Select(e => e.Value2 != null ? e.Value2.ToString() : "")], 23);
            PopulateColumn(sheet, [.. factors.Select(e => e.ParameterId.ToString() ?? "")], 24);

            // Applies dropdown validations to various columns
            ApplyDropdown(sheet, "OpenParenthesisRange", "C", 18, openParenthesisOptions.Length);
            ApplyDropdown(sheet, "ParameterNameRange", "D", 19, parameter.Count);
            ApplyDropdown(sheet, "ConditionRange", "E", 21, conditions.Count);
            ApplyDropdown(sheet, "logicalOperatorRange", "G", 25, logicalOperatorOptions.Length);
            ApplyDropdown(sheet, "CloseParenthesisRange", "H", 26, CloseParenthesisOptions.Length);
            // Adds formula to auto-populate ParameterId
            AddFormula(sheet, "Q", "D", 19, 20, parameter.Count);
            // Applies factor value dropdown validation
            ApplyFactorValueDropdown(sheet, factors);

            // Auto-fits all columns in the worksheet
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            // Returns the Excel package as a byte array
            return await Task.FromResult(package.GetAsByteArray());
        }
        public async Task<byte[]> DownloadTemplateEruleMaster(int entityId)
        {
            // Fetches all parameters for the specified entity
            _ = _parameterService.GetAll(entityId);
            // Fetches all conditions
            _ = _conditionService.GetAll();
            // Fetches all factors for the specified entity
            _ = _factorService.GetAll(entityId);

            // Sets the ExcelPackage license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new ExcelPackage instance
            using var package = new ExcelPackage();
            // Adds a new worksheet named "Rules"
            var sheet = package.Workbook.Worksheets.Add("Rules");

            // Creates a VBA project in the workbook
            package.Workbook.CreateVBAProject();
            // Builds the VBA change event code
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

            // Sets the VBA code for the worksheet
            sheet.CodeModule.Code = changeEvent.ToString();

            // Defines the header names
            string[] headers = ["RuleName*", "RuleDescription*", "IsActive*"];
            // Loops through each header and sets it in the first row
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }

            string[] isActiveOptions = ["True", "False"];


            for (int i = 0; i < isActiveOptions.Length; i++)
            {
                sheet.Cells[i + 2, 10].Value = isActiveOptions[i];
            }
            // Sets auxiliary column headers
            //sheet.Cells[1, 18].Value = "OpenParenthesisValue";
            //sheet.Cells[1, 19].Value = "ParameterNameValue";
            //sheet.Cells[1, 20].Value = "ParameterIdValue";
            //sheet.Cells[1, 21].Value = "ConditionValue";
            //sheet.Cells[1, 22].Value = "Value1";
            //sheet.Cells[1, 23].Value = "Value2";
            //sheet.Cells[1, 24].Value = "FactorParameterId";
            //sheet.Cells[1, 25].Value = "LogicalOperatorValue";
            //sheet.Cells[1, 26].Value = "CloseParenthesisValue";
            //sheet.Cells[1, 9].Value = "Expression*";
            sheet.Cells[1, 5].Value = "Field Description";
            //sheet.Cells[1, 17].Value = "ParameterId";

            // Adds description for required fields
            sheet.Cells[2, 5].Value = "* Fields marked with an asterisk are required.";
            // Makes the text bold
            sheet.Cells[2, 5].Style.Font.Bold = true;
            // Sets the text color to red
            sheet.Cells[2, 5].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Defines open parenthesis options
            //string[] openParenthesisOptions = ["("];
            //// Populates open parenthesis options in column 18
            //for (int i = 0; i < openParenthesisOptions.Length; i++)
            //{
            //    sheet.Cells[i + 2, 18].Value = openParenthesisOptions[i];
            //}

            //// Defines logical operator options
            //string[] logicalOperatorOptions = ["And", "Or"];
            //// Populates logical operator options in column 25
            //for (int i = 0; i < logicalOperatorOptions.Length; i++)
            //{
            //    sheet.Cells[i + 2, 25].Value = logicalOperatorOptions[i];
            //}

            //// Defines close parenthesis options
            //string[] CloseParenthesisOptions = [")"];
            //// Populates close parenthesis options in column 26
            //for (int i = 0; i < CloseParenthesisOptions.Length; i++)
            //{
            //    sheet.Cells[i + 2, 26].Value = CloseParenthesisOptions[i];
            //}

            //// Populates auxiliary columns with data
            //PopulateColumn(sheet, [.. parameter.Select(d => d.ParameterName ?? "")], 19);
            //PopulateColumn(sheet, [.. parameter.Select(e => e.ParameterId.ToString())], 20);
            //PopulateColumn(sheet, [.. conditions.Select(e => e.ConditionValue != null ? e.ConditionValue.ToString() : "")], 21);
            //PopulateColumn(sheet, [.. factors.Select(e => e.Value1 != null ? e.Value1.ToString() : "")], 22);
            //PopulateColumn(sheet, [.. factors.Select(e => e.Value2 != null ? e.Value2.ToString() : "")], 23);
            //PopulateColumn(sheet, [.. factors.Select(e => e.ParameterId.ToString() ?? "")], 24);

            //// Applies dropdown validations to various columns
            //ApplyDropdown(sheet, "OpenParenthesisRange", "C", 18, openParenthesisOptions.Length);
            //ApplyDropdown(sheet, "ParameterNameRange", "D", 19, parameter.Count);
            //ApplyDropdown(sheet, "ConditionRange", "E", 21, conditions.Count);
            //ApplyDropdown(sheet, "logicalOperatorRange", "G", 25, logicalOperatorOptions.Length);
            //ApplyDropdown(sheet, "CloseParenthesisRange", "H", 26, CloseParenthesisOptions.Length);
            //// Adds formula to auto-populate ParameterId
            //AddFormula(sheet, "Q", "D", 19, 20, parameter.Count);
            //// Applies factor value dropdown validation
            //ApplyFactorValueDropdown(sheet, factors);
            var isActiveRange = sheet.Cells[2, 10, 3, 10];
            sheet.Workbook.Names.Add("IsActiveRange", isActiveRange);

            // Apply dropdown on column C (IsActive)
            ApplyDropdown(sheet, "IsActiveRange", "C", 10, 100);
            // Auto-fits all columns in the worksheet
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            // Returns the Excel package as a byte array
            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// Adds a formula to a specified Excel worksheet column that performs a lookup operation based on a condition.
        /// </summary>
        /// <param name="sheet">The Excel worksheet where the formula will be added.</param>
        /// <param name="resultColumn">The result column where the formula will be placed.</param>
        /// <param name="lookupColumn">The column to be used for the lookup.</param>
        /// <param name="dataStartColumn">The starting column of the data range for the lookup.</param>
        /// <param name="idColumn">The column containing the ID used for the lookup operation.</param>
        /// <param name="dataCount">The number of rows of data to consider for the lookup.</param>
        private static void AddFormula(ExcelWorksheet sheet, string resultColumn, string lookupColumn, int dataStartColumn, int idColumn, int dataCount)
        {
            // Gets the address of the data range
            string rangeAddress = sheet.Cells[2, dataStartColumn, dataCount + 1, idColumn].Address;

            // Loops through rows 2 to 100
            for (int row = 2; row <= 100; row++)
            {
                // Sets the formula for the result column cell
                sheet.Cells[row, resultColumn[0] - 'A' + 1].Formula = $"IF({lookupColumn}{row}=\"\", \"\", VLOOKUP({lookupColumn}{row}, {rangeAddress}, 2, FALSE))";
            }
        }

        /// <summary>
        /// Applies dropdown validation to an Excel worksheet column based on a predefined list of options.
        /// </summary>
        /// <param name="sheet">The Excel worksheet where the dropdown will be applied.</param>
        /// <param name="factors">A list of factors that are used to generate the dropdown values.</param>
        private static void ApplyFactorValueDropdown(ExcelWorksheet sheet, List<FactorListModel> factors)
        {
            // Gets the workbook from the worksheet
            var workbook = sheet.Workbook;
            // Creates a dictionary to map parameter IDs to factor values
            var paramValueMap = new Dictionary<int, List<string>>();

            // Step 1: Map ParameterId to combined Value1 and Value2 for dropdown
            foreach (var factor in factors)
            {
                // Gets the parameter ID from the factor
                int paramId = (int)factor.ParameterId!;
                // Combines Value1 and Value2 for display
                string valueCombination = factor.Value2 == "" ? factor.Value1! : $"{factor.Value1} + {factor.Value2}";

                // If the parameter ID is not in the dictionary
                if (!paramValueMap.TryGetValue(paramId, out List<string>? value))
                {
                    // Adds the parameter ID with the value combination
                    paramValueMap[paramId] = [valueCombination];
                }
                else
                {
                    value.Add(valueCombination);
                }
            }

            // Step 2: Store Factor values in hidden columns
            // Sets the starting column for hidden data
            int startColumn = 30;
            // Loops through each parameter in the map
            foreach (var param in paramValueMap)
            {
                // Creates a range name for the parameter
                string rangeName = $"Fact_{param.Key}";
                // Sets the starting row index
                int rowIndex = 2;

                // Loops through each value for the parameter
                foreach (var value in param.Value)
                {
                    // Sets the value in the hidden column
                    sheet.Cells[rowIndex, startColumn].Value = value;
                    // Increments the row index
                    rowIndex++;
                }

                // Creates a range for the hidden data
                var range = sheet.Cells[2, startColumn, rowIndex - 1, startColumn];
                // Adds a named range for the data
                workbook.Names.Add(rangeName, range);

                // Increments the start column for the next parameter
                startColumn++;
            }

            // Step 3: Apply dynamic dropdown using INDIRECT()
            // Loops through rows 2 to 100
            for (int row = 2; row <= 100; row++)
            {
                // Adds list validation to the FactorValue column
                var validation = sheet.DataValidations.AddListValidation($"F{row}");

                // Sets the formula for the dropdown using INDIRECT
                validation.Formula.ExcelFormula = $"INDIRECT(\"Fact_\"&Q{row})";
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
        /// Populates an Excel worksheet column with a list of values starting from the second row.
        /// </summary>
        /// <param name="sheet">The Excel worksheet where the column will be populated.</param>
        /// <param name="values">An array of values to populate the column with.</param>
        /// <param name="columnIndex">The index of the column to be populated.</param>
        private static void PopulateColumn(ExcelWorksheet sheet, string[] values, int columnIndex)
        {
            // Checks if the values array is empty
            if (values.Length == 0)
            {
                // Ensures at least one blank entry
                sheet.Cells[2, columnIndex].Value = "";
                // Returns from the method
                return;
            }

            // Loops through each value in the array
            for (int i = 0; i < values.Length; i++)
            {
                // Sets the value in the specified column
                sheet.Cells[i + 2, columnIndex].Value = values[i];
            }
        }

        /// <summary>
        /// Applies dropdown validation to an Excel worksheet column based on a predefined list of options.
        /// </summary>
        /// <param name="sheet">The Excel worksheet where the dropdown will be applied.</param>
        /// <param name="rangeName">The name of the range for the dropdown.</param>
        /// <param name="column">The column to which the dropdown will be applied.</param>
        /// <param name="dataColumnIndex">The index of the data column for the dropdown options.</param>
        /// <param name="dataCount">The number of rows to apply the dropdown to.</param>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int dataCount)
        {
            ExcelRange range;
            // Checks if there is data to create a range
            if (dataCount > 0)
            {
                // Creates a range with the specified data
                range = sheet.Cells[2, dataColumnIndex, dataCount + 1, dataColumnIndex];
            }
            else
            {
                // Ensures a blank cell exists
                sheet.Cells[2, dataColumnIndex].Value = "";
                // Creates a range with just one cell
                range = sheet.Cells[2, dataColumnIndex, 2, dataColumnIndex];
            }
            // Adds a named range to the workbook
            sheet.Workbook.Names.Add(rangeName, range);

            // Loops through rows 2 to 100
            for (int row = 2; row <= 100; row++)
            {
                // Adds list validation to the specified cell
                var validation = sheet.DataValidations.AddListValidation($"{column}{row}");
                // Sets the formula for the dropdown
                validation.Formula.ExcelFormula = rangeName;
                // Enables error message display
                validation.ShowErrorMessage = true;
                // Sets the error style to stop
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                // Sets the error title
                validation.ErrorTitle = "Invalid Selection";
                // Sets the error message
                validation.Error = "Please select a valid option.";
            }
        }
        public async Task<string> ImportEruleMaster(int entityId, Stream fileStream, string createdBy)
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

            int rowCount = worksheet.Dimension?.Rows ?? 0;

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
                    string excelKey = $"{ruleName?.ToLower()}_{entityId}";
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
                        EruleName = ruleName,
                        EruleDesc = ruleDesc,
                        IsActive = isActive,
                        EntityId = entityId,
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
                        .AnyAsync(x => x.EntityId == entityId && x.EruleName == model.EruleName);

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
                List<string> messages = [];

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
        /// Exports a list of selected eRules from the database into an Excel file, with each rule's details in a separate row.
        /// </summary>
        /// <param name="entityId">The entity identifier for which rules are being exported.</param>
        /// <param name="selectedEruleIds">A list of eRule IDs to be exported.</param>
        /// <returns>A stream containing the Excel file with the selected eRules.</returns>
        /// <exception cref="Exception">Throws an exception if an error occurs during the export process.</exception>
        public async Task<Stream> ExportErule(int entityId, List<int> selectedEruleIds)
        {
            // Queries the eRules for the specified entity
            var Erules = from erule in _uow.EruleRepository.Query().Where(f => f.EntityId == entityId)
                         select new EruleModelDescription
                         {
                             EruleId = erule.EruleId,
                             ExpShown = erule.ExpShown,
                             Expression = erule.Expression
                         };

            // Checks if specific eRule IDs were provided
            if (selectedEruleIds != null && selectedEruleIds.Count > 0)
            {
                // Filters the query to only include the selected IDs
                Erules = Erules.Where(query => selectedEruleIds.Contains(query.EruleId));
            }

            // Executes the query and gets the results as a list
            var Erule = await Erules.ToListAsync();
            // Maps the results to EruleModelDescription objects
            var models = _mapper.Map<List<EruleModelDescription>>(Erule);
            // Sets the ExcelPackage license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new ExcelPackage instance
            using var package = new ExcelPackage();
            // Adds a new worksheet named "Erules"
            var worksheet = package.Workbook.Worksheets.Add("Erules");

            // Gets the properties of the EruleModelDescription type
            var properties = typeof(EruleModelDescription).GetProperties();
            // Loops through each property and sets it as a header
            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
            }

            // Loops through each model
            for (int row = 0; row < models.Count; row++)
            {
                // Loops through each property
                for (int col = 0; col < properties.Length; col++)
                {
                    // Sets the property value in the worksheet
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(models[row]);
                }
            }

            // Auto-fits all columns in the worksheet
            worksheet.Cells.AutoFitColumns();

            // Creates a memory stream
            var memoryStream = new MemoryStream();
            // Saves the Excel package to the memory stream
            package.SaveAs(memoryStream);
            // Resets the stream position to the beginning
            memoryStream.Position = 0;

            // Returns the memory stream
            return memoryStream;
        }
    }
}
