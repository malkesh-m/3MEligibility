using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEligibilityPlatform.Domain.Models
{
    public class FileUploadResponse
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public string Path { get; set; } = default!;

        public string ContentType { get; set; } = default!;

        public string Extension { get; set; } = default!;

        public byte[]? FileBytes { get; set; }

        public string Name { get; set; } = default!;
    }
}
