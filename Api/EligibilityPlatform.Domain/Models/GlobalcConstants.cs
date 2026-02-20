namespace MEligibilityPlatform.Domain.Models
{
    public class GlobalcConstants
    {
        public const string Success = "Success";
        public const string Created = "Created successfully.";
        public const string CreateFailed = "Failed to create.";
        public const string Deleted = "Deleted successfully.";
        public const string DeletedFailed = "Failed to delete.";
        public const string Faild = "Sorry! operation failed.";
        public const string NotFound = "Record not found.";
        public const string Saved = "Record saved successfully.";
        public const string SaveFailed = "Failed to save.";
        public const string Updated = "Record updated successfully.";
        public const string UpdatedFailed = "Failed to update.";

        // Standardized Export Messages
        public const string ExportSuccess = "Export completed successfully.";
        public const string NoRecordsToExport = "No records found to export.";
        public const string SelectAtLeastOne = "Please select at least one record.";
        public static string ExportCountMessage(int count) => $"{count} record{(count == 1 ? "" : "s")} exported successfully.";

        // Standardized Import Messages
        public const string ImportSuccess = "Import completed successfully.";
        public const string ImportFailed = "Something went wrong during import. Please try again.";
        public const string InvalidFileFormat = "Invalid file format. Please upload a valid Excel file.";

        // General Messages
        public const string GeneralError = "Something went wrong. Please try again.";
    }


}
