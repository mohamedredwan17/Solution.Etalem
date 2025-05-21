using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Etalem.Infrastructure.Services
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string subfolder);
        Task DeleteFileAsync(string filePath);
    }
}