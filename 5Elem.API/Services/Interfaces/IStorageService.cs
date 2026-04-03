namespace _5Elem.API.Services.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType);
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<string> GetThumbnailUrlAsync(string imageUrl, int width = 150, int height = 150);
    }
}
