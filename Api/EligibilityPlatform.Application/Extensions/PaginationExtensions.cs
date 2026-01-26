using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using MEligibilityPlatform.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MEligibilityPlatform.Application.Extensions
{
    /// <summary>
    /// Provides extension methods for pagination, sorting, and searching functionality.
    /// </summary>
    public static class PaginationExtensions
    {
        /// <summary>
        /// Filters a sequence of items based on a search term applied to a specified string property.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The collection of items to filter.</param>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="propertySelector">A function to select the property to search on.</param>
        /// <returns>A filtered <see cref="IQueryable{T}"/> containing only items that match the search term.</returns>
        public static IQueryable<T> Search<T>(this IQueryable<T> items, string searchTerm, Func<T, string> propertySelector)
        {
            // Returns original collection if search term is empty
            if (string.IsNullOrWhiteSpace(searchTerm))
                return items;

            // Converts search term to lowercase for case-insensitive search
            var lowerCaseSearchTerm = searchTerm.Trim().ToLower();

            // Filters items where selected property contains the search term
            return items.Where(item => propertySelector(item).Contains(lowerCaseSearchTerm, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Sorts a sequence of items dynamically based on a query string.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="items">The collection of items to sort.</param>
        /// <param name="orderByQueryString">The order by query string (e.g., "Name desc, Age asc").</param>
        /// <returns>A sorted <see cref="IQueryable{T}"/> based on the specified order string.</returns>
        public static IQueryable<T> Sort<T>(this IQueryable<T> items, string orderByQueryString)
        {
            // Returns default ordered collection if order string is empty
            if (string.IsNullOrWhiteSpace(orderByQueryString))
            {
                // Orders by Id property as default
                return items.OrderBy(e => EF.Property<object>(e!, "Id"));
            }

            // Splits order parameters by comma
            var orderParams = orderByQueryString.Trim().Split(',');
            // Gets all public instance properties of the type
            var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            // Creates string builder for order query construction
            var orderQueryBuilder = new StringBuilder();

            // Processes each order parameter
            foreach (var param in orderParams)
            {
                // Skips empty parameters
                if (string.IsNullOrWhiteSpace(param))
                    continue;

                // Extracts property name from parameter
                var propertyFromQueryName = param.Split(" ")[0];
                // Finds matching property info
                var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

                // Skips if property not found
                if (objectProperty == null)
                    continue;

                // Determines sort direction
                var direction = param.EndsWith(" desc") ? "descending" : "ascending";
                // Appends property and direction to order query
                orderQueryBuilder.Append($"{objectProperty.Name} {direction}, ");
            }

            // Removes trailing comma and space from order query
            var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
            // Returns default ordered collection if order query is empty
            if (string.IsNullOrWhiteSpace(orderQuery))
            {
                // Orders by Id property as default
                return items.OrderBy(e => EF.Property<object>(e!, "Id"));
            }

            // Applies dynamic ordering using the constructed query
            return items.OrderBy(orderQuery);
        }

        /// <summary>
        /// Applies pagination to a queryable sequence.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="query">The query to apply paging on.</param>
        /// <param name="param">The paging parameters (page number and page size).</param>
        /// <returns>
        /// A <see cref="PagedList{T}"/> containing the paged items and associated pagination metadata.
        /// </returns>
        public static PagedList<T> ApplyPaging<T>(this IQueryable<T> query, PagingParametersModel param)
        {
            // Gets total count of items in query
            var totalCount = query.Count();
            // Applies skip and take for pagination
            var items = query
                .Skip((param.PageNumber - 1) * param.PageSize)
                .Take(param.PageSize);

            // Creates pagination metadata
            var metaData = new MetaData
            {
                // Sets total count of items
                TotalCount = totalCount,
                // Sets page size from parameters
                PageSize = param.PageSize,
                // Sets current page number from parameters
                CurrentPage = param.PageNumber,
                // Calculates total pages
                TotalPages = (int)Math.Ceiling(totalCount / (double)param.PageSize)
            };

            // Returns paged list with items and metadata
            return new PagedList<T>
            {
                // Sets the paged items collection
                Items = items,
                // Sets the pagination metadata
                MetaData = metaData
            };
        }
    }

}
