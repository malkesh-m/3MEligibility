//using EligibilityPlatform.Domain.Models;

//namespace EligibilityPlatform.Application.Services.Inteface
//{
//    /// <summary>
//    /// Service interface for entity management operations.
//    /// Provides methods for performing CRUD operations, import/export, and bulk actions on entities.
//    /// </summary>
//    public interface IEntityService
//    {
//        /// <summary>
//        /// Retrieves all entities.
//        /// </summary>
//        /// <returns>A list of <see cref="EntityModel"/> objects containing all entities.</returns>
//        List<EntityModel> GetAll();

//        /// <summary>
//        /// Retrieves a specific entity by its identifier.
//        /// </summary>
//        /// <param name="id">The unique identifier of the entity to retrieve.</param>
//        /// <returns>The <see cref="EntityModel"/> with the specified ID.</returns>
//        EntityModel GetById(int id);

//        /// <summary>
//        /// Adds a new entity.
//        /// </summary>
//        /// <param name="model">The <see cref="CreateOrUpdateEntityModel"/> containing the entity details to add.</param>
//        /// <returns>A task representing the asynchronous operation.</returns>
//        Task Add(CreateOrUpdateEntityModel model);

//        /// <summary>
//        /// Updates an existing entity.
//        /// </summary>
//        /// <param name="model">The <see cref="CreateOrUpdateEntityModel"/> containing the updated entity details.</param>
//        /// <returns>A task representing the asynchronous operation.</returns>
//        Task Update(CreateOrUpdateEntityModel model);

//        /// <summary>
//        /// Deletes an entity by its identifier.
//        /// </summary>
//        /// <param name="id">The unique identifier of the entity to delete.</param>
//        /// <returns>A task representing the asynchronous operation.</returns>
//        Task Delete(int id);

//        /// <summary>
//        /// Deletes multiple entities in a single operation.
//        /// </summary>
//        /// <param name="ids">A list of unique identifiers of the entities to delete.</param>
//        /// <returns>A task representing the asynchronous operation.</returns>
//        Task RemoveMultiple(List<int> ids);

//        /// <summary>
//        /// Imports entities from a file stream.
//        /// </summary>
//        /// <param name="fileStream">The stream containing the entity data to import.</param>
//        /// <param name="createdBy">The identifier of the user who initiated the import.</param>
//        /// <returns>A string containing the result or status of the import operation.</returns>
//        Task<string> ImportEntities(Stream fileStream, string createdBy);

//        /// <summary>
//        /// Exports entities to a stream for the selected entity IDs.
//        /// </summary>
//        /// <param name="ids">A list of entity IDs to export.</param>
//        /// <returns>A stream containing the exported entity data.</returns>
//        Task<Stream> ExportEntities(List<int> ids);

//        /// <summary>
//        /// Downloads a template file for entity import.
//        /// </summary>
//        /// <returns>A byte array containing the template file data.</returns>
//        Task<byte[]> DownloadTemplate();
//    }
//}
