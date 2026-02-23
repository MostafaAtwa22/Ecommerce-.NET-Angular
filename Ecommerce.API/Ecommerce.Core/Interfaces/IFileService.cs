using Microsoft.AspNetCore.Http;

namespace Ecommerce.Core.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        bool DeleteFile(string relativePath);
    }
}