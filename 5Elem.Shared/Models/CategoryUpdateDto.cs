using Microsoft.AspNetCore.Http;

namespace _5Elem.Shared.Models
{
    public class CategoryUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
