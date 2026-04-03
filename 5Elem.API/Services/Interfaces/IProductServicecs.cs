using _5Elem.Shared.Models;

namespace _5Elem.API.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(ProductCreateDto productDto);
        Task<ProductDto> UpdateAsync(int id, ProductUpdateDto productDto);
        Task<bool> DeleteAsync(int id);
    }
}
