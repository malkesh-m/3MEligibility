using System.ComponentModel;
using System.Linq;
using MapsterMapper;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing categories.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CategoryService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="entityService">The entity service instance.</param>
    public class CategoryService(IUnitOfWork uow, IMapper mapper, IExportService exportService) : ICategoryService
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;
        //private readonly IEntityService _entityService = entityService;

        /// <summary>
        /// Retrieves all categories from the database for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>List of CategoryListModel objects.</returns>
        /// <summary>
        /// Retrieves all categories from the database for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>List of CategoryListModel objects.</returns>
        public async Task<List<CategoryListModel>> GetAll(int tenantId)
        {
            // Queries categories filtered by entity ID and converts to list
            var categories = await _uow.CategoryRepository.Query().Where(w => w.TenantId == tenantId).ToListAsync();
            // Maps category entities to CategoryListModel objects
            return _mapper.Map<List<CategoryListModel>>(categories);
        }

        /// <summary>
        /// Retrieves a category by its unique identifier for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="id">The category ID.</param>
        /// <returns>A CategoryModel object.</returns>
        public CategoryListModel GetById(int tenantId, int id)
        {
            // Finds the first category matching both entity ID and category ID
            var category = _uow.CategoryRepository.Query().First(f => f.TenantId == tenantId && f.CategoryId == id);
            // Maps the category entity to CategoryListModel object
            return _mapper.Map<CategoryListModel>(category);
        }

        /// <summary>
        /// Adds a new category to the database.
        /// </summary>
        /// <param name="category">The category model to add.</param>
        public async Task Add(CategoryCreateUpdateModel category)
        {
            var productName = _uow.CategoryRepository.Query()
         .Any(f => f.TenantId == category.TenantId && f.CategoryName == category.CategoryName);

            // Throws an exception if the code already exists.
            if (productName)
            {
                throw new InvalidOperationException("Product Name is already exits");
            }
            // Maps the input model to Category entity
            var models = _mapper.Map<Category>(category);
            // Sets the creation timestamp to current date and time
            models.CreatedByDateTime = DateTime.Now;
            // Sets the update timestamp to current date and time
            models.UpdatedByDateTime = DateTime.Now;
            // Adds the category entity to the repository
            _uow.CategoryRepository.Add(models);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Updates an existing category in the database.
        /// </summary>
        /// <param name="category">The updated category model.</param>
        public async Task Update(CategoryUpdateModel category)
        {
            var productName = _uow.CategoryRepository.Query()
    .Any(f => f.TenantId == category.TenantId && f.CategoryName == category.CategoryName && category.CategoryId != f.CategoryId);

            // Throws an exception if the code already exists.
            if (productName)
            {
                throw new InvalidOperationException("Product Name is already exits");
            }
            // Retrieves the existing category entity by ID
            var item = _uow.CategoryRepository.GetById(category.CategoryId);
            // Maps the updated model to the existing entity
            var models = _mapper.Map<CategoryUpdateModel, Category>(category, item);
            // Sets the update timestamp to current date and time
            models.UpdatedByDateTime = DateTime.Now;
            // Updates the category entity in the repository
            _uow.CategoryRepository.Update(models);
            // Commits the changes to the database
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Removes a category from the database by ID for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="id">The category ID.</param>
        public async Task<string> Remove(int tenantId, int id)
        {
            // Finds the category matching both entity ID and category ID
            var item = _uow.CategoryRepository.Query().First(f => f.TenantId == tenantId && f.CategoryId == id);
            var existingProduct = _uow.ProductRepository.Query().Any(p => p.CategoryId == id);
            string message = "";
            if (existingProduct)
            {
                message = "Cannot delete from Product because it is referenced by one or more Streams.";

            }
            else
            {
                // Removes the category entity from the repository
                _uow.CategoryRepository.Remove(item);
                // Commits the changes to the database
                await _uow.CompleteAsync();
                message = "Deleted Successfully";
            }
            return message;
        }

        /// <summary>
        /// Removes multiple categories from the database for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="ids">List of category IDs to remove.</param>
        public async Task<string> RemoveMultiple(int tenantId, List<int> ids)
        {
            var resultMessage = "";
            var notDeletedCategories = new HashSet<string>();  // names not deleted
            var deletedCategoryIds = new List<int>();          // ids deleted

            try
            {
                var products = _uow.ProductRepository.Query()
                                                     .Where(p => p.TenantId == tenantId)
                                                     .ToList();

                foreach (var id in ids)
                {
                    var category = _uow.CategoryRepository.Query()
                                    .FirstOrDefault(c => c.TenantId == tenantId && c.CategoryId == id);

                    if (category == null)
                        continue;

                    var isUsed = products.Any(p => p.CategoryId == id);

                    if (isUsed)
                    {
                        // Add to not- list
                        notDeletedCategories.Add(category.CategoryName);
                    }
                    else
                    {
                        // Safe to delete
                        _uow.CategoryRepository.Remove(category);
                        deletedCategoryIds.Add(id);
                    }
                }

                await _uow.CompleteAsync();

                if (notDeletedCategories.Count > 0)
                {
                    var msg = $"Cannot delete from Product because it is referenced by one or more Streams.: {string.Join(", ", notDeletedCategories)}.";
                    resultMessage += msg;
                }

                if (deletedCategoryIds.Count > 0)
                {
                    var msg = $"{deletedCategoryIds.Count} Product deleted successfully.";
                    resultMessage += " " + msg;
                }

                if (deletedCategoryIds.Count == 0 && notDeletedCategories.Count == 0)
                {
                    resultMessage = "No Products were deleted.";
                }
            }
            catch (Exception ex)
            {
                resultMessage = ex.Message;
            }

            return resultMessage;
        }


        private readonly IExportService _exportService = exportService;

        /// <summary>
        /// Exports categories based on standardized selection or filters.
        /// </summary>
        public async Task<Stream> ExportCategory(int tenantId, ExportRequestModel request)
        {
            var query = _uow.CategoryRepository.Query().Where(x => x.TenantId == tenantId);

            // Apply standardized Export logic: Selected -> Filtered -> All
            if (request.HasSelection)
            {
                query = query.Where(q => request.SelectedIds!.Contains(q.CategoryId));
            }
            else if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                string search = request.SearchTerm.ToLower();
                query = query.Where(q => 
                    (q.CategoryName != null && q.CategoryName.Contains(search)) ||
                    (q.CatDescription != null && q.CatDescription.Contains(search))
                );
            }

            var categorys = await query.ToListAsync();
            var data = _mapper.Map<List<CategoryCsvModel>>(categorys);
            
            return await _exportService.ExportToExcel(data, "Categories", ["EntityName", "TenantId"]);
        }

        /// Imports categories from an uploaded Excel file for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="fileStream">The file stream containing category data.</param>
        /// <param name="createdBy">User who performed the import.</param>
        /// <returns>A message indicating the result of the import.</returns>
        public async Task<string> ImportCategory(int tenantId, Stream fileStream, string createdBy)
        {
            // Sets the EPPlus license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new Excel package from the file stream
            using var package = new ExcelPackage(fileStream);
            // Gets the first worksheet from the workbook
            var worksheet = package.Workbook.Worksheets[0];
            // Gets the number of data rows in the worksheet
            int rowCount = GetRowCount(worksheet);
            // Initializes a list to store category models
            var models = new List<Category>();
            // Counter for skipped records
            int skippedRecordsCount = 0;
            // Counter for duplicate records
            int dublicatedRecordsCount = 0;
            // Counter for inserted records
            int insertedRecordsCount = 0;
            // Result message string
            var resultMessage = "";

            try
            {
                // Checks if the worksheet is empty
                if (rowCount == 0 || rowCount == -1)
                {
                    return resultMessage = "Uploaded File Is Empty";
                }
                // Reads data from the Excel file starting from row 2
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Gets category name from column 1
                    var CategoryName = worksheet.Cells[row, 1].Text;
                    // Gets category description from column 2
                    var CatDescription = worksheet.Cells[row, 2].Text;

                    // Validates required fields are present and valid
                    if (string.IsNullOrWhiteSpace(CategoryName))
                    {
                        // Increments skipped records count
                        skippedRecordsCount++;
                        // Skips to next record
                        continue;
                    }

                    // Creates a new category model from Excel data
                    var model = new Category
                    {
                        CategoryName = CategoryName,
                        CatDescription = CatDescription,
                        TenantId = tenantId,
                        CreatedBy = createdBy
                    };
                    // Adds the model to the list
                    models.Add(model);
                }

                // Checks if all records were skipped
                if (skippedRecordsCount == rowCount)
                {
                    return "No new records to insert.";
                }

                // Processes each model for insertion
                foreach (var model in models)
                {
                    // Checks if a category with same name and entity ID already exists
                    var existingEntity = await _uow.CategoryRepository.Query().AnyAsync(p => p.CategoryName == model.CategoryName && p.TenantId == model.TenantId);
                    if (existingEntity)
                    {
                        // Increments duplicate records count
                        dublicatedRecordsCount++;
                        // Skips to next model
                        continue;
                    }

                    // Sets update timestamp to current UTC time
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    // Sets creation timestamp to current UTC time
                    model.CreatedByDateTime = DateTime.UtcNow;
                    // Marks the record as imported
                    model.IsImport = true;
                    // Adds the mapped category entity to repository
                    _uow.CategoryRepository.Add(_mapper.Map<Category>(model));
                    // Increments inserted records count
                    insertedRecordsCount++;
                }

                // Commits the changes to the database
                await _uow.CompleteAsync();

                // Builds result message based on import results
                resultMessage = $"{insertedRecordsCount} {GlobalcConstants.Created} " +
                               $"{dublicatedRecordsCount} duplicates skipped, " +
                               $"{skippedRecordsCount} invalid rows skipped.";

            }
            catch (Exception ex)
            {
                // Handles any exceptions during import
                resultMessage = $"{GlobalcConstants.GeneralError} Error: {ex.Message}";
            }

            // Returns the result message
            return resultMessage;
        }

        /// <summary>
        /// Gets the number of data rows in the specified worksheet.
        /// </summary>
        /// <param name="worksheet">The Excel worksheet to check.</param>
        /// <returns>The number of data rows in the worksheet.</returns>
        private static int GetRowCount(ExcelWorksheet worksheet)
        {
            // Gets the last row number in the worksheet
            int lastRow = worksheet.Dimension.End.Row;
            // Initializes row count
            //int rowCount = 0;
            // Tracks the last non-empty row
            int lastNonEmptyRow = 0;

            // Iterates through rows starting from row 2
            for (int row = 2; row <= lastRow; row++)
            {
                // Checks if any of the first three columns have data
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text);

                if (hasData)
                {
                    // Updates the last non-empty row
                    lastNonEmptyRow = row;
                }
            }

            // Returns the last non-empty row number
            return lastNonEmptyRow - 1;
        }

        /// <summary>
        /// Generates an Excel template for category import.
        /// </summary>
        /// <returns>A byte array containing the template file.</returns>
        public async Task<byte[]> DownloadTemplate(int tenantId)
        {
            // Gets all entities from the entity service
            //List<EntityModel> entities = _entityService.GetAll();

            // Sets the EPPlus license context to non-commercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Creates a new Excel package
            using var package = new ExcelPackage();
            // Adds a new worksheet named "Category"
            var sheet = package.Workbook.Worksheets.Add("Category");

            // Defines column headers
            string[] headers = ["CategoryName*", "CatDescription", "Field Description"];
            // Sets headers in the first row
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }

            // Adds description for required fields in row 2, column 5
            sheet.Cells[2, 3].Value = "* Fields marked with an asterisk are required.";
            // Makes the text bold
            sheet.Cells[2, 3].Style.Font.Bold = true;
            // Sets the text color to red
            sheet.Cells[2, 3].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Hides column 4 (TenantId) from view
         

            // Auto-fits all columns to content
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            // Returns the Excel package as a byte array
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
            // Handles empty values array
            if (values.Length == 0)
            {
                // Ensures at least one blank entry
                sheet.Cells[2, columnIndex].Value = "";
                return;
            }

            // Populates the column with values starting from row 2
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
        /// <param name="maxRows">Maximum number of rows to apply the dropdown.</param>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int maxRows)
        {
            // Counts non-empty cells in the data column
            int lastRow = sheet.Cells[sheet.Dimension.Start.Row, dataColumnIndex, sheet.Dimension.End.Row, dataColumnIndex]
                .Where(c => c.Value != null).Count();

            // Ensures at least one value exists for dropdown
            if (lastRow == 0)
            {
                // Adds a blank value if no data exists
                sheet.Cells[2, dataColumnIndex].Value = "";
                lastRow = 2;
            }

            // Creates a range of cells with data for the dropdown
            var range = sheet.Cells[2, dataColumnIndex, lastRow, dataColumnIndex];
            // Creates a named range for the dropdown values
            sheet.Workbook.Names.Add(rangeName, range);

            // Applies dropdown validation to each row in the specified column
            for (int row = 2; row <= maxRows; row++)
            {
                // Creates a list validation for the cell
                var validation = sheet.DataValidations.AddListValidation($"{column}{row}");
                // Sets the formula to reference the named range
                validation.Formula.ExcelFormula = rangeName;
                // Enables error message display
                validation.ShowErrorMessage = true;
                // Sets the error style to stop (prevent invalid entries)
                validation.ErrorStyle = OfficeOpenXml.DataValidation.ExcelDataValidationWarningStyle.stop;
                // Sets the error title
                validation.ErrorTitle = "Invalid Selection";
                // Sets the error message
                validation.Error = "Please select a valid option.";
            }
        }

        /// <summary>
        /// Adds a formula to a specified column in the Excel worksheet.
        /// </summary>
        /// <param name="sheet">The Excel worksheet.</param>
        /// <param name="resultColumn">The column where the formula should be applied.</param>
        /// <param name="lookupColumn">The lookup column used in the formula.</param>
        /// <param name="dataStartColumn">The starting column for data.</param>
        /// <param name="idColumn">The column containing ID values.</param>
        /// <param name="dataCount">The number of data entries.</param>
        private static void AddFormula(ExcelWorksheet sheet, string resultColumn, string lookupColumn, int dataStartColumn, int idColumn, int dataCount)
        {
            // Creates the address range for the lookup data
            string rangeAddress = sheet.Cells[2, dataStartColumn, dataCount + 1, idColumn].Address;

            // Applies formula to each row in the result column
            for (int row = 2; row <= 100; row++)
            {
                // Sets a VLOOKUP formula to find entity ID based on entity name
                sheet.Cells[row, resultColumn[0] - 'A' + 1].Formula = $"IF({lookupColumn}{row}=\"\", \"\", VLOOKUP({lookupColumn}{row}, {rangeAddress}, 2, FALSE))";
            }
        }
    }
}
