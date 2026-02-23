using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Application.Services.Interface;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MEligibilityPlatform.Application.Services
{
    public class DriveService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IDriveService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IConfiguration _configuration = configuration;

        public async Task<ApiResponse<FileUploadResponse>> UploadAsync(IFormFile file, string token, CancellationToken ct = default)
        {
            var client = _httpClientFactory.CreateClient("MDrive");

            client.DefaultRequestHeaders.Remove("Authorization");
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            
            // Critical: Set the specific content type for the file part
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(fileContent, "file", file.FileName);

            var version = _configuration["MDrive:Version"] ?? "1";

            var response = await client.PostAsync($"api/v{version}/Files/Upload", content, ct);
            
            // Check status code first
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"MDrive Upload failed ({response.StatusCode}): {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<FileUploadResponse>>(cancellationToken: ct);
            
            // Check internal success flag
            if (result == null || !result.Succeeded)
            {
                var msg = result?.Messages != null ? string.Join(", ", result.Messages) : "Unknown error";
                throw new Exception($"MDrive Upload internal error: {msg}");
            }

            return result;
        }
        public async Task DeleteAsync(int fileId, string token, CancellationToken ct = default)
        {
            var client = _httpClientFactory.CreateClient("MDrive");
            client.DefaultRequestHeaders.Remove("Authorization");
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }

            var version = _configuration["MDrive:Version"] ?? "1";

            var response = await client.DeleteAsync($"api/v{version}/Files/{fileId}", ct);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"MDrive Delete returned {response.StatusCode}: {errorContent}");
            }
        }

        public async Task<(byte[] Bytes, string ContentType)> DownloadAsync(int fileId, string token, CancellationToken ct = default)
        {
            var client = _httpClientFactory.CreateClient("MDrive");
            
            client.DefaultRequestHeaders.Remove("Authorization");
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }

            var version = _configuration["MDrive:Version"] ?? "1";

            var response = await client.GetAsync($"api/v{version}/Files/Download/{fileId}", ct);
            
            if (!response.IsSuccessStatusCode)
            {
                // Try to parse the error message if it's an ApiResponse JSON
                string errorMsg;
                try 
                {
                    var errorObj = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
                    errorMsg = errorObj?.Messages != null ? string.Join(", ", errorObj.Messages) : "Unknown error";
                }
                catch
                {
                    errorMsg = await response.Content.ReadAsStringAsync(ct);
                }

                throw new HttpRequestException($"MDrive Download failed ({response.StatusCode}): {errorMsg}");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

            return (bytes, contentType);
        }
    }
}
