using System;
using System.Collections.Generic;

namespace MEligibilityPlatform.Domain.Models
{
    /// <summary>
    /// standardized model for export requests across the application.
    /// </summary>
    public class ExportRequestModel
    {
        /// <summary>
        /// Optional list of specific record IDs to export.
        /// If provided, this takes precedence over filters.
        /// </summary>
        public List<int>? SelectedIds { get; set; }

        /// <summary>
        /// Optional general search term to filter records.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Optional Dictionary for field-specific filters.
        /// Key: Field name, Value: Filter value.
        /// </summary>
        public Dictionary<string, string>? Filters { get; set; }

        /// <summary>
        /// Gets whether any selection or filter is active.
        /// </summary>
        public bool HasSelection => SelectedIds != null && SelectedIds.Count > 0;
        
        /// <summary>
        /// Gets whether any filter (search or field-specific) is active.
        /// </summary>
        public bool HasFilter => !string.IsNullOrWhiteSpace(SearchTerm) || (Filters != null && Filters.Count > 0);
    }
}
