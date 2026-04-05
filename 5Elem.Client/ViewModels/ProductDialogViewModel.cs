using _5Elem.Client.Services;
using _5Elem.Client.ViewModels.Base;
using _5Elem.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;

namespace _5Elem.Client.ViewModels
{
    public class ProductDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ProductCreateDto _product;
        private string _selectedImagePath;
        private string _imageName;
        private string _errorMessage;
        private bool _isLoading;
        private List<CategoryDto> _categories;
        private CategoryDto _selectedCategory;

        public ProductDialogViewModel(ApiService apiService, int? defaultCategoryId = null)
        {
            _apiService = apiService;
            _product = new ProductCreateDto();
            _categories = new List<CategoryDto>();

            Title = "Добавление товара";

            if (defaultCategoryId.HasValue)
            {
                _product.CategoryId = defaultCategoryId;
            }

            SelectImageCommand = new RelayCommand(_ => ExecuteSelectImage());
            SaveCommand = new RelayCommand(async _ => await ExecuteSave(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());
            CloseCommand = new RelayCommand(_ => ExecuteCancel());

            LoadCategories();
        }

        public ProductDialogViewModel(ApiService apiService, ProductDto existingProduct)
        {
            _apiService = apiService;
            _product = new ProductCreateDto
            {
                Name = existingProduct.Name,
                Description = existingProduct.Description,
                Price = existingProduct.Price,
                Stock = existingProduct.Stock,
                CategoryId = existingProduct.CategoryId
            };
            _categories = new List<CategoryDto>();

            Title = "Редактирование товара";

            SelectImageCommand = new RelayCommand(_ => ExecuteSelectImage());
            SaveCommand = new RelayCommand(async _ => await ExecuteSave(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());
            CloseCommand = new RelayCommand(_ => ExecuteCancel());

            LoadCategories();
        }

        public ProductCreateDto Product
        {
            get => _product;
            set => SetProperty(ref _product, value);
        }

        public string Title { get; }

        public string ImageName
        {
            get => _imageName;
            set => SetProperty(ref _imageName, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public List<CategoryDto> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public CategoryDto SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                Product.CategoryId = value?.Id;
            }
        }

        public ICommand SelectImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CloseCommand { get; }

        private async void LoadCategories()
        {
            try
            {
                Categories = await _apiService.GetCategoriesAsync();
                if (Product.CategoryId.HasValue)
                {
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == Product.CategoryId.Value);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки категорий: {ex.Message}";
            }
        }

        private void ExecuteSelectImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Выберите изображение"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
                ImageName = System.IO.Path.GetFileName(_selectedImagePath);
            }
        }

        private bool CanSave()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(Product.Name) &&
                   Product.Price > 0 &&
                   Product.Stock >= 0;
        }

        private async Task ExecuteSave()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    var fileInfo = new FileInfo(_selectedImagePath);
                    var fileBytes = File.ReadAllBytes(_selectedImagePath);
                    var stream = new MemoryStream(fileBytes);

                    Product.ImageFile = new FormFile(stream, 0, fileBytes.Length,
                        fileInfo.Name, fileInfo.Name)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = GetContentType(fileInfo.Extension)
                    };
                }

                ProductDto result = null;

                if (Title.Contains("Редактирование"))
                {
                    // Обновление - нужно сохранить ID
                }
                else
                {
                    result = await _apiService.CreateProductAsync(Product);
                }

                if (result != null)
                {
                    var window = System.Windows.Application.Current.Windows
                        .OfType<System.Windows.Window>()
                        .FirstOrDefault(w => w.DataContext == this);

                    if (window != null)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
                else
                {
                    ErrorMessage = "Ошибка при сохранении товара";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteCancel()
        {
            var window = System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>()
                .FirstOrDefault(w => w.DataContext == this);

            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}
