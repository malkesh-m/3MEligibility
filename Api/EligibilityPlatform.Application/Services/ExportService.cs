using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MEligibilityPlatform.Application.Services.Interface;
using OfficeOpenXml;

namespace MEligibilityPlatform.Application.Services
{
    /// <summary>
    /// standardized implementation of the export service using EPPlus.
    /// </summary>
    public class ExportService : IExportService
    {
        public async Task<Stream> ExportToExcel<T>(IEnumerable<T> data, string sheetName, string[]? ignoredProperties = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            var type = typeof(T);
            var properties = type.GetProperties()
                .Where(p => ignoredProperties == null || !ignoredProperties.Contains(p.Name))
                .ToArray();

            // If display order is provided, honor it; otherwise keep reflection order.
            var ordered = properties
                .Select(p => new
                {
                    Property = p,
                    Display = p.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>()
                })
                .ToList();

            if (ordered.Any(x => x.Display?.GetOrder() != null))
            {
                properties = [.. ordered
                    .OrderBy(x => x.Display?.GetOrder() ?? int.MaxValue)
                    .ThenBy(x => x.Property.Name)
                    .Select(x => x.Property)];
            }

            // Add Headers
            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cells[1, col + 1].Value = properties[col].Name;
                worksheet.Cells[1, col + 1].Style.Font.Bold = true;
            }

            // Add Data
            var dataList = data.ToList();
            for (int row = 0; row < dataList.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(dataList[row]);
                }
            }

            worksheet.Cells.AutoFitColumns();

            var memoryStream = new MemoryStream();
            await package.SaveAsAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
