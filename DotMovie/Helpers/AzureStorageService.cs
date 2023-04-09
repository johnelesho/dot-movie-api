using Azure.Storage.Blobs;
using DotMovie.Dtos;

namespace DotMovie.Helpers;

public class AzureStorageService : IFileStorageService
{
    private string connectionString;

    public AzureStorageService(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("AzureStorageConnection");
        
    }

    public async Task DeleteFile(string fileRoute,  FolderNames containerName)
    {
        if (string.IsNullOrEmpty(fileRoute))
        {
            return;
        }

        var client = new BlobContainerClient(connectionString, containerName.ToString());
        await client.CreateIfNotExistsAsync();
        var fileName = Path.GetFileName(fileRoute);
        var blob = client.GetBlobClient(fileName);
        await blob.DeleteIfExistsAsync();
    }

    public async Task<string> EditFile( FolderNames containerName, IFormFile file, string fileRoute)
    {
        await DeleteFile(fileRoute, containerName);
        return await SaveFile(containerName, file);
    }

    public async Task<string> SaveFile( FolderNames containerName, IFormFile file)
    {
        var client = new BlobContainerClient(connectionString, containerName.ToString());
        await client.CreateIfNotExistsAsync();
         await client.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var blob = client.GetBlobClient(fileName);
        await blob.UploadAsync(file.OpenReadStream());
        return blob.Uri.ToString();
    }
}