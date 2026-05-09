using AssetManagement.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AssetManagement.Core.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".heic", ".webp" };

    public LocalFileStorageService(IConfiguration configuration)
    {
        _webRootPath = configuration["WebRootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        if (!IsImageFile(file.ContentType))
            throw new InvalidOperationException("Only image files are allowed (jpg, png, heic, webp).");

        if (file.Length > MaxFileSize)
            throw new InvalidOperationException("File size exceeds the 10 MB limit.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext)) ext = ".jpg";

        var uploadPath = Path.Combine(_webRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{folder}/{fileName}";
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_webRootPath, relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public bool IsImageFile(string contentType)
        => contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
}
