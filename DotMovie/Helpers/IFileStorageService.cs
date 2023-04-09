using DotMovie.Dtos;

namespace DotMovie.Helpers;

public interface IFileStorageService
{
    Task DeleteFile(string fileRoute, FolderNames containerName);
    Task<string> SaveFile( FolderNames containerName, IFormFile file);
    Task<string> EditFile( FolderNames containerName, IFormFile file, string fileRoute);
}