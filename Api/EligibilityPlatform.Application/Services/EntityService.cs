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
    /// Service class for managing entity operations including CRUD, import, and export functionality.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="EntityService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance for database operations.</param>
    /// <param name="mapper">The AutoMapper instance for object mapping.</param>
    /// <param name="countryService">The country service instance for country-related operations.</param>
    /// <param name="cityService">The city service instance for city-related operations.</param>
    public partial class EntityService(IUnitOfWork uow, IMapper mapper, ICountryService countryService, ICityService cityService) : IEntityService
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
        /// Adds a new entity to the database after validating maximum entity limit and code uniqueness.
        /// </summary>
        /// <param name="model">The entity model containing the new entity data.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the maximum entity limit is reached or entity code already exists.</exception>
        public async Task Add(CreateOrUpdateEntityModel model)
        {
            /// <summary>
            /// Variable to store the maximum entity limit from application settings.
            /// </summary>
            var maximumEntity = 0;

            /// <summary>
            /// Retrieves the application settings from the database.
            /// </summary>
            var appSetting = await _uow.AppSettingRepository.Query().FirstOrDefaultAsync();

            /// <summary>
            /// If application settings exist, retrieves the maximum entity limit.
            /// </summary>
            if (appSetting != null)
            {
                maximumEntity = appSetting.MaximumEntities;
            }

            /// <summary>
            /// Checks if the current entity count exceeds the maximum allowed limit.
            /// </summary>
            if (maximumEntity != 0 && _uow.EntityRepository.Query().Count() >= maximumEntity)
            {
                /// <summary>
                /// Throws an exception if maximum entity limit is reached.
                /// </summary>
                throw new Exception("You have reached to maximum entity limit. Not allowed to create any more entity.");
            }

            /// <summary>
            /// Checks if an entity with the same code already exists.
            /// </summary>
            var Entity = _uow.EntityRepository.Query().FirstOrDefault(e => e.Code == model.Code);

            /// <summary>
            /// If entity with same code exists, throws an exception.
            /// </summary>
            if (Entity != null)
            {
                /// <summary>
                /// Throws an exception for duplicate entity code.
                /// </summary>
                throw new Exception("Entity Code Already Exist");
            }

            /// <summary>
            /// Maps the model to an Entity object.
            /// </summary>
            var entity = _mapper.Map<Entity>(model);

            /// <summary>
            /// Sets the entity code from the model.
            /// </summary>
            entity.Code = model.Code;

            /// <summary>
            /// Sets the updated date/time to current UTC time.
            /// </summary>
            entity.UpdatedByDateTime = DateTime.UtcNow;

            /// <summary>
            /// Sets the created date/time to current UTC time.
            /// </summary>
            entity.CreatedByDateTime = DateTime.UtcNow;

            /// <summary>
            /// Sets the updated date/time to current UTC time again (redundant).
            /// </summary>
            entity.UpdatedByDateTime = DateTime.UtcNow;

            /// <summary>
            /// Adds the mapped entity to the repository.
            /// </summary>
            _uow.EntityRepository.Add(_mapper.Map<CreateOrUpdateEntityModel, Entity>(model, entity));

            /// <summary>
            /// Commits the changes to the database.
            /// </summary>
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes an entity by ID after validating existence and checking for child dependencies.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the entity is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity has child dependencies.</exception>
        public async Task Delete(int id)
        {
            /// <summary>
            /// Retrieves the entity by its ID.
            /// </summary>
            var Item = _uow.EntityRepository.GetById(id) ?? throw new Exception("Entity not found.");

            /// <summary>
            /// Checks if the entity has any child entities.
            /// </summary>
            var haschild = await _uow.EntityRepository.Query().AnyAsync(x => x.ParentEnitityId == id);

            /// <summary>
            /// If entity has child dependencies, throws an exception.
            /// </summary>
            if (haschild)
            {
                /// <summary>
                /// Throws an exception for entities with dependencies.
                /// </summary>
                throw new InvalidOperationException("Cannot delete the entity because it has dependencies.");
            }

            /// <summary>
            /// Removes the entity from the repository.
            /// </summary>
            _uow.EntityRepository.Remove(Item);

            /// <summary>
            /// Commits the changes to the database.
            /// </summary>
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all entities from the database.
        /// </summary>
        /// <returns>List of all entities mapped to EntityModel.</returns>
        public List<EntityModel> GetAll()
        {
            /// <summary>
            /// Retrieves all entities from the repository.
            /// </summary>
            var entities = _uow.EntityRepository.GetAll();

            /// <summary>
            /// Maps the entities to EntityModel objects and returns the list.
            /// </summary>
            return _mapper.Map<List<EntityModel>>(entities);
        }

        /// <summary>
        /// Retrieves an entity by its unique ID.
        /// </summary>
        /// <param name="id">The unique identifier of the entity.</param>
        /// <returns>The entity mapped to EntityModel.</returns>
        public EntityModel GetById(int id)
        {
            /// <summary>
            /// Retrieves the entity by its ID from the repository.
            /// </summary>
            var entities = _uow.EntityRepository.GetById(id);

            /// <summary>
            /// Maps the entity to EntityModel and returns it.
            /// </summary>
            return _mapper.Map<EntityModel>(entities);
        }

        /// <summary>
        /// Updates an existing entity with new values.
        /// </summary>
        /// <param name="model">The updated entity model.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the entity code already exists for another entity.</exception>
        public async Task Update(CreateOrUpdateEntityModel model)
        {
            /// <summary>
            /// Checks if another entity with the same code exists (excluding current entity).
            /// </summary>
            var lastEntity = _uow.EntityRepository.Query().FirstOrDefault(e => e.Code == model.Code && e.EntityId != model.EntityId);

            /// <summary>
            /// If duplicate code found, throws an exception.
            /// </summary>
            if (lastEntity != null)
            {
                /// <summary>
                /// Throws an exception for duplicate entity code.
                /// </summary>
                throw new Exception("Entity Code Already Exist");
            }

            /// <summary>
            /// Retrieves the existing entity by its ID.
            /// </summary>
            var entity = _uow.EntityRepository.GetById(model.EntityId);

            /// <summary>
            /// Updates the updated date/time to current UTC time.
            /// </summary>
            entity.UpdatedByDateTime = DateTime.UtcNow;


            var createdBy = entity.CreatedBy;
            var createdDate = entity.CreatedByDateTime;
            /// <summary>
            /// Updates the entity with new values from the model.
            /// </summary>
            _uow.EntityRepository.Update(_mapper.Map<CreateOrUpdateEntityModel, Entity>(model, entity));
            entity.CreatedBy = createdBy;
            entity.CreatedByDateTime = createdDate;

            /// <summary>
            /// Commits the changes to the database.
            /// </summary>
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Removes multiple entities by their IDs.
        /// </summary>
        /// <param name="ids">List of entity IDs to remove.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task RemoveMultiple(List<int> ids)
        {
            /// <summary>
            /// Iterates through each entity ID in the list.
            /// </summary>
            foreach (var id in ids)
            {
                /// <summary>
                /// Retrieves the entity by its ID.
                /// </summary>
                var item = _uow.EntityRepository.GetById(id);

                /// <summary>
                /// If entity exists, removes it from the repository.
                /// </summary>
                if (item != null)
                {
                    _uow.EntityRepository.Remove(item);
                }
            }

            /// <summary>
            /// Commits all changes to the database.
            /// </summary>
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Exports entities to an Excel stream, optionally filtered by selected entity IDs.
        /// </summary>
        /// <param name="selectedEntityIds">List of entity IDs to filter export (null for all entities).</param>
        /// <returns>Stream containing the Excel file with entity data.</returns>
        public async Task<Stream> ExportEntities(List<int> selectedEntityIds)
        {
            /// <summary>
            /// Creates a query joining entities with countries and cities.
            /// </summary>
            var query = from entity in _uow.EntityRepository.Query()
                        join country in _uow.CountryRepository.Query()
                        on entity.CountryId equals country.CountryId
                        join city in _uow.CityRepository.Query()
                        on entity.CityId equals city.CityId
                        select new EntityModelDescription
                        {
                            EntityId = entity.EntityId,
                            EntityName = entity.EntityName,
                            CountryId = entity.CountryId,
                            CountryName = country.CountryName,
                            CityId = entity.CityId,
                            CityName = city.CityName,
                            EntityAddress = entity.EntityAddress,
                            Code = entity.Code,
                            Isparent = entity.Isparent,
                            ParentEnitityId = entity.ParentEnitityId
                        };

            /// <summary>
            /// If specific entity IDs are provided, filters the query by those IDs.
            /// </summary>
            if (selectedEntityIds != null && selectedEntityIds.Count > 0)
            {
                query = query.Where(entity => selectedEntityIds.Contains(entity.EntityId));
            }

            /// <summary>
            /// Executes the query and retrieves the entities as a list.
            /// </summary>
            var entities = await query.ToListAsync();

            /// <summary>
            /// Maps the entities to EntityModelDescription objects.
            /// </summary>
            var models = _mapper.Map<List<EntityModelDescription>>(entities);

            /// <summary>
            /// Sets the EPPlus license context to non-commercial.
            /// </summary>
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            /// <summary>
            /// Creates a new Excel package.
            /// </summary>
            using var package = new ExcelPackage();

            /// <summary>
            /// Adds a new worksheet named "Entities".
            /// </summary>
            var worksheet = package.Workbook.Worksheets.Add("Entities");

            /// <summary>
            /// Gets the properties of the EntityModelDescription type for column headers.
            /// </summary>
            var properties = typeof(EntityModelDescription).GetProperties();

            /// <summary>
            /// Sets the column headers in the first row.
            /// </summary>
            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
            }

            /// <summary>
            /// Populates the worksheet with entity data.
            /// </summary>
            for (int row = 0; row < models.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(models[row]);
                }
            }

            /// <summary>
            /// Auto-fits the columns to the content.
            /// </summary>
            worksheet.Cells.AutoFitColumns();

            /// <summary>
            /// Creates a memory stream to hold the Excel file.
            /// </summary>
            var memoryStream = new MemoryStream();

            /// <summary>
            /// Saves the Excel package to the memory stream.
            /// </summary>
            package.SaveAs(memoryStream);

            /// <summary>
            /// Resets the stream position to the beginning.
            /// </summary>
            memoryStream.Position = 0;

            /// <summary>
            /// Returns the memory stream containing the Excel file.
            /// </summary>
            return memoryStream;
        }

        /// <summary>
        /// Imports entities from an Excel file stream.
        /// </summary>
        /// <param name="fileStream">The stream containing the Excel file.</param>
        /// <param name="createdBy">The user who created the import.</param>
        /// <returns>Task with a string message indicating the import result.</returns>
        public async Task<string> ImportEntities(Stream fileStream, string createdBy)
        {
            /// <summary>
            /// Sets the EPPlus license context to non-commercial.
            /// </summary>
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            /// <summary>
            /// Creates an Excel package from the file stream.
            /// </summary>
            using var package = new ExcelPackage(fileStream);

            /// <summary>
            /// Gets the first worksheet from the workbook.
            /// </summary>
            var worksheet = package.Workbook.Worksheets[0];
            string[] expectedHeaders = [
              "EntityName*",
              "CountryName*",
              "CountryId*",
              "CityName*",
              "CityId*",
              "Address*",
              "IsChild",
              "ParentEntity",
              "ParentEntityId",
              "Code*"
];

            // helper for case-insensitive + trim + remove spaces
            static bool Same(string a, string b)
            {
                return string.Equals(
                    a?.Replace(" ", "").Trim(),
                    b?.Replace(" ", "").Trim(),
                    StringComparison.OrdinalIgnoreCase
                );
            }

            // validate each header cell
            for (int i = 0; i < expectedHeaders.Length; i++)
            {
                string excelHeader = worksheet.Cells[1, i + 1].Text;

                if (!Same(excelHeader, expectedHeaders[i]))
                {
                    return $"Incorrect file format. Expected header '{expectedHeaders[i]}' in column {i + 1}.";
                }
            }

            /// <summary>
            /// Gets the number of rows with data in the worksheet.
            /// </summary>
            int rowCount = GetRowCount(worksheet);

            /// <summary>
            /// List to store the imported entity models.
            /// </summary>
            var models = new List<EntityModel>();

            /// <summary>
            /// Counter for skipped records due to validation errors.
            /// </summary>
            //int skippedRecordsCount = 0;

            /// <summary>
            /// Counter for duplicate records.
            /// </summary>

            /// <summary>
            /// Variable to store the result message.
            /// </summary>
            var resultMessage = "";

            /// <summary>
            /// Gets the last entity to determine the next code.
            /// </summary>
            var lastEntity = _uow.EntityRepository.GetAll()
                     .OrderByDescending(e => e.Code)
                     .FirstOrDefault();

            /// <summary>
            /// Parses the last entity code or defaults to 0.
            /// </summary>
            int lastCode = lastEntity != null && int.TryParse(lastEntity.Code, out int code) ? code : 0;

            /// <summary>
            /// Wraps the import logic in a try-catch block.
            /// </summary>
            try
            {
                int skippedCount = 0;

                /// <summary>
                /// Checks if the worksheet is empty.
                /// </summary>
                if (rowCount == 0 || rowCount == -1)
                {
                    /// <summary>
                    /// Returns message for empty file.
                    /// </summary>
                    return resultMessage = "Uploaded File Is Empty";
                }

                /// <summary>
                /// Reads data from the Excel file row by row.
                /// </summary>
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    try
                    {
                        var Code = worksheet.Cells[row, 10].Text;
                        var entityName = worksheet.Cells[row, 1].Text;
                        var countryIdText = worksheet.Cells[row, 3].Text;
                        var cityIdText = worksheet.Cells[row, 5].Text;
                        var address = worksheet.Cells[row, 6].Text;

                        _ = int.TryParse(countryIdText, out int countryId);
                        _ = int.TryParse(cityIdText, out int cityId);

                        // if required fields missing → skip
                        if (string.IsNullOrWhiteSpace(Code) ||
                            string.IsNullOrWhiteSpace(entityName) ||
                            countryId == 0 ||
                            cityId == 0 ||
                            string.IsNullOrWhiteSpace(address))
                        {
                            skippedCount++;
                            continue;
                        }

                        var model = new EntityModel
                        {
                            Code = Code,
                            EntityName = entityName,
                            CountryId = countryId,
                            CityId = cityId,
                            EntityAddress = address,
                            CreatedBy = createdBy,
                        };

                        models.Add(model);
                    }
                    catch
                    {
                        skippedCount++; // row crashed due to wrong format
                        continue;
                    }
                }

                /// <summary>
                /// Retrieves all existing entities for duplicate checking.
                /// </summary>
                var existingEntities = GetAll();

                int insertedCount = 0;
                int duplicateCount = 0;

                // Process each new model
                foreach (var model in models)
                {
                    // Skip invalid rows
                    if (string.IsNullOrWhiteSpace(model.Code) || string.IsNullOrWhiteSpace(model.EntityName) || (model.CountryId == null || model.CountryId == 0) || (model.CityId == null || model.CityId == 0) || string.IsNullOrWhiteSpace(model.EntityAddress))
                    {
                        skippedCount++;
                        continue;
                    }

                    // Check for duplicates (by code or name/address)
                    bool isDuplicate = existingEntities.Any(existing =>
                        existing.Code == model.Code ||
                        (existing.EntityName == model.EntityName &&
                         existing.CityId == model.CityId &&
                         existing.EntityAddress == model.EntityAddress));

                    if (isDuplicate)
                    {
                        duplicateCount++;
                        continue;
                    }

                    // Assign metadata
                    model.CreatedByDateTime = DateTime.UtcNow;
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    model.CreatedBy = createdBy;
                    model.UpdatedBy = createdBy;

                    // Map and add entity
                    var entity = _mapper.Map<Entity>(model);
                    _uow.EntityRepository.Add(entity);
                    insertedCount++;
                }

                // Commit to database
                await _uow.CompleteAsync();

                //  Build proper result message
                List<string> messageParts = [];

                if (insertedCount > 0)
                    messageParts.Add($"{insertedCount} record{(insertedCount > 1 ? "s" : "")} inserted successfully");

                if (duplicateCount > 0)
                    messageParts.Add($"{duplicateCount} duplicate record{(duplicateCount > 1 ? "s" : "")} skipped");

                if (skippedCount > 0)
                    messageParts.Add($"{skippedCount} invalid record{(skippedCount > 1 ? "s" : "")} skipped due to missing required fields");

                if (messageParts.Count == 0)
                    resultMessage = "No new records to insert.";
                else
                    resultMessage = string.Join("; ", messageParts) + "."; // semicolon-separated for readability

                return resultMessage;
            }

            /// <summary>
            /// Catches any exceptions during import.
            /// </summary>
            catch (Exception ex)
            {
                /// <summary>
                /// Returns the exception message as result.
                /// </summary>
                return resultMessage = ex.Message;
            }

            /// <summary>
            /// Returns the final result message.
            /// </summary>
        }

        /// <summary>
        /// Gets the number of rows with data in the Excel worksheet.
        /// </summary>
        /// <param name="worksheet">The Excel worksheet to check.</param>
        /// <returns>The number of rows with data.</returns>
        static int GetRowCount(ExcelWorksheet worksheet)
        {
            /// <summary>
            /// Gets the last row number in the worksheet.
            /// </summary>
            int lastRow = worksheet.Dimension.End.Row;

            /// <summary>
            /// Counter for rows with data.
            /// </summary>
            //int rowCount = 0;

            /// <summary>
            /// Tracks the last non-empty row.
            /// </summary>
            int lastNonEmptyRow = 0;

            /// <summary>
            /// Iterates through each row to find data.
            /// </summary>
            for (int row = 2; row <= lastRow; row++)
            {
                /// <summary>
                /// Checks if any of the first three columns have data.
                /// </summary>
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text);

                /// <summary>
                /// If row has data, updates the last non-empty row.
                /// </summary>
                if (hasData)
                {
                    lastNonEmptyRow = row;
                }
            }

            /// <summary>
            /// Returns the count of non-empty rows (excluding header).
            /// </summary>
            return lastNonEmptyRow - 1;
        }

        /// <summary>
        /// Downloads an Excel template for entity import with validation and dropdowns.
        /// </summary>
        /// <returns>Task with byte array containing the Excel template.</returns>
        public async Task<byte[]> DownloadTemplate()
        {
            /// <summary>
            /// Retrieves all countries from the country service.
            /// </summary>
            List<CountryModel> countries = _countryService.GetAll();

            /// <summary>
            /// Retrieves all entities.
            /// </summary>
            List<EntityModel> entities = GetAll();

            /// <summary>
            /// Retrieves all cities from the city service, ordered by country ID.
            /// </summary>
            List<CityModel> cities = [.. _cityService.GetAll().OrderBy(x => x.CountryId)];

            /// <summary>
            /// Sanitizes country names for Excel range names.
            /// </summary>
            foreach (var country in countries)
            {
                country.CountryName = SanitizeRangeName(country.CountryName);
            }

            /// <summary>
            /// Sanitizes city names for Excel range names.
            /// </summary>
            foreach (var city in cities)
            {
                city.CityName = SanitizeRangeName(city.CityName);
            }

            /// <summary>
            /// Sets the EPPlus license context to non-commercial.
            /// </summary>
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            /// <summary>
            /// Creates a new Excel package.
            /// </summary>
            using var package = new ExcelPackage();

            /// <summary>
            /// Adds a worksheet named "Entities".
            /// </summary>
            var sheet = package.Workbook.Worksheets.Add("Entities");

            /// <summary>
            /// Defines the column headers for the template.
            /// </summary>
            string[] headers = ["EntityName*", "CountryName*", "CountryId*", "CityName*", "CityId*", "Address*", "IsChild", "ParentEntity", "ParentEntityId", "Code*", "Field Description",];

            /// <summary>
            /// Sets the column headers in the first row.
            /// </summary>
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }

            /// <summary>
            /// Sets hidden column headers for data storage.
            /// </summary>
            sheet.Cells[1, 13].Value = "CountryNameValue";
            sheet.Cells[1, 14].Value = "CountryIdValue";
            sheet.Cells[1, 16].Value = "CityNameValue";
            sheet.Cells[1, 17].Value = "CityIdValue";
            sheet.Cells[1, 18].Value = "CityCountryIdValue";
            sheet.Cells[1, 20].Value = "IsChildValue";
            sheet.Cells[1, 22].Value = "ParentEntityName";
            sheet.Cells[1, 23].Value = "ParentEntityId";

            /// <summary>
            /// Defines options for IsChild dropdown.
            /// </summary>
            string[] IsChildOptions = ["True", "False"];

            /// <summary>
            /// Populates the IsChild options in hidden column.
            /// </summary>
            for (int i = 0; i < IsChildOptions.Length; i++)
            {
                sheet.Cells[i + 2, 20].Value = IsChildOptions[i];
            }

            /// <summary>
            /// Adds description for required fields.
            /// </summary>
            sheet.Cells[2, 11].Value = "* Fields marked with an asterisk are required.";

            /// <summary>
            /// Makes the required field description bold.
            /// </summary>
            sheet.Cells[2, 11].Style.Font.Bold = true;

            /// <summary>
            /// Sets the required field description color to red.
            /// </summary>
            sheet.Cells[2, 11].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            /// <summary>
            /// Populates hidden columns with country data.
            /// </summary>
            PopulateColumn(sheet, [.. countries.Select(e => e.CountryName ?? "")], 13);

            /// <summary>
            /// Populates hidden columns with country ID data.
            /// </summary>
            PopulateColumn(sheet, [.. countries.Select(e => e.CountryId.ToString())], 14);

            /// <summary>
            /// Populates hidden columns with city name data.
            /// </summary>
            PopulateColumn(sheet, [.. cities.Select(e => e.CityName ?? "".ToString())], 16);

            /// <summary>
            /// Populates hidden columns with city ID data.
            /// </summary>
            PopulateColumn(sheet, [.. cities.Select(e => e.CityId.ToString())], 17);

            /// <summary>
            /// Populates hidden columns with city country ID data.
            /// </summary>
            PopulateColumn(sheet, [.. cities.Select(e => e.CountryId.ToString() ?? "")], 18);

            /// <summary>
            /// Populates hidden columns with entity name data.
            /// </summary>
            PopulateColumn(sheet, [.. entities.Select(e => e.EntityName ?? "".ToString())], 22);

            /// <summary>
            /// Populates hidden columns with entity ID data.
            /// </summary>
            PopulateColumn(sheet, [.. entities.Select(e => e.EntityId.ToString())], 23);

            /// <summary>
            /// Applies dropdown validation for country names.
            /// </summary>
            ApplyDropdown(sheet, "CountryNameRange", "B", 13, 100);

            /// <summary>
            /// Disables parent entity selection when IsChild is false.
            /// </summary>
            DisableParentEntityForIsChild(sheet, "G", "H", 100);

            /// <summary>
            /// Applies dropdown validation for IsChild field.
            /// </summary>
            ApplyDropdown(sheet, "IsChildRange", "G", 20, 100);

            /// <summary>
            /// Populates hidden columns with entity name data again.
            /// </summary>
            PopulateColumn(sheet, [.. entities.Select(e => e.EntityName ?? "".ToString())], 22);

            /// <summary>
            /// Defines a named range for parent entity dropdown.
            /// </summary>
            sheet.Workbook.Names.Add("ParentEntityRange", sheet.Cells[2, 22, entities.Count + 1, 22]);

            /// <summary>
            /// Adds formula to populate country ID based on country name.
            /// </summary>
            AddFormula(sheet, "C", "B", 13, 14, countries.Count);

            /// <summary>
            /// Applies parameter-based dropdown for cities.
            /// </summary>
            ApplyParamValueDropdown(sheet, cities);

            /// <summary>
            /// Adds formula to populate city ID based on city name.
            /// </summary>
            AddFormula(sheet, "E", "D", 16, 17, cities.Count);

            /// <summary>
            /// Adds formula to populate parent entity ID based on parent entity name.
            /// </summary>
            AddFormula(sheet, "I", "H", 22, 23, entities.Count);

            /// <summary>
            /// Auto-fits all columns to content.
            /// </summary>
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            /// <summary>
            /// Returns the Excel template as a byte array.
            /// </summary>
            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// Disables parent entity selection when IsChild is false.
        /// </summary>
        /// <param name="sheet">The Excel worksheet.</param>
        /// <param name="conditionColumn">The column containing the IsChild condition.</param>
        /// <param name="targetColumn">The column to disable (parent entity).</param>
        /// <param name="rowCount">The number of rows to apply to.</param>
        private static void DisableParentEntityForIsChild(ExcelWorksheet sheet, string conditionColumn, string targetColumn, int rowCount)
        {
            /// <summary>
            /// Applies conditional formatting and validation to each row.
            /// </summary>
            for (int row = 2; row <= rowCount + 1; row++)
            {
                /// <summary>
                /// Creates cell reference for condition column.
                /// </summary>
                string conditionCell = $"{conditionColumn}{row}";

                /// <summary>
                /// Creates cell reference for target column.
                /// </summary>
                string targetCell = $"{targetColumn}{row}";

                /// <summary>
                /// Adds conditional formatting to visually disable when condition is false.
                /// </summary>
                var conditionFormatting = sheet.ConditionalFormatting.AddExpression(sheet.Cells[targetCell]);

                /// <summary>
                /// Sets the conditional formatting formula.
                /// </summary>
                conditionFormatting.Formula = $"NOT(${conditionCell}=\"True\")";

                /// <summary>
                /// Sets the fill pattern for disabled cells.
                /// </summary>
                conditionFormatting.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;

                /// <summary>
                /// Sets the background color for disabled cells.
                /// </summary>
                conditionFormatting.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                /// <summary>
                /// Sets the text color for disabled cells.
                /// </summary>
                conditionFormatting.Style.Font.Color.SetColor(System.Drawing.Color.DarkGray);

                /// <summary>
                /// Adds dynamic dropdown validation based on condition.
                /// </summary>
                var validation = sheet.DataValidations.AddListValidation(targetCell);

                /// <summary>
                /// Sets the validation formula to show dropdown only when condition is true.
                /// </summary>
                validation.Formula.ExcelFormula = $"IF(${conditionCell}=\"True\", ParentEntityRange, \"\")";

                /// <summary>
                /// Enables error message display.
                /// </summary>
                validation.ShowErrorMessage = true;

                /// <summary>
                /// Sets the error title.
                /// </summary>
                validation.ErrorTitle = "Invalid Selection";

                /// <summary>
                /// Sets the error message.
                /// </summary>
                validation.Error = "Select a valid Parent Entity when IsChild is True.";
            }
        }

        /// <summary>
        /// Applies parameter-based dropdown validation for cities based on selected country.
        /// </summary>
        /// <param name="sheet">The Excel worksheet.</param>
        /// <param name="factors">List of city factors.</param>
        private static void ApplyParamValueDropdown(ExcelWorksheet sheet, List<CityModel> factors)
        {
            /// <summary>
            /// Gets the workbook from the worksheet.
            /// </summary>
            var workbook = sheet.Workbook;

            /// <summary>
            /// Dictionary to map country IDs to city names.
            /// </summary>
            var paramValueMap = new Dictionary<int, List<string>>();

            /// <summary>
            /// Maps each country ID to its corresponding cities.
            /// </summary>
            foreach (var factor in factors)
            {
                /// <summary>
                /// Gets the country ID from the city.
                /// </summary>
                int paramId = (int)factor.CountryId!;

                /// <summary>
                /// Gets the city name.
                /// </summary>
                string valueCombination = factor.CityName ?? "";

                /// <summary>
                /// Adds city to the country's city list.
                /// </summary>
                if (!paramValueMap.TryGetValue(paramId, out List<string>? value))
                {
                    paramValueMap[paramId] = [valueCombination];
                }
                else
                {
                    value.Add(valueCombination);
                }
            }

            /// <summary>
            /// Stores city lists in hidden columns for dropdown validation.
            /// </summary>
            int startColumn = 30;

            /// <summary>
            /// Stores each country's city list in a hidden column.
            /// </summary>
            foreach (var param in paramValueMap)
            {
                /// <summary>
                /// Creates a range name for the country.
                /// </summary>
                string rangeName = $"Param_{param.Key}";

                /// <summary>
                /// Starting row for data.
                /// </summary>
                int rowIndex = 2;

                /// <summary>
                /// Populates the hidden column with city names.
                /// </summary>
                foreach (var value in param.Value)
                {
                    sheet.Cells[rowIndex, startColumn].Value = value;
                    rowIndex++;
                }

                /// <summary>
                /// Defines the range for the city list.
                /// </summary>
                var range = sheet.Cells[2, startColumn, rowIndex - 1, startColumn];

                /// <summary>
                /// Adds the named range to the workbook.
                /// </summary>
                workbook.Names.Add(rangeName, range);

                /// <summary>
                /// Moves to the next hidden column.
                /// </summary>
                startColumn++;
            }

            /// <summary>
            /// Applies dynamic dropdown validation to each row.
            /// </summary>
            for (int row = 2; row <= 100; row++)
            {
                /// <summary>
                /// Adds list validation to the city column.
                /// </summary>
                var validation = sheet.DataValidations.AddListValidation($"D{row}");

                /// <summary>
                /// Sets the validation formula to use INDIRECT based on selected country.
                /// </summary>
                validation.Formula.ExcelFormula = $"INDIRECT(\"Param_\"&C{row})";

                /// <summary>
                /// Enables error message display.
                /// </summary>
                validation.ShowErrorMessage = true;

                /// <summary>
                /// Sets the error style to stop.
                /// </summary>
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;

                /// <summary>
                /// Sets the error title.
                /// </summary>
                validation.ErrorTitle = "Invalid Selection";

                /// <summary>
                /// Sets the error message.
                /// </summary>
                validation.Error = "Please select a valid ParamValue.";
            }
        }

        /// <summary>
        /// Populates a worksheet column with values.
        /// </summary>
        /// <param name="sheet">The Excel worksheet.</param>
        /// <param name="values">The values to populate.</param>
        /// <param name="columnIndex">The column index to populate.</param>
        private static void PopulateColumn(ExcelWorksheet sheet, string[] values, int columnIndex)
        {
            /// <summary>
            /// Populates each cell in the column with the corresponding value.
            /// </summary>
            for (int i = 0; i < values.Length; i++)
            {
                sheet.Cells[i + 2, columnIndex].Value = values[i];
            }
        }

        /// <summary>
        /// Applies dropdown validation to a column.
        /// </summary>
        /// <param name="sheet">The Excel worksheet.</param>
        /// <param name="rangeName">The name of the range for validation.</param>
        /// <param name="column">The column letter to apply validation to.</param>
        /// <param name="dataColumnIndex">The column index containing the validation data.</param>
        /// <param name="maxRows">The maximum number of rows to apply validation to.</param>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int maxRows)
        {
            /// <summary>
            /// Gets the last row with data in the validation column.
            /// </summary>
            int lastRow = sheet.Cells[sheet.Dimension.Start.Row, dataColumnIndex, sheet.Dimension.End.Row, dataColumnIndex]
                .Where(c => c.Value != null).Count();

            /// <summary>
            /// If no data available, skips dropdown creation.
            /// </summary>
            if (lastRow <= 1) return;

            /// <summary>
            /// Defines the range for validation data.
            /// </summary>
            var range = sheet.Cells[2, dataColumnIndex, lastRow, dataColumnIndex];

            /// <summary>
            /// Adds the named range to the workbook.
            /// </summary>
            sheet.Workbook.Names.Add(rangeName, range);

            /// <summary>
            /// Applies dropdown validation to each row.
            /// </summary>
            for (int row = 2; row <= maxRows; row++)
            {
                /// <summary>
                /// Adds list validation to the cell.
                /// </summary>
                var validation = sheet.DataValidations.AddListValidation($"{column}{row}");

                /// <summary>
                /// Sets the validation formula to use the named range.
                /// </summary>
                validation.Formula.ExcelFormula = rangeName;

                /// <summary>
                /// Enables error message display.
                /// </summary>
                validation.ShowErrorMessage = true;

                /// <summary>
                /// Sets the error style to stop.
                /// </summary>
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;

                /// <summary>
                /// Sets the error title.
                /// </summary>
                validation.ErrorTitle = "Invalid Selection";

                /// <summary>
                /// Sets the error message.
                /// </summary>
                validation.Error = "Please select a valid option.";
            }
        }

        /// <summary>
        /// Adds a formula to populate IDs based on names using VLOOKUP.
        /// </summary>
        /// <param name="sheet">The Excel worksheet.</param>
        /// <param name="resultColumn">The column to display the result (ID).</param>
        /// <param name="lookupColumn">The column containing the name to look up.</param>
        /// <param name="dataStartColumn">The starting column of the data range.</param>
        /// <param name="idColumn">The column containing the IDs in the data range.</param>
        /// <param name="dataCount">The number of data rows.</param>
        private static void AddFormula(ExcelWorksheet sheet, string resultColumn, string lookupColumn, int dataStartColumn, int idColumn, int dataCount)
        {
            /// <summary>
            /// Creates the address for the data range.
            /// </summary>
            string rangeAddress = sheet.Cells[2, dataStartColumn, dataCount + 1, idColumn].Address;

            /// <summary>
            /// Adds VLOOKUP formula to each row in the result column.
            /// </summary>
            for (int row = 2; row <= 100; row++)
            {
                sheet.Cells[row, resultColumn[0] - 'A' + 1].Formula = $"IF({lookupColumn}{row}=\"\", \"\", VLOOKUP({lookupColumn}{row}, {rangeAddress}, 2, FALSE))";
            }
        }

        /// <summary>
        /// Sanitizes range names for Excel compatibility.
        /// </summary>
        /// <param name="name">The original name to sanitize.</param>
        /// <returns>The sanitized range name.</returns>
        private static string SanitizeRangeName(string? name)
        {
            /// <summary>
            /// Replaces spaces with underscores.
            /// </summary>
            string sanitized = name ?? "".Replace(" ", "_");

            /// <summary>
            /// Removes invalid characters using regex.
            /// </summary>
            sanitized = MyRegex().Replace(sanitized, "_");

            /// <summary>
            /// Adds underscore prefix if name starts with digit.
            /// </summary>
            if (char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }

            /// <summary>
            /// Returns the sanitized name.
            /// </summary>
            return sanitized;
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"[^A-Za-z0-9_]")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
    }
}
