using _5Elem.API.Services;
using _5Elem.API.Services.Interfaces;
using _5Elem.Shared.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace _5Elem.API.Services
{
    public class ProductService : IProductService
    {
        private readonly string _connectionString;
        private readonly IStorageService _storageService;

        public ProductService(IConfiguration configuration, IStorageService storageService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _storageService = storageService;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            const string sql = @"
                SELECT p.*, c.Name as CategoryName 
                FROM Products p 
                LEFT JOIN Categories c ON p.CategoryId = c.Id 
                ORDER BY p.Id DESC";

            var products = await connection.QueryAsync<ProductDto>(sql);

            foreach (var product in products)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    product.ImageUrl = await _storageService.GetThumbnailUrlAsync(product.ImageUrl, 200, 200);
                }
            }

            return products;
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            const string sql = @"
                SELECT p.*, c.Name as CategoryName 
                FROM Products p 
                LEFT JOIN Categories c ON p.CategoryId = c.Id 
                WHERE p.Id = @Id";

            var product = await connection.QueryFirstOrDefaultAsync<ProductDto>(sql, new { Id = id });

            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                product.ImageUrl = await _storageService.GetThumbnailUrlAsync(product.ImageUrl, 200, 200);
            }

            return product;
        }

        public async Task<ProductDto> CreateAsync(ProductCreateDto productDto)
        {
            string imageUrl = null;

            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                using var stream = productDto.ImageFile.OpenReadStream();
                imageUrl = await _storageService.UploadImageAsync(
                    stream,
                    productDto.ImageFile.FileName,
                    productDto.ImageFile.ContentType);
            }

            using var connection = new SqlConnection(_connectionString);

            const string sql = @"
                INSERT INTO Products (Name, Description, Price, Stock, CategoryId, ImageUrl) 
                VALUES (@Name, @Description, @Price, @Stock, @CategoryId, @ImageUrl);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                productDto.Name,
                productDto.Description,
                productDto.Price,
                productDto.Stock,
                productDto.CategoryId,
                ImageUrl = imageUrl
            });

            return await GetByIdAsync(id);
        }

        public async Task<ProductDto> UpdateAsync(int id, ProductUpdateDto productDto)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null)
                return null;

            string imageUrl = existing.ImageUrl;

            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existing.ImageUrl))
                {
                    await _storageService.DeleteImageAsync(existing.ImageUrl);
                }

                using var stream = productDto.ImageFile.OpenReadStream();
                imageUrl = await _storageService.UploadImageAsync(
                    stream,
                    productDto.ImageFile.FileName,
                    productDto.ImageFile.ContentType);
            }

            using var connection = new SqlConnection(_connectionString);

            const string sql = @"
                UPDATE Products 
                SET Name = @Name, 
                    Description = @Description, 
                    Price = @Price, 
                    Stock = @Stock, 
                    CategoryId = @CategoryId,
                    ImageUrl = @ImageUrl
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                Id = id,
                productDto.Name,
                productDto.Description,
                productDto.Price,
                productDto.Stock,
                productDto.CategoryId,
                ImageUrl = imageUrl
            });

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product == null)
                return false;

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                await _storageService.DeleteImageAsync(product.ImageUrl);
            }

            using var connection = new SqlConnection(_connectionString);

            const string sql = "DELETE FROM Products WHERE Id = @Id";
            var rows = await connection.ExecuteAsync(sql, new { Id = id });

            return rows > 0;
        }
    }
}
