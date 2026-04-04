using _5Elem.API.Services.Interfaces;
using _5Elem.Shared.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace _5Elem.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly string _connectionString;
        private readonly IStorageService _storageService;

        public CategoryService(IConfiguration configuration, IStorageService storageService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _storageService = storageService;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            const string sql = @"
                SELECT c.*, COUNT(p.Id) as ProductsCount
                FROM Categories c
                LEFT JOIN Products p ON p.CategoryId = c.Id
                GROUP BY c.Id, c.Name, c.Description, c.ImageUrl, c.CreatedAt
                ORDER BY c.Name";

            var categories = await connection.QueryAsync<CategoryDto>(sql);

            foreach (var category in categories)
            {
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    category.ThumbnailUrl = await _storageService.GetThumbnailUrlAsync(category.ImageUrl, 150, 150);
                }
            }

            return categories;
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            const string sql = @"
                SELECT c.*, COUNT(p.Id) as ProductsCount
                FROM Categories c
                LEFT JOIN Products p ON p.CategoryId = c.Id
                WHERE c.Id = @Id
                GROUP BY c.Id, c.Name, c.Description, c.ImageUrl, c.CreatedAt";

            var category = await connection.QueryFirstOrDefaultAsync<CategoryDto>(sql, new { Id = id });

            if (category != null && !string.IsNullOrEmpty(category.ImageUrl))
            {
                category.ThumbnailUrl = await _storageService.GetThumbnailUrlAsync(category.ImageUrl, 150, 150);
            }

            return category;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            using var connection = new SqlConnection(_connectionString);

            const string sql = @"
                SELECT p.*
                FROM Products p
                WHERE p.CategoryId = @CategoryId
                ORDER BY p.Name";

            var products = await connection.QueryAsync<ProductDto>(sql, new { CategoryId = categoryId });

            foreach (var product in products)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    product.ThumbnailUrl = await _storageService.GetThumbnailUrlAsync(product.ImageUrl, 200, 200);
                }
            }

            return products;
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto categoryDto)
        {
            string imageUrl = null;

            if (categoryDto.ImageFile != null && categoryDto.ImageFile.Length > 0)
            {
                using var stream = categoryDto.ImageFile.OpenReadStream();
                imageUrl = await _storageService.UploadImageAsync(
                    stream,
                    categoryDto.ImageFile.FileName,
                    categoryDto.ImageFile.ContentType);
            }

            using var connection = new SqlConnection(_connectionString);

            const string sql = @"
                INSERT INTO Categories (Name, Description, ImageUrl) 
                VALUES (@Name, @Description, @ImageUrl);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                categoryDto.Name,
                categoryDto.Description,
                ImageUrl = imageUrl
            });

            return await GetByIdAsync(id);
        }

        public async Task<CategoryDto> UpdateAsync(int id, CategoryUpdateDto categoryDto)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null)
                return null;

            string imageUrl = existing.ImageUrl;

            if (categoryDto.ImageFile != null && categoryDto.ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existing.ImageUrl))
                {
                    await _storageService.DeleteImageAsync(existing.ImageUrl);
                }

                using var stream = categoryDto.ImageFile.OpenReadStream();
                imageUrl = await _storageService.UploadImageAsync(
                    stream,
                    categoryDto.ImageFile.FileName,
                    categoryDto.ImageFile.ContentType);
            }

            using var connection = new SqlConnection(_connectionString);

            const string sql = @"
                UPDATE Categories 
                SET Name = @Name, 
                    Description = @Description, 
                    ImageUrl = @ImageUrl
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                Id = id,
                categoryDto.Name,
                categoryDto.Description,
                ImageUrl = imageUrl
            });

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            const string checkSql = "SELECT COUNT(1) FROM Products WHERE CategoryId = @Id";
            var productCount = await connection.ExecuteScalarAsync<int>(checkSql, new { Id = id });

            if (productCount > 0)
                return false;

            var category = await GetByIdAsync(id);
            if (category != null && !string.IsNullOrEmpty(category.ImageUrl))
            {
                await _storageService.DeleteImageAsync(category.ImageUrl);
            }

            const string deleteSql = "DELETE FROM Categories WHERE Id = @Id";
            var rows = await connection.ExecuteAsync(deleteSql, new { Id = id });

            return rows > 0;
        }
    }
}
