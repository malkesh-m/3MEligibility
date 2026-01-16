//using EligibilityPlatform.Application.Attributes;
//using EligibilityPlatform.Application.Services.Inteface;
//using EligibilityPlatform.Domain.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace EligibilityPlatform.Controllers
//{
//    /// <summary>
//    /// API controller for managing entity operations.
//    /// </summary>
//    /// <remarks>
//    /// Initializes a new instance of the <see cref="EntityController"/> class.
//    /// </remarks>
//    /// <param name="entityService">The entity service.</param>
//    [Route("api/entity")]
//    [ApiController]
//    [Authorize]
//    public class EntityController(IEntityService entityService) : ControllerBase
//    {
//        private readonly IEntityService _entityService = entityService;

//        /// <summary>
//        /// Retrieves all entity records.
//        /// </summary>
//        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="EntityModel"/> objects.</returns>
//        /// 
//        [HttpGet("getall")]
//        public IActionResult Get()
//        {
//            /// <summary>
//            /// Retrieves all entities from the service and returns a success response.
//            /// </summary>
//            List<EntityModel> result = _entityService.GetAll();
//            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
//        }

//        /// <summary>
//        /// Retrieves all entity records (anonymous access allowed).
//        /// </summary>
//        /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="EntityModel"/> objects.</returns>
//        [HttpGet("getallentity"), AllowAnonymous]
//        public IActionResult GetAllEntity()
//        {
//            /// <summary>
//            /// Retrieves all entities from the service and returns a success response with anonymous access.
//            /// </summary>
//            List<EntityModel> result = _entityService.GetAll();
//            return Ok(new ResponseModel { IsSuccess = true, Data = result, Message = GlobalcConstants.Success });
//        }

//        /// <summary>
//        /// Retrieves an entity record by its unique identifier.
//        /// </summary>
//        /// <param name="id">The unique identifier of the entity.</param>
//        /// <returns>An <see cref="IActionResult"/> containing the <see cref="EntityModel"/> if found; otherwise, not found.</returns>
//        [RequireRole("View Entities Screen")]

//        [HttpGet("{id}")]
//        public IActionResult Get(int id)
//        {
//            /// <summary>
//            /// Retrieves a specific entity by ID and returns appropriate response based on existence.
//            /// </summary>
//            var result = _entityService.GetById(id);
//            if (result != null)
//            {
//                /// <summary>
//                /// Returns success response with the found entity data.
//                /// </summary>
//                return Ok(new ResponseModel { IsSuccess = false, Data = result, Message = GlobalcConstants.Success });
//            }
//            else
//            {
//                /// <summary>
//                /// Returns not found response when entity does not exist.
//                /// </summary>
//                return NotFound(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.NotFound });
//            }
//        }

//        /// <summary>
//        /// Adds a new entity record.
//        /// </summary>
//        /// <param name="model">The <see cref="CreateOrUpdateEntityModel"/> to add.</param>
//        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
//        /// 
//        [RequireRole("Add new Entity")]
//        //[AllowAnonymous]
//        [HttpPost]
//        public async Task<IActionResult> Post(CreateOrUpdateEntityModel model)
//        {
//            /// <summary>
//            /// Validates the model state before proceeding with entity creation.
//            /// </summary>
//            /// 
//            var userName = User.GetUserName();
//            model.CreatedBy = userName ?? "System";
//            model.UpdatedBy = userName??"System";
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }
//            /// <summary>
//            /// Adds the new entity and returns a success response.
//            /// </summary>
//            await _entityService.Add(model);
//            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Created });
//        }

//        /// <summary>
//        /// Updates an existing entity record.
//        /// </summary>
//        /// <param name="model">The <see cref="CreateOrUpdateEntityModel"/> to update.</param>
//        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
//        /// 
//        [RequireRole("Edit Entity")]

//        [HttpPut]
//        public async Task<IActionResult> Put(CreateOrUpdateEntityModel model)
//        {
//            /// <summary>
//            /// Validates the model state before proceeding with entity update.
//            /// </summary>
//            /// 
//            var userName = User.GetUserName();
//            model.UpdatedBy = userName;
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }
//            /// <summary>
//            /// Updates the entity and returns a success response.
//            /// </summary>
//            await _entityService.Update(model);
//            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Updated });
//        }

//        /// <summary>
//        /// Deletes an entity record by its unique identifier.
//        /// </summary>
//        /// <param name="id">The unique identifier of the entity to delete.</param>
//        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
//        /// 
//        [RequireRole("Delete Entity")]

//        [HttpDelete]
//        public async Task<IActionResult> Delete(int id)
//        {
//            /// <summary>
//            /// Deletes the specified entity and returns a success response.
//            /// </summary>
//            await _entityService.Delete(id);
//            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
//        }

//        /// <summary>
//        /// Imports entities from a file.
//        /// </summary>
//        /// <param name="file">The file to import.</param>
//        /// <param name="createdBy">The creator of the import.</param>
//        /// <returns>An <see cref="IActionResult"/> indicating the result of the import operation.</returns>
//        /// 
//        [RequireRole("Import Entity")]

//        [HttpPost("import")]
//        public async Task<IActionResult> ImportEntities(IFormFile file)
//        {

//            var userName = User.GetUserName();
//            /// <summary>
//            /// Validates that a file was uploaded before proceeding with import.
//            /// </summary>
//            if (file == null || file.Length == 0)
//                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No file uploaded." });
//            try
//            {
//                /// <summary>
//                /// Processes the file import and returns the result message.
//                /// </summary>
//                string resultMessage = await _entityService.ImportEntities(file.OpenReadStream(), userName ?? "");
//                return Ok(new ResponseModel { IsSuccess = true, Message = resultMessage });
//            }
//            catch (Exception ex)
//            {
//                /// <summary>
//                /// Handles exceptions during file import and returns an error response.
//                /// </summary>
//                return BadRequest(new ResponseModel { IsSuccess = false, Message = ex.Message });
//            }
//        }

//        /// <summary>
//        /// Exports selected entities.
//        /// </summary>
//        /// <param name="selectedEntityIds">The list of selected entity IDs to export.</param>
//        /// <returns>An <see cref="IActionResult"/> containing the exported file.</returns>
//        /// 
//        [RequireRole("Export Entity")]

//        [HttpPost("export")]
//        public async Task<IActionResult> ExportEntities([FromBody] List<int> selectedEntityIds)
//        {
//            /// <summary>
//            /// Exports the selected entities and returns the Excel file as a download.
//            /// </summary>
//            var stream = await _entityService.ExportEntities(selectedEntityIds);
//            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "entities.xlsx");
//        }

//        /// <summary>
//        /// Downloads the entity import template.
//        /// </summary>
//        /// <returns>An <see cref="IActionResult"/> containing the template file.</returns>
//        [HttpGet("download-template")]
//        public async Task<IActionResult> DownloadTemplate()
//        {
//            /// <summary>
//            /// Generates and returns the entity import template as an Excel file download.
//            /// </summary>
//            byte[] excelBytes = await _entityService.DownloadTemplate();
//            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template.xlsx");
//        }

//        /// <summary>
//        /// Deletes multiple entity records by their unique identifiers.
//        /// </summary>
//        /// <param name="ids">The list of unique identifiers of the entities to delete.</param>
//        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
//        /// 
//        [RequireRole("Delete Entity")]

//        [HttpDelete("multipledelete")]
//        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
//        {
//            /// <summary>
//            /// Validates that IDs were provided before proceeding with multiple deletion.
//            /// </summary>
//            if (ids == null || ids.Count == 0)
//            {
//                return BadRequest(new ResponseModel { IsSuccess = false, Message = "No IDs provided." });
//            }
//            /// <summary>
//            /// Deletes the multiple entities and returns a success response.
//            /// </summary>
//            await _entityService.RemoveMultiple(ids);
//            return Ok(new ResponseModel { IsSuccess = true, Message = GlobalcConstants.Deleted });
//        }
//    }
//}
