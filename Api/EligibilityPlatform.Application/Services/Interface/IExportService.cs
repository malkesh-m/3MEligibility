using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Application.Services.Interface
{
    /// <summary>
    /// Interface for a generic export service.
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Exports a collection of data to an Excel stream.
        /// </summary>
        /// <typeparam name="T">The type of data to export.</typeparam>
        /// <param name="data">The data collection.</param>
        /// <param name="sheetName">The name of the worksheet.</param>
        /// <param name="ignoredProperties">Optional list of property names to exclude from the export.</param>
        /// <returns>A memory stream containing the Excel file.</returns>
        Task<Stream> ExportToExcel<T>(IEnumerable<T> data, string sheetName, string[]? ignoredProperties = null);
    }
}
