using DiscordApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DiscordApp.Infrastructure.Services;

// Simple local file storage implementation
// In production, this could be swapped with S3-compatible storage
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _uploadPath = configuration["FileStorage:Path"] ?? "uploads";
        _baseUrl = configuration["FileStorage:BaseUrl"] ?? "/uploads";
        
        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(_uploadPath, uniqueFileName);

        // Save file
        using (var fileStreamOutput = File.Create(filePath))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        // Return URL
        return $"{_baseUrl}/{uniqueFileName}";
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_uploadPath, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<Stream> GetFileAsync(string fileUrl)
    {
        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_uploadPath, fileName);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", fileName);
        }

        return Task.FromResult<Stream>(File.OpenRead(filePath));
    }
}
