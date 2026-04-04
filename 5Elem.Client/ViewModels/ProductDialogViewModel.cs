using _5Elem.Client.Dialogs;
using _5Elem.Client.Models;
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

        public ProductDialogViewModel(ProductDto existingProduct = null)
        {
            _apiService = App.ApiService;
            _product = new ProductCreateDto();

            if (existingProduct != null)
            {
                _product.Name = existingProduct.Name;
                _product.Description = existingProduct.Description;
                _product.Price = existingProduct.Price;
                _product.Stock = existingProduct.Stock;
                _product.CategoryId = existingProduct.CategoryId;
                Title = "Редактирование товара";
            }
            else
            {
                Title = "Добавление товара";
            }

            SelectImageCommand = new RelayCommand(_ => ExecuteSelectImage());
            SaveCommand = new RelayCommand(_ => ExecuteSave(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());
            CloseCommand = new RelayCommand(_ => ExecuteCancel());
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

        public ICommand SelectImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CloseCommand { get; }

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
            return !string.IsNullOrWhiteSpace(Product.Name) && Product.Price > 0 && Product.Stock >= 0;
        }

        private async Task ExecuteSave()
        {
            try
            {
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

                var result = await _apiService.CreateProductAsync(Product);

                if (result != null)
                {
                    System.Windows.Application.Current.Windows.OfType<ProductDialog>().FirstOrDefault()?.Close();
                }
                else
                {
                    Console.WriteLine("Ошибка при создании товара");

                }
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.ToString());
            }

            
        }

        private void ExecuteCancel()
        {
            Product = null;
            System.Windows.Application.Current.Windows.OfType<ProductDialog>().FirstOrDefault()?.Close();
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
