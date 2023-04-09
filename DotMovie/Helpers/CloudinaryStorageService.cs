using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DotMovie.Dtos;
using NuGet.Protocol;

namespace DotMovie.Helpers;

public class CloudinaryStorageService : IFileStorageService
{
    private readonly ILogger<CloudinaryStorageService> _logger;
    private readonly Cloudinary cloudinary;

    public CloudinaryStorageService(IConfiguration configuration, ILogger<CloudinaryStorageService> logger)
    {
        _logger = logger;
        var cloudinaryConfig = configuration.GetRequiredSection("Cloudinary").Get<Account>();
        cloudinary = new Cloudinary(cloudinaryConfig);

    }
    public async Task DeleteFile(string fileRoute,  FolderNames containerName)
    {
        var publicId = $"{containerName}/{Path.GetFileNameWithoutExtension(fileRoute)}";
        _logger.LogInformation("PublicId to be deleted {} ", publicId);
        var deletionParams = new DeletionParams(publicId){
            ResourceType = ResourceType.Image};
        await cloudinary.DestroyAsync(deletionParams);
    }

    public async Task<string> SaveFile( FolderNames containerName, IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var filePath  = Path.Combine(Path.GetTempPath(), fileName);
        _logger.LogInformation("FilePath {}", filePath);
        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
         
        }   
    
        // Upload
    
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(filePath),
            PublicId = $"{containerName}/{fileNameWithoutExtension}"

            // Path.Combine(containerName,fileNameWithoutExtension),
            
        };
        _logger.LogInformation("About to Upload");

        var uploadResult = await cloudinary.UploadAsync(uploadParams);
    _logger.LogInformation(uploadResult.ToJson());
       var imgUrl= cloudinary.Api.UrlImgUp.Transform(new Transformation().Width(300).Height(300).Crop("scale"))
            .BuildUrl(uploadResult.Url.AbsoluteUri);
       _logger.LogInformation("Uploaded Successfully {} ", imgUrl);
       
       File.Delete(filePath);
       return imgUrl;

    }

    public async Task<string> EditFile( FolderNames containerName, IFormFile file, string fileRoute)
    {
        await DeleteFile(fileRoute, containerName);
        return await SaveFile(containerName, file);
    }
}