using DockerHubBackend.Models;
using DockerHubBackend.Repository.Utils;

namespace DockerHubBackend.Services.Interface
{
    public interface IImageService 
    {
        Task<string> GetImageUrl(string fileName);
        Task UploadImage(string imageName, Stream fileStream);
        Task DeleteImage(string filePath);
    }
}
