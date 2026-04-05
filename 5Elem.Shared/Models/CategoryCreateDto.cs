using Microsoft.AspNetCore.Http;

namespace _5Elem.Shared.Models
{
    public class CategoryCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
