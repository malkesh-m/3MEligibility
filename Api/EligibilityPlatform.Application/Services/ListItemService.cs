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
    /// Service class for managing ListItem operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ListItemService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="entityService">The entity service instance.</param>
    /// <param name="managedListService">The managed list service instance.</param>
    public class ListItemService(IUnitOfWork uow, IMapper mapper, IEntityService entityService, IManagedListService managedListService) : IListItemService
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
        /// The entity service instance.
        /// </summary>
        private readonly IEntityService _entityService = entityService;

        /// <summary>
        /// The managed list service instance.
        /// </summary>
        private readonly IManagedListService managedListService = managedListService;

        /// <summary>
        /// Adds a new list item to the database.
        /// </summary>
        /// <param name="model">The ListItemModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(ListItemCreateUpdateModel model)
        {
            var duplicateCode = _uow.ListItemRepository.Query().Any(p => p.ListId == model.ListId && model.Code == p.Code);
            if (duplicateCode)
            {
                // Throws exception if parameter name already exists
                throw new Exception("Code already exists in this List");
            }
            var res = _uow.ListItemRepository.Query().Any(p => p.ListId == model.ListId && model.ItemName == p.ItemName);
            if (res)
            {
                // Throws exception if parameter name already exists
                throw new Exception("Item already exists in this List");
            }
            // Sets ListId to null if it is 0
            model.ListId = model.ListId == 0 ? (int?)null : model.ListId;
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Sets the creation timestamp to current UTC time
            model.CreatedByDateTime = DateTime.UtcNow;
            // Maps the incoming model to ListItem entity and adds to repository
            _uow.ListItemRepository.Add(_mapper.Map<ListItem>(model));

            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a list item by its ID.
        /// </summary>
        /// <param name="id">The ID of the list item to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int id)
        {
            // Retrieves the list item by ID
            var Item = _uow.ListItemRepository.GetById(id);
            // Removes the list item from the repository
            _uow.ListItemRepository.Remove(Item);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all list items.
        /// </summary>
        /// <returns>A list of ListItemModel representing all list items.</returns>
        public List<ListItemModel> GetAll()
        {
            // Retrieves all list items from the repository
            var items = _uow.ListItemRepository.GetAll();
            // Maps the list items to ListItemModel objects
            return _mapper.Map<List<ListItemModel>>(items);
        }

        /// <summary>
        /// Gets a list item by its ID.
        /// </summary>
        /// <param name="id">The ID of the list item to retrieve.</param>
        /// <returns>The ListItemModel for the specified ID.</returns>
        public ListItemModel GetById(int id)
        {
            // Retrieves the specific list item by ID
            var item = _uow.ListItemRepository.GetById(id);
            // Maps the list item to ListItemModel object
            return _mapper.Map<ListItemModel>(item);
        }

        /// <summary>
        /// Updates an existing list item.
        /// </summary>
        /// <param name="model">The ListItemModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(ListItemCreateUpdateModel model)
        {
            var duplicateCode = _uow.ListItemRepository.Query().Any(p => p.ListId == model.ListId && model.Code == p.Code && model.ItemId != p.ItemId);
            if (duplicateCode)
            {
                // Throws exception if parameter name already exists
                throw new Exception("Code already exists in this List");
            }
            var res = _uow.ListItemRepository.Query().Any(p => p.ListId == model.ListId && model.ItemName == p.ItemName && model.ItemId != p.ItemId);
            if (res)
            {
                // Throws exception if parameter name already exists
                throw new Exception("Item already exists in this List");
            }
            // Sets ListId to null if it is 0

            model.ListId = model.ListId == 0 ? (int?)null : model.ListId;
            // Retrieves the existing list item by ID
            var Item = _uow.ListItemRepository.GetById(model.ItemId);
            // Sets the update timestamp to current UTC time
            model.UpdatedByDateTime = DateTime.UtcNow;
            // Sets the update timestamp to current UTC time again (redundant)
            model.UpdatedByDateTime = DateTime.UtcNow;
            var userName = Item.CreatedBy;
            // Updates the list item with mapped data from the model
            _uow.ListItemRepository.Update(_mapper.Map<ListItemCreateUpdateModel, ListItem>(model, Item));
            Item.CreatedBy = userName;
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes multiple list items by their IDs.
        /// </summary>
        /// <param name="ids">A list of IDs of the list items to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MultipleDelete(List<int> ids)
        {
            // Processes each ID in the provided list
            foreach (var id in ids)
            {
                // Retrieves the list item by ID
                var historypcitem = _uow.ListItemRepository.GetById(id);
                // Removes the list item if found
                if (historypcitem != null)
                {
                    _uow.ListItemRepository.Remove(historypcitem);
                }
            }

            // Commits all deletion changes to the database with error handling
            try
            {
                await _uow.CompleteAsync();
            }
            catch (DbUpdateException ex)
            {
                // Throws exception with database update error message
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Exports selected list items to an Excel file.
        /// </summary>
        /// <param name="selectedListItemIds">A list of selected list item IDs to export.</param>
        /// <returns>A task that represents the asynchronous operation, with a stream containing the exported Excel file.</returns>
        public async Task<Stream> ExportListIteam(List<int> selectedListItemIds)
        {
            // Queries list items joined with managed lists
            var lists = from list in _uow.ListItemRepository.Query()
                        join managedlist in _uow.ManagedListRepository.Query()
                        on list.ListId equals managedlist.ListId
                        select new ListItemModelDescription
                        {
                            ItemId = list.ItemId,
                            ItemName = list.ItemName,
                            ListId = list.ListId,
                            ListName = managedlist.ListName ?? ""
                        };

            // Filters by selected IDs if provided
            if (selectedListItemIds != null && selectedListItemIds.Count > 0)
            {
                lists = lists.Where(listItem => selectedListItemIds.Contains(listItem.ItemId));
            }

            // Executes the query and retrieves results
            var Lists = await lists.ToListAsync();
            // Maps results to model objects
            var models = _mapper.Map<List<ListItemModelDescription>>(Lists);

            // Sets EPPlus license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new Excel package
            using var package = new ExcelPackage();
            // Adds a new worksheet named "ListItem"
            var worksheet = package.Workbook.Worksheets.Add("ListItem");

            // Gets properties of the model for column headers
            var properties = typeof(ListItemModelDescription).GetProperties();
            // Populates header row with property names
            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
            }

            // Populates data rows with model values
            for (int row = 0; row < models.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(models[row]);
                }
            }

            // Auto-fits columns to content
            worksheet.Cells.AutoFitColumns();

            // Saves package to memory stream and returns
            var memoryStream = new MemoryStream();
            package.SaveAs(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Imports list items from an Excel file into the database.
        /// </summary>
        /// <param name="fileStream">The file stream containing the Excel data to import.</param>
        /// <param name="createdBy">The user who is performing the import operation.</param>
        /// <returns>A task that represents the asynchronous operation, with a string message describing the result.</returns>
        public async Task<string> ImportListIteams(Stream fileStream, string createdBy)
        {
            // Sets EPPlus license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Loads Excel package from file stream
            using var package = new ExcelPackage(fileStream);
            // Gets the first worksheet
            var worksheet = package.Workbook.Worksheets[0];
            string[] expectedHeaders =
            [
        "ListName*",
        "ItemName*",
        "ListId*",
        "Code*"
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
            // Gets the number of rows with data
            int rowCount = GetRowCount(worksheet);
            // Initializes list for imported models
            var models = new List<ListItemModel>();
            // Counters for tracking import results
            int skippedRecordsCount = 0;
            int dublicatedRecordsCount = 0;
            int insertedRecordsCount = 0;
            var resultMessage = "";

            try
            {
                // Returns message if worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    return resultMessage = "Uploaded File Is Empty";
                }

                // Reads data from Excel rows
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Gets values from specific columns
                    var ItemName = worksheet.Cells[row, 2].Text;
                    var ListId = worksheet.Cells[row, 3].Text;
                    var Code = worksheet.Cells[row, 4].Text;

                    // Skips rows with missing or invalid data
                    if (string.IsNullOrWhiteSpace(ItemName) || !int.TryParse(ListId, out _) || string.IsNullOrWhiteSpace(Code))
                    {
                        skippedRecordsCount++;
                        continue;
                    }

                    // Creates model from valid data
                    var model = new ListItemModel
                    {
                        ItemName = ItemName,
                        ListId = int.Parse(ListId),
                        CreatedBy = createdBy,
                        Code = Code
                    };
                    models.Add(model);
                }

                // Returns message if no valid records found
                if (skippedRecordsCount == rowCount)
                {
                    return "No new records to insert.";
                }

                // Processes each model for insertion
                foreach (var model in models)
                {
                    // Checks for existing list item with same name and list ID
                    var existingEntity = await _uow.ListItemRepository.Query().AnyAsync(p => p.ItemName == model.ItemName && p.ListId == model.ListId);
                    if (existingEntity)
                    {
                        dublicatedRecordsCount++;
                        continue;
                    }

                    // Sets ListId to null if it is 0
                    model.ListId = model.ListId == 0 ? (int?)null : model.ListId;
                    // Sets creation and update timestamps
                    model.CreatedByDateTime = DateTime.UtcNow;
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    model.UpdatedBy = createdBy;
                    // Marks as imported
                    model.IsImport = true;
                    // Maps and adds to repository
                    _uow.ListItemRepository.Add(_mapper.Map<ListItem>(model));
                    insertedRecordsCount++;
                }

                // Commits changes to database
                await _uow.CompleteAsync();

                // Builds result message based on import outcomes
                if (insertedRecordsCount > 0)
                {
                    resultMessage = $"{insertedRecordsCount} List Items Inserted Successfully.";
                }
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $" {skippedRecordsCount} records were not inserted because of missing required field.";
                }
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $" {dublicatedRecordsCount} record with the same ItemName and ListId already exists.";
                }
            }
            catch (Exception ex)
            {
                // Returns error message if exception occurs
                resultMessage = "Error On List Iteam page = " + ex.Message;
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
            // Gets the last row index from worksheet dimension
            int lastRow = worksheet.Dimension.End.Row;
            //int rowCount = 0;
            int lastNonEmptyRow = 0;

            // Iterates through rows starting from row 2
            for (int row = 2; row <= lastRow; row++)
            {
                // Checks if any of the cells in columns 1, 2, or 3 contain data
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text);

                // Tracks the last row with data
                if (hasData)
                {
                    lastNonEmptyRow = row;
                }
            }

            // Returns the count of rows with data (adjusting for header row)
            return lastNonEmptyRow - 1;
        }

        /// <summary>
        /// Downloads an Excel template for list items.
        /// </summary>
        /// <param name="entityId">The entity ID for which to generate the template.</param>
        /// <returns>A task that represents the asynchronous operation, with the Excel file as a byte array.</returns>
        public async Task<byte[]> DownloadTemplate(int entityId)
        {
            // Gets all managed lists for the specified entity
            List<ManagedListGetModel> listItem = managedListService.GetAll(entityId);

            // Sets EPPlus license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new Excel package
            using var package = new ExcelPackage();
            // Adds a new worksheet named "ListItem"
            var sheet = package.Workbook.Worksheets.Add("ListItem");

            // Defines column headers
            string[] headers = ["ListName*", "ItemName*", "ListId*", "Code*", "Field Description"];
            // Populates header row
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }

            // Adds description for required fields
            sheet.Cells[2, 5].Value = "* Fields marked with an asterisk are required.";
            // Applies bold formatting to required fields description
            sheet.Cells[2, 5].Style.Font.Bold = true;
            // Applies red color to required fields description for visibility
            sheet.Cells[2, 5].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Adds reference column headers for internal data
            sheet.Cells[1, 11].Value = "ListName";
            sheet.Cells[1, 12].Value = "ListIds";

            // Populates reference columns with managed list data
            PopulateColumn(sheet, [.. listItem.Select(e => e.ListName ?? "")], 11);
            PopulateColumn(sheet, [.. listItem.Select(e => e.ListId.ToString())], 12);

            // Applies dropdown validation to ListName column
            ApplyDropdown(sheet, "ListNameRange", "A", 11, 100);

            // Adds formula to populate ListId based on ListName selection
            AddFormula(sheet, "C", "A", 11, 12, 100);

            // Auto-fits columns to content
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            // Returns the Excel package as a byte array
            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// Populates the specified column in the Excel sheet with the provided array of values.
        /// </summary>
        /// <param name="sheet">The Excel worksheet where the values will be populated.</param>
        /// <param name="values">The array of values to be inserted into the specified column.</param>
        /// <param name="columnIndex">The index of the column to populate.</param>
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
        /// </summary>
        /// <param name="sheet">The Excel worksheet where the dropdown will be applied.</param>
        /// <param name="rangeName">The name of the range containing the dropdown values.</param>
        /// <param name="column">The column to apply the dropdown validation to.</param>
        /// <param name="dataColumnIndex">The index of the data column that the dropdown references.</param>
        /// <param name="maxRows">The maximum number of rows to apply the dropdown to.</param>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int maxRows)
        {
            // Detects non-empty values in the data column
            int lastRow = sheet.Cells[sheet.Dimension.Start.Row, dataColumnIndex, sheet.Dimension.End.Row, dataColumnIndex]
                .Where(c => c.Value != null).Count();

            // Ensures at least one blank row is available for dropdowns
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
        /// Adds a VLOOKUP formula to the specified column in the Excel sheet.
        /// </summary>
        /// <param name="sheet">The Excel worksheet where the formula will be applied.</param>
        /// <param name="resultColumn">The column where the formula result will be inserted.</param>
        /// <param name="lookupColumn">The column to look up the values for the formula.</param>
        /// <param name="dataStartColumn">The start column of the data range for the VLOOKUP.</param>
        /// <param name="idColumn">The column containing the ID for the lookup.</param>
        /// <param name="dataCount">The number of rows to apply the formula to.</param>
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
    }
}
