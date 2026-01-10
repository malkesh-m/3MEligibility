namespace EligibilityPlatform.Application.Repository
{
    /// <summary>
    /// Generic repository interface for managing entities of type T.
    /// Provides basic CRUD operations and query capabilities for all entity types.
    /// </summary>
    /// <typeparam name="T">The entity type that this repository manages.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves all entities of type T.
        /// </summary>
        /// <returns>An enumerable collection of all entities.</returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Retrieves all entities of type T with optional change tracking.
        /// </summary>
        /// <param name="asNoTracking">If true, entities will be retrieved without change tracking.</param>
        /// <returns>An enumerable collection of entities with specified tracking behavior.</returns>
        IEnumerable<T> GetAll(bool asNoTracking);

        /// <summary>
        /// Retrieves multiple entities by their identifiers.
        /// </summary>
        /// <param name="ids">The list of unique identifiers of the entities to retrieve.</param>
        /// <returns>A list of entities with the specified identifiers.</returns>
        List<T> GetByIds(IList<int> ids);

        /// <summary>
        /// Retrieves a single entity by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>The entity with the specified identifier, if found.</returns>
        T GetById(int id);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity with updated information.</param>
        void Update(T entity);

        /// <summary>
        /// Removes a single entity.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        void Remove(T entity);

        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        void Add(T entity, bool skipMakerChecker = false);

        /// <summary>
        /// Provides a queryable interface for building custom queries.
        /// </summary>
        /// <returns>An IQueryable instance for building LINQ queries.</returns>
        IQueryable<T> Query();

        /// <summary>
        /// Removes multiple entities in a single operation.
        /// </summary>
        /// <param name="entities">The collection of entities to remove.</param>
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>
        /// Adds multiple entities in a single operation.
        /// </summary>
        /// <param name="entities">The collection of entities to add.</param>
        void AddRange(IEnumerable<T> entities);
    }
}
