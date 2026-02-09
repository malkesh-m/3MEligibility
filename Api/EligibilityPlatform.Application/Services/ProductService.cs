using System.ComponentModel;
using System.Formats.Asn1;
using System.Globalization;
using AutoMapper;
using CsvHelper;
using MEligibilityPlatform.Application.UnitOfWork;
using MEligibilityPlatform.Domain.Entities;
using MEligibilityPlatform.Domain.Models;
using MEligibilityPlatform.Application.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// Service class for managing products.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </remarks>
    /// <param name="uow">The unit of work instance.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="entityService">The entity service instance.</param>
    /// <param name="categoryService">The category service instance.</param>
    public class ProductService(IUnitOfWork uow, IMapper mapper, ICategoryService categoryService, IWebHostEnvironment webHostEnvironment) : IProductService
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
        /// The category service instance for category-related operations.
        /// </summary>
        private readonly ICategoryService _categoryService = categoryService;

        /// <summary>
        /// The web host environment for accessing wwwroot path.
        /// </summary>
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

        /// <summary>
        /// Adds a new product to the database.
        /// </summary>
        /// <param name="model">The ProductAddUpdateModel containing the data to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Add(ProductAddUpdateModel model)
        {
            // Checks if a product with the same code already exists for the entity.
            var results = _uow.ProductRepository.Query()
                .Any(f => f.TenantId == model.TenantId && f.Code == model.Code);

            // Throws an exception if the code already exists.
            if (results)
            {
                throw new InvalidOperationException("Code is already exits");
            }
            var productName = _uow.ProductRepository.Query()
              .Any(f => f.TenantId == model.TenantId && f.ProductName == model.ProductName);

            // Throws an exception if the code already exists.
            if (productName)
            {
                throw new InvalidOperationException("Stream Name is already exits");
            }

            // Handle file upload if provided
            if (model.ProductImageFile != null)
            {
                model.ProductImagePath = await FileUploadHelper.SaveFileAsync(
                    model.ProductImageFile, 
                    model.TenantId, 
                    _webHostEnvironment.WebRootPath);
            }

            // Maps the model to a Product entity.
            var productEntities = _mapper.Map<Product>(model);

            // Sets CategoryId to null if it's 0, otherwise uses the provided value.
            productEntities.CategoryId = model.CategoryId == 0 ? (int?)null : model.CategoryId;

            // Sets the creation and update timestamps to current UTC time.
            productEntities.CreatedByDateTime = DateTime.UtcNow;
            productEntities.UpdatedByDateTime = DateTime.UtcNow;

            // Adds the product entity to the repository.
            _uow.ProductRepository.Add(productEntities);

            // Commits the changes to the database.
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Deletes a product by its entity ID and product ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="id">The product ID to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Delete(int tenantId, int id)
        {
            // Retrieves the product entity by ID and entity ID.
            var Item = _uow.ProductRepository.Query().First(f => f.ProductId == id && f.TenantId == tenantId);

            // Delete associated image file if it exists
            if (!string.IsNullOrWhiteSpace(Item.ProductImagePath))
            {
                FileUploadHelper.DeleteFile(Item.ProductImagePath, _webHostEnvironment.WebRootPath);
            }

            // Removes the product entity from the repository.
            _uow.ProductRepository.Remove(Item);

            // Commits the changes to the database.
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Gets all products for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of ProductListModel representing all products for the entity.</returns>
        public List<ProductListModel> GetAll(int tenantId)
        {
            // Retrieves products filtered by entity ID and includes category information.
            var products = _uow.ProductRepository.Query().Include(i => i.Category)
                .Where(f => f.TenantId == tenantId)
                .Select(s => new ProductListModel
                {
                    // Maps product properties to the list model.
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category == null ? null : s.Category.CategoryName,
                    Code = s.Code,
                    CreatedBy = s.CreatedBy,
                    CreatedByDateTime = s.CreatedByDateTime,
                    Description = s.Description,
                    TenantId = s.TenantId,
                    Narrative = s.Narrative,
                    ProductId = s.ProductId,
                    ProductImagePath = s.ProductImagePath,
                    ProductName = s.ProductName,
                    UpdatedBy = s.UpdatedBy,
                    UpdatedByDateTime = s.UpdatedByDateTime,
                    MaxEligibleAmount = (int?)s.MaxEligibleAmount
                }).ToList();

            // Returns the list of product models.
            return products;
        }

        /// <summary>
        /// Gets product IDs and names for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of ProductIdAndNameModel for the specified entity.</returns>
        public List<ProductIdAndNameModel> GetProductIAndName(int tenantId)
        {
            // Retrieves product IDs and names filtered by entity ID.
            var products = _uow.ProductRepository.Query()
                .Where(w => w.TenantId == tenantId)
                .Select(s => new ProductIdAndNameModel
                {
                    // Maps product ID, name, and image path to the model.
                    ProductId = s.ProductId,
                    ProductName = s.ProductName,
                    ProductImagePath = s.ProductImagePath
                })
                .ToList();

            // Returns the list of product ID and name models.
            return products;
        }

        /// <summary>
        /// Gets product names for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <returns>A list of ProductEligibleModel for the specified entity.</returns>
        public List<ProductEligibleModel> GetProductName(int tenantId)
        {
            // Retrieves product IDs and names filtered by entity ID.
            var products = _uow.ProductRepository.Query()
                .Where(w => w.TenantId == tenantId)
                .Select(s => new ProductEligibleModel
                {
                    // Maps product ID and name to the model.
                    ProductId = s.ProductId,
                    ProductName = s.ProductName,
                })
                .ToList();

            // Returns the list of product eligible models.
            return products;
        }

        /// <summary>
        /// Gets products by category for a specific entity and category ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="categoryId">The category ID.</param>
        /// <returns>A list of ProductModel for the specified entity and category ID.</returns>
        public List<ProductModel> GetProductsByCategory(int tenantId, int categoryId)
        {
            // Retrieves products filtered by entity ID and category ID.
            var products = _uow.ProductRepository.Query().Where(x => x.TenantId == tenantId && x.CategoryId == categoryId);

            // Maps the product entities to models and returns.
            return _mapper.Map<List<ProductModel>>(products);
        }

        /// <summary>
        /// Gets a product by its entity ID and product ID.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="id">The product ID to retrieve.</param>
        /// <returns>The ProductListModel for the specified entity and product ID.</returns>
        public ProductListModel GetById(int tenantId, int id)
        {
            // Retrieves products filtered by entity ID and product ID.
            var products = _uow.ProductRepository.Query().Where(w => w.ProductId == id && w.TenantId == tenantId).ToList();

            // Maps the product entity to a list model and returns.
            return _mapper.Map<ProductListModel>(products);
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="model">The ProductAddUpdateModel containing updated data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Update(ProductAddUpdateModel model)
        {
            // Checks if another product with the same code already exists for the entity.
            var results = _uow.ProductRepository.Query()
           .Any(f => f.TenantId == model.TenantId && f.Code == model.Code && model.ProductId != f.ProductId);

            // Throws an exception if the code already exists.
            if (results)
            {
                throw new InvalidOperationException("Code is already exits");
            }
            var productName = _uow.ProductRepository.Query()
           .Any(f => f.TenantId == model.TenantId && f.ProductName == model.ProductName && f.ProductId != model.ProductId);

            // Throws an exception if the code already exists.
            if (productName)
            {
                throw new InvalidOperationException("Stream Name is already exits");
            }
            // Sets CategoryId to null if it's 0, otherwise uses the provided value.
            model.CategoryId = model.CategoryId == 0 ? (int?)null : model.CategoryId;

            // Retrieves the existing product entity by ID and entity ID.
            var products = _uow.ProductRepository.Query().FirstOrDefault(w => w.ProductId == model.ProductId && w.TenantId == model.TenantId) ?? throw new InvalidOperationException("Product Does Not Exist");
            var createdBy = products.CreatedBy;
            var oldImagePath = products.ProductImagePath;

            // Handle file upload/removal from wwwroot
            if (model.ProductImageFile != null)
            {
                // Delete old file if it exists
                if (!string.IsNullOrWhiteSpace(oldImagePath))
                {
                    FileUploadHelper.DeleteFile(oldImagePath, _webHostEnvironment.WebRootPath);
                }

                // Save new file
                model.ProductImagePath = await FileUploadHelper.SaveFileAsync(
                    model.ProductImageFile,
                    model.TenantId,
                    _webHostEnvironment.WebRootPath);
            }
            else if (model.RemoveOldImage)
            {
                if (!string.IsNullOrWhiteSpace(oldImagePath))
                {
                    FileUploadHelper.DeleteFile(oldImagePath, _webHostEnvironment.WebRootPath);
                }

                model.ProductImagePath = null;
            }
            else
            {
                // Preserve existing image path if no new file is uploaded
                model.ProductImagePath = oldImagePath;
            }

            // Maps the updated model to the existing entity.
            var productEntities = _mapper.Map<ProductAddUpdateModel, Product>(model, products);
            products.CreatedBy = createdBy;

            // Sets the updated by user and timestamp.
            productEntities.UpdatedBy = model.UpdatedBy ?? "";
            productEntities.UpdatedByDateTime = DateTime.UtcNow;

            // Updates the product entity in the repository.
            _uow.ProductRepository.Update(productEntities);

            // Commits the changes to the database.
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Removes multiple products by their IDs for a specific entity.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="ids">A list of product IDs to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveMultiple(int tenantId, List<int> ids)
        {
            // Iterates through each product ID to remove.
            foreach (var id in ids)
            {
                // Retrieves the product entity by ID and entity ID.
                var products = _uow.ProductRepository.Query().First(w => w.ProductId == id && w.TenantId == tenantId);

                // Removes the product entity if it exists.
                if (products != null)
                {
                    // Delete associated image file if it exists
                    if (!string.IsNullOrWhiteSpace(products.ProductImagePath))
                    {
                        FileUploadHelper.DeleteFile(products.ProductImagePath, _webHostEnvironment.WebRootPath);
                    }

                    _uow.ProductRepository.Remove(products);
                }
            }

            // Commits the changes to the database.
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Exports product information to an Excel file.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="selectedProductIds">A list of selected product IDs to export.</param>
        /// <returns>A task that represents the asynchronous operation, with a stream containing the exported Excel file.</returns>
        public async Task<Stream> ExportInfo(int tenantId, List<int> selectedProductIds)
        {
            // Queries product information with joins to category and entity tables.
            var infos = from product in _uow.ProductRepository.Query()
                        join category in _uow.CategoryRepository.Query()
                        on product.CategoryId equals category.CategoryId
                        join entity in _uow.EntityRepository.Query()
                        on product.TenantId equals entity.EntityId into entityGroup
                        from entity in entityGroup.DefaultIfEmpty() // LEFT JOIN
                        where product.TenantId == tenantId && category.TenantId == tenantId
                        select new ProductDescription
                        {
                            // Maps product information to the description model.
                            Code = product.Code,
                            ProductId = product.ProductId,
                            ProductName = product.ProductName,
                            CategoryId = product.CategoryId,
                            CategoryName = category.CategoryName,
                            TenantId = product.TenantId,
                            EntityName = entity.EntityName ?? "",
                            ProductImagePath = product.ProductImagePath,
                            Narrative = product.Narrative,
                            Description = product.Description
                        };

            // Filters by selected product IDs if provided.
            if (selectedProductIds != null && selectedProductIds.Count > 0)
            {
                infos = infos.Where(query => selectedProductIds.Contains(query.ProductId));
            }

            // Executes the query and retrieves the results.
            var Info = await infos.ToListAsync();

            // Maps the entities to description models.
            var models = _mapper.Map<List<ProductDescription>>(Info);

            // Sets the EPPlus license context to non-commercial.
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Creates a new Excel package.
            using var package = new ExcelPackage();

            // Adds a worksheet named "Info" to the workbook.
            var worksheet = package.Workbook.Worksheets.Add("Info");

            // Gets the properties of the ProductDescription type.
            var properties = typeof(ProductDescription).GetProperties();

            // Adds headers to the worksheet.
            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
            }

            // Adds data rows to the worksheet.
            for (int row = 0; row < models.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(models[row]);
                }
            }

            // Auto-fits the columns to the content.
            worksheet.Cells.AutoFitColumns();

            // Creates a memory stream to hold the Excel file.
            var memoryStream = new MemoryStream();

            // Saves the Excel package to the memory stream.
            package.SaveAs(memoryStream);

            // Resets the memory stream position to the beginning.
            memoryStream.Position = 0;

            // Returns the memory stream containing the Excel file.
            return memoryStream;
        }

        /// <summary>
        /// Imports product entities from a CSV file into the database.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="fileStream">The file stream containing the CSV data to import.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ImportEntities(int tenantId, Stream fileStream)
        {
            // Creates a stream reader for the file stream.
            using var reader = new StreamReader(fileStream);
            // Creates a CSV reader with invariant culture settings.
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            // Reads all records from the CSV file.
            var models = csv.GetRecords<ProductModel>().ToList();

            // Processes each model from the CSV file.
            foreach (var model in models)
            {
                // Sets the entity ID for the model.
                model.TenantId = tenantId;

                // Maps the model to a product entity.
                var entity = _mapper.Map<Product>(model);

                // Adds the product entity to the repository.
                _uow.ProductRepository.Add(entity);
            }

            // Commits the changes to the database.
            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Imports product information from an Excel file into the database.
        /// </summary>
        /// <param name="tenantId">The entity ID.</param>
        /// <param name="fileStream">The file stream containing the Excel data to import.</param>
        /// <param name="createdBy">The user who is performing the import operation.</param>
        /// <returns>A task that represents the asynchronous operation, with a string message describing the result.</returns>
        public async Task<string> ImportInfo(int tenantId, Stream fileStream, string createdBy)
        {
            // Sets the EPPlus license context to non-commercial.
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Creates an Excel package from the file stream.
            using var package = new ExcelPackage(fileStream);

            // Gets the first worksheet from the workbook.
            var worksheet = package.Workbook.Worksheets[0];

            // Gets the number of rows with data in the worksheet.
            int rowCount = GetRowCount(worksheet);

            // Initializes a list to hold product models.
            var models = new List<Product>();

            // Initializes counters for skipped, duplicated, and inserted records.
            int skippedRecordsCount = 0;
            int dublicatedRecordsCount = 0;
            int insertedRecordsCount = 0;

            // Initializes the result message string.
            var resultMessage = "";

            // Initializes a list to track duplicate codes.
            List<string> duplicateCodes = [];

            try
            {
                // Returns a message if the uploaded file is empty.
                if (rowCount == 0 || rowCount == -1)
                {
                    return resultMessage = "Uploaded File Is Empty";
                }

                // Processes each row in the worksheet.
                for (int row = 2; row <= rowCount + 1; row++)
                {
                    // Reads cell values from the worksheet.
                    var Code = worksheet.Cells[row, 1].Text;
                    var ProductName = worksheet.Cells[row, 2].Text;
                    var CategoryId = worksheet.Cells[row, 4].Text;
                    var imageUrl = worksheet.Cells[row, 7].Text;
                    var Narrative = worksheet.Cells[row, 8].Text;
                    var Description = worksheet.Cells[row, 9].Text;

                    // Checks for missing or invalid data and skips the row if found.
                    if (string.IsNullOrWhiteSpace(Code) || !int.TryParse(CategoryId, out _) || string.IsNullOrWhiteSpace(ProductName) || string.IsNullOrWhiteSpace(imageUrl))
                    {
                        skippedRecordsCount++;
                        continue;
                    }

                    // Creates a new product model from the worksheet data.
                    var model = new Product
                    {
                        Code = Code,
                        CreatedBy = createdBy,
                        ProductName = ProductName,
                        CategoryId = int.Parse(CategoryId),
                        Narrative = Narrative,
                        TenantId = tenantId,
                        UpdatedBy = createdBy,
                        CreatedByDateTime = DateTime.UtcNow,
                        UpdatedByDateTime = DateTime.UtcNow,
                        IsImport = true
                    };
                    // Adds the model to the list.
                    models.Add(model);
                }

                // Returns a message if no new records were found to insert.
                if (skippedRecordsCount == rowCount)
                {
                    return "No new records to insert.";
                }

                // Processes each model for insertion.
                foreach (var model in models)
                {
                    // Checks if an identical product already exists.
                    var existingEntity = await _uow.ProductRepository.Query().AnyAsync(p => p.Code == model.Code || (p.ProductName == model.ProductName && p.CategoryId == model.CategoryId && p.TenantId == model.TenantId && p.TenantId == model.TenantId));

                    // Skips the model if it already exists.
                    if (existingEntity)
                    {
                        dublicatedRecordsCount++;
                        continue;
                    }

                    // Checks if the product code already exists.
                    var existingCode = await _uow.ProductRepository.Query()
                        .AnyAsync(p => p.Code == model.Code);

                    // Adds to duplicate codes list and skips if code exists.
                    if (existingCode)
                    {
                        duplicateCodes.Add(model.Code!); // Store duplicate codes
                        continue;
                    }
                    model.UpdatedBy = createdBy;
                    // Sets timestamps for the model.
                    model.UpdatedByDateTime = DateTime.UtcNow;
                    model.CreatedByDateTime = DateTime.UtcNow;
                    //model.UpdatedByDateTime = DateTime.UtcNow;

                    // Sets CategoryId to null if it's 0, otherwise uses the provided value.
                    model.CategoryId = model.CategoryId == 0 ? (int?)null : model.CategoryId;

                    // Adds the mapped product entity to the repository.
                    _uow.ProductRepository.Add(_mapper.Map<Product>(model));

                    // Increments the inserted records counter.
                    insertedRecordsCount++;
                }

                // Commits the changes to the database.
                await _uow.CompleteAsync();

                // Builds the result message based on the operation outcome.
                if (insertedRecordsCount > 0)
                {
                    resultMessage = $"{models.Count} Product Inserted Successfully.";
                }
                if (skippedRecordsCount > 0)
                {
                    resultMessage += $" {skippedRecordsCount} records were not inserted because of missing required field.";
                }
                if (dublicatedRecordsCount > 0)
                {
                    resultMessage += $" {dublicatedRecordsCount} record already exists.";
                }
                if (duplicateCodes.Count > 0)
                {
                    resultMessage += $" The following Codes already exist: {string.Join(", ", duplicateCodes)}.";
                }
            }
            catch (Exception ex)
            {
                // Returns an error message if an exception occurs.
                resultMessage = "Error On Info Page = " + ex.Message;
            }

            // Returns the result message.
            return resultMessage;
        }
        private static async Task<byte[]?> SafeLoadImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            try
            {
                return await ConvertImagePathToByteArrayAsync(imageUrl);
            }
            catch
            {
                // URL not valid / file missing / permission denied → return null
                return null;
            }
        }

        /// <summary>
        /// Converts an image URL or file path to a byte array.
        /// </summary>
        /// <param name="imageUrl">The image URL or file path.</param>
        /// <returns>A task that represents the asynchronous operation, with the image as a byte array.</returns>
        private static async Task<byte[]> ConvertImagePathToByteArrayAsync(string imageUrl)
        {
            // Handles HTTP/HTTPS URLs.
            if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
            {
                // Creates an HTTP client for downloading the image.
                using var httpClient = new HttpClient();
                // Downloads the image as a byte array from the URL.
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                // Returns the image byte array.
                return imageBytes;
            }
            // Handles file:// URLs.
            else if (imageUrl.StartsWith("file://"))
            {
                // Extracts the file path from the file:// URL.
                var filePath = imageUrl[7..]; // Remove "file://"

                // Checks if the file exists and reads it as a byte array.
                if (File.Exists(filePath))
                {
                    var imageBytes = await File.ReadAllBytesAsync(filePath);

                    // Returns the image byte array.
                    return imageBytes;
                }
                else
                {
                    // Throws an exception if the file is not found.
                    throw new FileNotFoundException("The specified file was not found.", filePath);
                }
            }
            else
            {
                // Throws an exception for invalid URL formats.
                throw new ArgumentException("Invalid URL format.");
            }
        }

        /// <summary>
        /// Gets the number of rows with data in the specified worksheet.
        /// </summary>
        /// <param name="worksheet">The worksheet to evaluate.</param>
        /// <returns>The number of rows with data.</returns>
        static int GetRowCount(ExcelWorksheet worksheet)
        {
            // Gets the last row in the worksheet dimension.
            int lastRow = worksheet.Dimension.End.Row;

            // Initializes row count and last non-empty row variables.
            //int rowCount = 0;
            int lastNonEmptyRow = 0;

            // Iterates through each row to find non-empty rows.
            for (int row = 2; row <= lastRow; row++)
            {
                // Checks if any of the first three cells in the row have data.
                bool hasData = !string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) ||
                               !string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text);

                // Updates the last non-empty row if data is found.
                if (hasData)
                {
                    lastNonEmptyRow = row;  // Track the last row that has data
                }
            }

            // Returns the last non-empty row number.
            return lastNonEmptyRow - 1;
        }

        /// <summary>
        /// Downloads an Excel template for product information.
        /// </summary>
        /// <param name="tenantId">The entity ID for which to generate the template.</param>
        /// <returns>A task that represents the asynchronous operation, with the Excel file as a byte array.</returns>
        public async Task<byte[]> DownloadTemplate(int tenantId)
        {
            // Fetches all entities from the entity service.
            //List<EntityModel> entities = _entityService.GetAll();

            // Fetches all categories for the specified entity from the category service.
            List<CategoryListModel> categories = _categoryService.GetAll(tenantId);

            // Sets the EPPlus license context to non-commercial.
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Creates a new Excel package.
            using var package = new ExcelPackage();

            // Adds a worksheet named "Info" to the workbook.
            var sheet = package.Workbook.Worksheets.Add("Info");

            // Defines the column headers for the template.
            string[] headers = ["Code*", "ProductName*", "Category*", "CategoryId*", "Entity*", "TenantId*", "ProductImage", "Narrative", "Description", "Field Description"];

            // Adds headers to the worksheet.
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }

            // Adds description for required fields.
            sheet.Cells[2, 10].Value = "* Fields marked with an asterisk are required.";

            // Applies bold formatting to the description text.
            sheet.Cells[2, 10].Style.Font.Bold = true;

            // Applies red color to the description text for visibility.
            sheet.Cells[2, 10].Style.Font.Color.SetColor(System.Drawing.Color.Red);

            // Sets the number format for the first column to text.
            sheet.Column(1).Style.Numberformat.Format = "@";

            // Adds additional headers for reference data.
            sheet.Cells[1, 13].Value = "EntityName";
            sheet.Cells[1, 14].Value = "TenantId";
            sheet.Cells[1, 16].Value = "CategoryName";
            sheet.Cells[1, 17].Value = "CategoryId";

            // Populates reference data columns with entity and category information.
            //PopulateColumn(sheet, [.. entities.Select(e => e.EntityName ?? "")], 13);
            //PopulateColumn(sheet, [.. entities.Select(e => e.TenantId.ToString())], 14);
            PopulateColumn(sheet, [.. categories.Select(e => e.CategoryName ?? "".ToString())], 16);
            PopulateColumn(sheet, [.. categories.Select(e => e.CategoryId.ToString())], 17);

            // Populates the product image column with sample data.
            PopulateImageColumn(sheet, 7);

            // Applies dropdown validation to entity and category columns.
            ApplyDropdown(sheet, "EntityNameRange", "E", 13, 100);
            ApplyDropdown(sheet, "CategoryNameRange", "C", 16, 100);

            // Adds formulas to automatically populate ID columns based on name selections.
            //AddFormula(sheet, "F", "E", 13, 14, entities.Count);
            //AddFormula(sheet, "D", "C", 16, 17, entities.Count);

            // Auto-fits the columns to the content.
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            // Returns the Excel package as a byte array.
            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// Populates a column in the worksheet with image data converted to Base64.
        /// </summary>
        /// <param name="sheet">The worksheet to populate.</param>
        /// <param name="columnIndex">The index of the column.</param>
        private static void PopulateImageColumn(ExcelWorksheet sheet, int columnIndex)
        {
            // Iterates through each row in the column.
            for (int i = 2; i <= sheet.Dimension.End.Row; i++)
            {
                // Gets the image URL from the cell.
                var imageUrl = sheet.Cells[i, columnIndex].Text;

                // Converts the image URL to Base64 and updates the cell value if URL exists.
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var base64Image = ConvertImageUrlToBase64(imageUrl);
                    sheet.Cells[i, columnIndex].Value = base64Image;
                }
            }
        }

        /// <summary>
        /// Converts an image URL to a Base64 string.
        /// </summary>
        /// <param name="imageUrl">The image URL to convert.</param>
        /// <returns>The Base64 representation of the image.</returns>
        private static string ConvertImageUrlToBase64(string imageUrl)
        {
            // Creates an HTTP client for downloading the image.
            using var httpClient = new HttpClient();
            // Downloads the image as a byte array.
            var imageBytes = httpClient.GetByteArrayAsync(imageUrl).Result;

            // Converts the byte array to a Base64 string.
            return Convert.ToBase64String(imageBytes);
        }

        /// <summary>
        /// Populates a column in the worksheet with values.
        /// </summary>
        /// <param name="sheet">The worksheet to populate.</param>
        /// <param name="values">The values to populate the column with.</param>
        /// <param name="columnIndex">The index of the column to populate.</param>
        private static void PopulateColumn(ExcelWorksheet sheet, string[] values, int columnIndex)
        {
            // Adds a blank value if no values are provided.
            if (values.Length == 0)
            {
                sheet.Cells[2, columnIndex].Value = "";
                return;
            }

            // Populates the column with the provided values.
            for (int i = 0; i < values.Length; i++)
            {
                sheet.Cells[i + 2, columnIndex].Value = values[i];
            }
        }

        /// <summary>
        /// Applies dropdown validation to a column in the worksheet.
        /// </summary>
        /// <param name="sheet">The worksheet to apply validation to.</param>
        /// <param name="rangeName">The name of the range for validation.</param>
        /// <param name="column">The column letter to apply validation to.</param>
        /// <param name="dataColumnIndex">The index of the column containing validation data.</param>
        /// <param name="maxRows">The maximum number of rows to apply validation to.</param>
        private static void ApplyDropdown(ExcelWorksheet sheet, string rangeName, string column, int dataColumnIndex, int maxRows)
        {
            // Detects non-empty values in the data column.
            int lastRow = sheet.Cells[sheet.Dimension.Start.Row, dataColumnIndex, sheet.Dimension.End.Row, dataColumnIndex]
                .Where(c => c.Value != null).Count();

            // Adds a blank value if no data is available.
            if (lastRow == 0)
            {
                sheet.Cells[2, dataColumnIndex].Value = ""; // Add a blank value
                lastRow = 2;
            }

            // Defines the range for validation data.
            var range = sheet.Cells[2, dataColumnIndex, lastRow, dataColumnIndex];

            // Adds the range as a named range in the workbook.
            sheet.Workbook.Names.Add(rangeName, range);

            // Applies list validation to each cell in the target column.
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
        /// Adds a VLOOKUP formula to a column in the worksheet.
        /// </summary>
        /// <param name="sheet">The worksheet to add the formula to.</param>
        /// <param name="resultColumn">The column letter where the formula result will be placed.</param>
        /// <param name="lookupColumn">The column letter containing the lookup value.</param>
        /// <param name="dataStartColumn">The starting column index of the lookup data.</param>
        /// <param name="idColumn">The column index containing the ID values in the lookup data.</param>
        /// <param name="dataCount">The number of data rows in the lookup range.</param>
        private static void AddFormula(ExcelWorksheet sheet, string resultColumn, string lookupColumn, int dataStartColumn, int idColumn, int dataCount)
        {
            // Defines the address range for the lookup data.
            string rangeAddress = sheet.Cells[2, dataStartColumn, dataCount + 1, idColumn].Address;

            // Adds the VLOOKUP formula to each cell in the result column.
            for (int row = 2; row <= 100; row++)
            {
                sheet.Cells[row, resultColumn[0] - 'A' + 1].Formula = $"IF({lookupColumn}{row}=\"\", \"\", VLOOKUP({lookupColumn}{row}, {rangeAddress}, 2, FALSE))";
            }
        }

        /// <summary>
        /// Gets the full file path for an image from its relative path.
        /// </summary>
        /// <param name="relativePath">The relative path to the image.</param>
        /// <returns>The full file path.</returns>
        public string GetImagePath(string relativePath)
        {
            return FileUploadHelper.GetFullPath(relativePath, _webHostEnvironment.WebRootPath);
        }
    }
}
