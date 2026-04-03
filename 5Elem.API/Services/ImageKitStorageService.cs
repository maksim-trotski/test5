using _5Elem.API.Services.Interfaces;
using Imagekit.Sdk;

namespace _5Elem.API.Services
{
    public class ImageKitStorageService : IStorageService
    {
        private readonly ImagekitClient _imagekit;
        private readonly string _urlEndpoint;
        private readonly ILogger<ImageKitStorageService> _logger;
        public ImageKitStorageService(IConfiguration configuration, ILogger<ImageKitStorageService> logger)
        {
            _logger = logger;
            _urlEndpoint = configuration["ImageKit:UrlEndpoint"];

            _imagekit = new ImagekitClient(
                configuration["ImageKit:PublicKey"],
                configuration["ImageKit:PrivateKey"],
                _urlEndpoint
            );
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var uploadRequest = new FileCreateRequest
                {
                    file = Convert.ToBase64String(fileBytes),
                    fileName = $"{Guid.NewGuid()}_{fileName}",
                    folder = "product_catalog",
                    useUniqueFileName = true,
                    isPrivateFile = false
                };

                var result = await _imagekit.UploadAsync(uploadRequest);

                if (result.HttpStatusCode == (int)System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Image uploaded: {result.url}");
                    return result.url;
                }

                throw new Exception($"Upload failed, status code: {result.HttpStatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to ImageKit");
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                var fileId = ExtractFileIdFromUrl(imageUrl);
                if (string.IsNullOrEmpty(fileId))
                    return false;

                var result = await _imagekit.DeleteFileAsync(fileId);
                return result.HttpStatusCode == (int)System.Net.HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from ImageKit");
                return false;
            }
        }

        public async Task<string> GetThumbnailUrlAsync(string imageUrl, int width = 150, int height = 150)
        {
            try
            {
                // ImageKit transformations через URL параметры
                var baseUrl = imageUrl.Split('?')[0];
                var thumbnailUrl = $"{baseUrl}?tr=w-{width},h-{height},c-at_max";
                return await Task.FromResult(thumbnailUrl);
            }
            catch
            {
                return imageUrl;
            }
        }

        private string ExtractFileIdFromUrl(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var segments = uri.Segments;
                // Последний сегмент - это fileId
                var fileId = segments[^1];
                // Убираем возможные query параметры
                return fileId.Split('?')[0];
            }
            catch
            {
                return null;
            }
        }
    }
}
