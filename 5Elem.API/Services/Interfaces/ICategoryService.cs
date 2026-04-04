using _5Elem.Shared.Models;

namespace _5Elem.API.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
        Task<CategoryDto> CreateAsync(CategoryCreateDto categoryDto);
        Task<CategoryDto> UpdateAsync(int id, CategoryUpdateDto categoryDto);
        Task<bool> DeleteAsync(int id);
    }
}
