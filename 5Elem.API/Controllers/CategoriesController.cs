using _5Elem.API.Services.Interfaces;
using _5Elem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _5Elem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Категория не найдена" });
            return Ok(category);
        }

        [HttpGet("{id}/products")]
        public async Task<IActionResult> GetProductsByCategory(int id)
        {
            var products = await _categoryService.GetProductsByCategoryAsync(id);
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CategoryCreateDto categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.CreateAsync(categoryDto);
            if (category == null)
                return BadRequest(new { message = "Ошибка при создании категории" });

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] CategoryUpdateDto categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.UpdateAsync(id, categoryDto);
            if (category == null)
                return NotFound(new { message = "Категория не найдена" });

            return Ok(category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "Категория не найдена или содержит товары" });

            return NoContent();
        }
    }
}
