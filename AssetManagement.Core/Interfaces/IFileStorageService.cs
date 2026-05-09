using Microsoft.AspNetCore.Http;

namespace AssetManagement.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folder);
    Task<bool> DeleteFileAsync(string fileUrl);
    bool IsImageFile(string contentType);
}
