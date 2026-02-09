using System.ComponentModel;
using AutoMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing eligibility lists operations including CRUD, import, and export functionality.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the ManagedListService class with required dependencies.
    /// </remarks>
    /// <param name="uow">The unit of work instance for database operations.</param>
    /// <param name="mapper">The AutoMapper instance for object mapping.</param>
    /// <param name="entityService">The entity service instance for entity operations.</param>
    public class ManagedListService(IUnitOfWork uow, IMapper mapper/*, IEntityService entityService*/) : IManagedListService
    {
        // The unit of work instance for database operations and transaction management
        private readonly IUnitOfWork _uow = uow;

        // The AutoMapper instance for converting between entities and models
        private readonly IMapper _mapper = mapper;

        // The entity service instance for entity-related operations
        //private readonly IEntityService _entityService = entityService;

        /// <summary>
        /// Adds a new managed list to the repository with current timestamp information.
        /// </summary>
        /// <param name="model">The managed list model containing data to add.</param>
        public async Task Add(ManagedListAddUpdateModel model)
        {
            var res = _uow.ManagedListRepository.Query().Any(p => p.ListName == model.ListName && p.TenantId == model.TenantId);
            if (res)
            {
                // Throws exception if parameter name already exists
                throw new Exception("List name already exists in this entity");
            }

            // Map the input model to a ManagedList entity
            var entities = _mapper.Map<ManagedList>(model);

            // Set the last updated timestamp to current UTC time
            entities.UpdatedByDateTime = DateTime.UtcNow;
            // Set the creation timestamp to current UTC time
            entities.CreatedByDateTime = DateTime.UtcNow;

            // Add the entity to the repository
            _uow.ManagedListRepository.Add(entities);

            // Save changes to the database asynchronously
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a specific managed list by entity ID and list ID.
        /// </summary>
        /// <param name="tenantId">The ID of the entity that owns the list.</param>
        /// <param name="id">The ID of the list to delete.</param>
        public async Task Delete(int tenantId, int id)
        {
            // Query the repository to find the specific list by entity and list ID
            var Item = _uow.ManagedListRepository.Query().First(f => f.ListId == id && f.TenantId == tenantId);
            // Mark the entity for removal
            _uow.ManagedListRepository.Remove(Item);
            // Save the deletion to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Retrieves all managed lists for a specific entity.
        /// </summary>
        /// <param name="tenantId">The ID of the entity to retrieve lists for.</param>
        /// <returns>A list of managed list models for the specified entity.</returns>
        public List<ManagedListGetModel> GetAll(int tenantId)
        {
            // Query the repository for all lists belonging to the specified entity
            // Map the entities to get models and return as a list
            return _mapper.Map<List<ManagedListGetModel>>(_uow.ManagedListRepository.GetAllByTenantId(tenantId));
        }

        /// <summary>
        /// Retrieves a specific managed list by entity ID and list ID.
        /// </summary>
        /// <param name="tenantId">The ID of the entity that owns the list.</param>
        /// <param name="id">The ID of the list to retrieve.</param>
        /// <returns>The managed list model for the specified IDs.</returns>
        public ManagedListGetModel GetById(int tenantId, int id)
        {
            // Query the repository for a specific list by entity and list ID
            // Map the entity to a get model and return it
            return _mapper.Map<ManagedListGetModel>(_uow.ManagedListRepository.Query().First(f => f.ListId == id && f.TenantId == tenantId));
        }

        /// <summary>
        /// Updates an existing managed list with new data.
        /// </summary>
        /// <param name="model">The managed list update model containing new data.</param>
        public async Task Update(ManagedListUpdateModel model)
        {

            var res = _uow.ManagedListRepository.Query().Any(p => p.ListName == model.ListName && p.TenantId == model.TenantId && model.ListId != p.ListId);
            if (res)
            {
                // Throws exception if parameter name already exists
                throw new Exception("List name already exists in this entity");
            }

            // Find the existing entity to update
            var Item = _uow.ManagedListRepository.Query().First(f => f.ListId == model.ListId && f.TenantId == model.TenantId);
            // Map the update model data onto the existing entity
            var entities = _mapper.Map<ManagedListModel, ManagedList>(model, Item);
            // Set the updated by information from the model
            entities.UpdatedBy = model.UpdatedBy ?? "";
            // Set the last updated timestamp to current UTC time
            entities.UpdatedByDateTime = DateTime.UtcNow;

            // Mark the entity as updated in the repository
            _uow.ManagedListRepository.Update(entities);
            // Save changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple managed lists by their IDs for a given entity.
        /// </summary>
        /// <param name="tenantId">The ID of the entity that owns the lists.</param>
        /// <param name="ids">The list of managed list IDs to delete.</param>
        public async Task MultipleDelete(int tenantId, List<int> ids)
        {
            // Iterate through each ID in the provided list
            foreach (var id in ids)
            {
                // Find the managed list by entity ID and list ID
                var manageitem = _uow.ManagedListRepository.Query().First(f => f.ListId == id && f.TenantId == tenantId);
                // If the item exists, mark it for removal
                if (manageitem != null)
                {
                    _uow.ManagedListRepository.Remove(manageitem);
                }
            }
            try
            {
                // Attempt to save all deletions to the database
                await _uow.CompleteAsync();
            }
            catch (DbUpdateException ex)
            {
                // If a database update exception occurs, throw a new exception with the message
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Exports selected managed lists to an Excel file stream.
        /// </summary>
        /// <param name="tenantId">The ID of the entity to export lists for.</param>
        /// <param name="selectedListIds">The list of specific list IDs to export, or null for all lists.</param>
        /// <returns>A stream containing the Excel file with exported data.</returns>
        public async Task<Stream> ExportLists(int tenantId, List<int> selectedListIds)
        {
            // Create a query joining managed lists with their associated entities
            var lists = from managedlist in _uow.ManagedListRepository.Query()
                        join entity in _uow.EntityRepository.Query()
                        on managedlist.TenantId equals entity.EntityId
                        where managedlist.TenantId == tenantId && entity.EntityId == tenantId
                        select new ManagedListModelDescription
                        {
                            ListId = managedlist.ListId,
                            ListName = managedlist.ListName,
                            TenantId = managedlist.TenantId,
                            EntityName = entity.EntityName ?? ""
                        };

            // If specific list IDs are provided, filter the query to only include those IDs
            if (selectedListIds != null && selectedListIds.Count > 0)
            {
                lists = lists.Where(entity => selectedListIds.Contains(entity.ListId));
            }

            // Execute the query and get the results as a list
            var List = await lists.ToListAsync();

            // Map the entity results to model objects
            var models = _mapper.Map<List<ManagedListModelDescription>>(List);

            // Set the EPPlus license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Create a new Excel package
            using var package = new ExcelPackage();
            // Add a worksheet named "Lists" to the workbook
            var worksheet = package.Workbook.Worksheets.Add("Lists");

            // Get the properties of the model type for column headers
            var properties = typeof(ManagedListModelDescription).GetProperties();
            // Loop through each property to create column headers
            for (int col = 0; col < properties.Length; col++)
            {
                // Set the header cell value to the property name
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
            }

            // Loop through each model to populate the worksheet data
            for (int row = 0; row < models.Count; row++)
            {
                // Loop through each property of the current model
                for (int col = 0; col < properties.Length; col++)
                {
                    // Set the cell value to the property value of the current model
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(models[row]);
                }
            }

            // Auto-fit all columns to content for better readability
            worksheet.Cells.AutoFitColumns();

            // Create a memory stream to hold the Excel file
            var memoryStream = new MemoryStream();
            // Save the Excel package to the memory stream
            package.SaveAs(memoryStream);
            // Reset the stream position to the beginning for reading
            memoryStream.Position = 0;
            // Return the stream containing the Excel file
            return memoryStream;
        }

        /// <summary>
        /// Generates and downloads an Excel template for managed list import.
        /// </summary>
        /// <returns>A byte array containing the Excel template file.</returns>
        public async Task<byte[]> DownloadTemplate()
        {
            // Get all entities from the entity service
            //List<EntityModel> entities = _entityService.GetAll();

            // Set the EPPlus license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Create a new Excel package
            using var package = new ExcelPackage();
            // Add a worksheet named "Lists" to the workbook
            var sheet = package.Workbook.Worksheets.Add("Lists");

            // Define the column headers for the template
            string[] headers = ["ListName*", "Field Description"];
            // Loop through headers to populate the first row
            for (int i = 0; i < headers.Length; i++)
            {
                // Set each header cell value
                sheet.Cells[1, i + 1].Value = headers[i];
            }

            // Add description for required fields in the fourth column
            sheet.Cells[2, 2].Value = "* Fields marked with an asterisk are required.";
            // Make the description text bold for emphasis
            sheet.Cells[2, 4].Style.Font.Bold = true;
            // Set the description text color to red for visibility
            sheet.Cells[2, 4].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Set header for entity name reference data
            //sheet.Cells[1, 10].Value = "EntityName";
            //// Set header for entity ID reference data
            //sheet.Cells[1, 11].Value = "TenantId";

            // Populate the entity name reference column with entity names
            //PopulateColumn(sheet, [.. entities.Select(e => e.EntityName!)], 10);
            //// Populate the entity ID reference column with entity IDs
            //PopulateColumn(sheet, [.. entities.Select(e => e.TenantId.ToString())], 11);

            // Apply dropdown validation to the EntityName column (column B)
            //ApplyDropdown(sheet, "EntityNameRange", "B", 10, 100);

            //// Add VLOOKUP formula to automatically populate TenantId based on EntityName
            //AddFormula(sheet, "C", "B", 10, 11, 100);

            // Auto-fit all columns to content
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            // Return the Excel package as a byte array
            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// Populates a specified column in the Excel worksheet with values.
        /// </summary>
        /// <param name="sheet">The Excel worksheet to populate.</param>
        /// <param name="values">Array of string values to populate the column with.</param>
        /// <param name="columnIndex">The 1-based index of the column to populate.</param>
        private static void PopulateColumn(ExcelWorksheet sheet, string[] values, int columnIndex)
        {
            // Check if the values array is empty
            if (values.Length == 0)
            {
                // Ensure at least one blank entry to avoid errors
                sheet.Cells[2, columnIndex].Value = "";
                // Exit the method
                return;
            }

            // Loop through each value in the array
            for (int i = 0; i < values.Length; i++)
            {
                // Populate each cell in the column starting from row 2
                sheet.Cells[i + 2, columnIndex].Value = values[i];
            }
        }

        /// <summary>
        /// Applies dropdown list validation to a specified column in the Excel worksheet.
        /// </summary>
        /// <param name="sheet">The Excel worksheet to apply validation to.</param>
        /// <param name="rangeName">The name of the named range containing dropdown values.</param>
        /// <param name="column">The column letter where dropdown should be applied.</param>
        /// <param name="dataColumnIndex">The column index containing the source data for dropdown.</param>
        /// <param name="maxRows">The maximum number of rows to apply the dropdown to.</param>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int maxRows)
        {
            // Detect non-empty values in the source data column
            int lastRow = sheet.Cells[sheet.Dimension.Start.Row, dataColumnIndex, sheet.Dimension.End.Row, dataColumnIndex]
                .Where(c => c.Value != null).Count();

            // Ensure at least one blank row is available for dropdowns
            if (lastRow == 0)
            {
                // Add a blank value to avoid errors
                sheet.Cells[2, dataColumnIndex].Value = "";
                // Set last row to 2 (first data row)
                lastRow = 2;
            }

            // Create a range covering only the filled cells in the source column
            var range = sheet.Cells[2, dataColumnIndex, lastRow, dataColumnIndex];
            // Add a named range for the dropdown values
            sheet.Workbook.Names.Add(rangeName, range);

            // Apply dropdown validation to each row in the target column
            for (int row = 2; row <= maxRows; row++)
            {
                // Create list validation for the current cell
                var validation = sheet.DataValidations.AddListValidation($"{column}{row}");
                // Set the formula to reference the named range
                validation.Formula.ExcelFormula = rangeName;
                // Enable error message display
                validation.ShowErrorMessage = true;
                // Set error style to stop (prevent invalid entries)
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                // Set the error title
                validation.ErrorTitle = "Invalid Selection";
                // Set the error message content
                validation.Error = "Please select a valid option.";
            }
        }

        /// <summary>
        /// Adds VLOOKUP formulas to a column to automatically populate values based on another column.
        /// </summary>
        /// <param name="sheet">The Excel worksheet to add formulas to.</param>
        /// <param name="resultColumn">The column letter where formula results will be placed.</param>
        /// <param name="lookupColumn">The column letter containing the lookup value.</param>
        /// <param name="dataStartColumn">The starting column index of the lookup table.</param>
        /// <param name="idColumn">The column index containing the result values in the lookup table.</param>
        /// <param name="dataCount">The number of data rows in the lookup table.</param>
        private static void AddFormula(ExcelWorksheet sheet, string resultColumn, string lookupColumn, int dataStartColumn, int idColumn, int dataCount)
        {
            // Create the address range for the lookup table
            string rangeAddress = sheet.Cells[2, dataStartColumn, dataCount + 1, idColumn].Address;

            // Apply the VLOOKUP formula to each row in the result column
            for (int row = 2; row <= 100; row++)
            {
                // Set the formula: if lookup value is empty, result is empty, else perform VLOOKUP
                sheet.Cells[row, resultColumn[0] - 'A' + 1].Formula = $"IF({lookupColumn}{row}=\"\", \"\", VLOOKUP({lookupColumn}{row}, {rangeAddress}, 2, FALSE))";
            }
        }
        /// <summary>
        /// Imports managed lists from an uploaded Excel file.
        /// </summary>
        /// <param name="tenantId">The entity ID that will own the imported lists.</param>
        /// <param name="fileStream">The stream containing the Excel file data.</param>
        /// <param name="createdBy">The username of the user performing the import.</param>
        /// <returns>A message indicating the result of the import operation.</returns>
        public async Task<string> ImportList(int tenantId, Stream fileStream, string createdBy)
        {
            // Set EPPlus license context to non-commercial to avoid licensing issues
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Create Excel package from the file stream using disposable pattern
            using var package = new ExcelPackage(fileStream);

            // Get the first worksheet from the workbook
            var worksheet = package.Workbook.Worksheets[0];
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
            // Calculate the number of data rows in the worksheet
            int rowCount = GetRowCount(worksheet);

            // Initialize list to store valid models extracted from Excel
            var models = new List<ManagedList>();

            // Counter for records skipped due to validation errors
            int skippedRecordsCount = 0;

            // Counter for duplicate records found in database
            int dublicatedRecordsCount = 0;

            // Counter for successfully inserted records
            int insertedRecordsCount = 0;

            // Variable to store the final result message
            var resultMessage = "";

            try
            {
                // Check if worksheet contains any data rows
                if (rowCount == 0 || rowCount == -1)
                {
                    // Return empty file message if no data found
                    return resultMessage = "Uploaded File Is Empty";
                }

                // Process each row in the Excel worksheet (starting from row 2 to skip headers)
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Extract list name from column 1 (A) of current row
                    var ListName = worksheet.Cells[row, 1].Text;

                    // Extract entity ID from column 3 (C) of current row
                    //var TenantId = worksheet.Cells[row, 3].Text;

                    // Validate required fields: list name must not be empty and entity ID must be numeric
                    if (string.IsNullOrWhiteSpace(ListName))
                    {
                        // Increment skip counter and move to next row if validation fails
                        skippedRecordsCount++;
                        continue;
                    }

                    // Create new ManagedList object with validated data
                    var model = new ManagedList
                    {
                        // Set list name from Excel data
                        ListName = ListName,

                        // Parse and set entity ID from Excel data
                        TenantId = tenantId,

                        // Set created by user from method parameter
                        CreatedBy = createdBy
                    };

                    // Add valid model to the collection
                    models.Add(model);
                }

                // Check if all records were skipped due to validation errors
                if (skippedRecordsCount == rowCount)
                {
                    // Return message indicating no valid records found
                    return "No new records to insert.";
                }

                // Process each valid model for database insertion
                foreach (var model in models)
                {
                    // Check if record already exists in database with same entity ID and list name
                    var existingEntity = await _uow.ManagedListRepository.Query().AnyAsync(p => p.TenantId == tenantId && p.ListName == model.ListName);

                    // Skip insertion if duplicate record found
                    if (existingEntity)
                    {
                        // Increment duplicate counter and skip to next model
                        dublicatedRecordsCount++;
                        continue;
                    }

                    // Set entity ID from method parameter (overriding Excel value)
                    model.TenantId = tenantId;

                    // Set last updated timestamp to current UTC time
                    model.UpdatedByDateTime = DateTime.UtcNow;

                    // Set creation timestamp to current UTC time
                    model.CreatedByDateTime = DateTime.UtcNow;

                    // Redundant update of last updated timestamp (could be optimized)
                    model.UpdatedByDateTime = DateTime.UtcNow;

                    // Map model to entity and add to repository for insertion
                    _uow.ManagedListRepository.Add(_mapper.Map<ManagedList>(model));

                    // Increment successful insertion counter
                    insertedRecordsCount++;
                }

                // Persist all changes to the database
                await _uow.CompleteAsync();

                // Build result message based on operation outcomes
                if (insertedRecordsCount > 0)
                {
                    // Add success message with count of inserted records
                    resultMessage = $"{insertedRecordsCount} List Inserted Successfully.";
                }

                // Add skip information if any records were skipped
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $" {skippedRecordsCount} records were not inserted because of missing required field.";
                }

                // Add duplicate information if any duplicates were found
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $" {dublicatedRecordsCount} record already exists.";
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return error message
                resultMessage = "Error On Lists page = " + ex.Message;
            }

            // Return the final result message
            return resultMessage;
        }

        /// <summary>
        /// Gets the row count of non-empty rows in the worksheet.
        /// </summary>
        /// <param name="worksheet">The Excel worksheet.</param>
        /// <returns>The number of non-empty rows.</returns>
        static int GetRowCount(ExcelWorksheet worksheet)
        {
            // Get the last row number in the worksheet dimension
            int lastRow = worksheet.Dimension.End.Row;

            // Initialize row count variable
            //int rowCount = 0;

            // Variable to track the last row that contains data
            int lastNonEmptyRow = 0;

            // Iterate through rows starting from row 2 (skipping header)
            for (int row = 2; row <= lastRow; row++)
            {
                // Check if any of the first three columns contain data
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text);

                // Update last non-empty row if current row contains data
                if (hasData)
                {
                    lastNonEmptyRow = row;  // Track the last row that has data
                }
            }

            // Return count of data rows (subtract 1 to exclude header row)
            return lastNonEmptyRow - 1;
        }
    }
}
