using _5Elem.Shared.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace _5Elem.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string _token;

        public ApiService(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var data = new { username, password };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("api/auth/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<AuthResponse>(await response.Content.ReadAsStringAsync());
                    SetToken(result.Token);
                    App.Username = result.Username;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            var data = new { username, email, password };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/register", content);
            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<AuthResponse>(await response.Content.ReadAsStringAsync());
                SetToken(result.Token);
                App.Username = result.Username;
                return true;
            }
            return false;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var response = await _httpClient.GetAsync("api/categories");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<CategoryDto>>(await response.Content.ReadAsStringAsync());
            }
            return new List<CategoryDto>();
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/categories/{id}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<CategoryDto>(await response.Content.ReadAsStringAsync());
            }
            return null;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto category)
        {
            using var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(category.Name), "Name");
            formData.Add(new StringContent(category.Description ?? ""), "Description");

            if (category.ImageFile != null)
            {
                var streamContent = new StreamContent(category.ImageFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(category.ImageFile.ContentType);
                formData.Add(streamContent, "ImageFile", category.ImageFile.FileName);
            }

            var response = await _httpClient.PostAsync("api/categories", formData);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<CategoryDto>(await response.Content.ReadAsStringAsync());
            }
            return null;
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, CategoryUpdateDto category)
        {
            using var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(category.Name), "Name");
            formData.Add(new StringContent(category.Description ?? ""), "Description");

            if (category.ImageFile != null)
            {
                var streamContent = new StreamContent(category.ImageFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(category.ImageFile.ContentType);
                formData.Add(streamContent, "ImageFile", category.ImageFile.FileName);
            }

            var response = await _httpClient.PutAsync($"api/categories/{id}", formData);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<CategoryDto>(await response.Content.ReadAsStringAsync());
            }
            return null;
        }

        public async Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var response = await _httpClient.GetAsync($"api/categories/{categoryId}/products");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<ProductDto>>(await response.Content.ReadAsStringAsync());
            }
            return new List<ProductDto>();
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/categories/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ProductDto>> GetProductsAsync()
        {
            var response = await _httpClient.GetAsync("api/products");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<ProductDto>>(await response.Content.ReadAsStringAsync());
            }
            return new List<ProductDto>();
        }

        public async Task<ProductDto> CreateProductAsync(ProductCreateDto product)
        {
            using var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(product.Name), "Name");
            formData.Add(new StringContent(product.Description), "Description");
            formData.Add(new StringContent(product.Price.ToString()), "Price");
            formData.Add(new StringContent(product.Stock.ToString()), "Stock");
            if (product.CategoryId.HasValue)
                formData.Add(new StringContent(product.CategoryId.Value.ToString()), "CategoryId");

            if (product.ImageFile != null)
            {
                var streamContent = new StreamContent(product.ImageFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(product.ImageFile.ContentType);
                formData.Add(streamContent, "ImageFile", product.ImageFile.FileName);
            }

            var response = await _httpClient.PostAsync("api/products", formData);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ProductDto>(await response.Content.ReadAsStringAsync());
            }
            return null;
        }

        public async Task<ProductDto> UpdateProductAsync(int id, ProductUpdateDto product)
        {
            using var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(product.Name), "Name");
            formData.Add(new StringContent(product.Description), "Description");
            formData.Add(new StringContent(product.Price.ToString()), "Price");
            formData.Add(new StringContent(product.Stock.ToString()), "Stock");
            if (product.CategoryId.HasValue)
                formData.Add(new StringContent(product.CategoryId.Value.ToString()), "CategoryId");

            if (product.ImageFile != null)
            {
                var streamContent = new StreamContent(product.ImageFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(product.ImageFile.ContentType);
                formData.Add(streamContent, "ImageFile", product.ImageFile.FileName);
            }

            var response = await _httpClient.PutAsync($"api/products/{id}", formData);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ProductDto>(await response.Content.ReadAsStringAsync());
            }
            return null;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/products/{id}");
            return response.IsSuccessStatusCode;
        }

        private class AuthResponse
        {
            public string Token { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
        }
    }
}
