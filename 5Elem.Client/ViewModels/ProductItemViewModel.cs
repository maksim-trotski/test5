using _5Elem.Client.Dialogs;
using _5Elem.Client.Services;
using _5Elem.Client.ViewModels.Base;
using _5Elem.Shared.Models;
using System.Data.Common;
using System.Windows;
using System.Windows.Input;

namespace _5Elem.Client.ViewModels
{
    public class ProductItemViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly ProductDto _product;
        ICommand _refreshCategoriesCommand;
        private string _thumbnailUrl;

        public ProductItemViewModel(ProductDto product, ICommand refreshCategoriesCommand, ApiService apiService)
        {
            _apiService = apiService;
            _product = product;
            _refreshCategoriesCommand = refreshCategoriesCommand;
            _thumbnailUrl = product.ThumbnailUrl;

            EditProductCommand = new RelayCommand(_ => ExecuteEditProduct());
            DeleteProductCommand = new RelayCommand(_ => ExecuteDeleteProduct());
        }

        public int Id => _product.Id;
        public string Name => _product.Name;
        public string Description => _product.Description;
        public decimal Price => _product.Price;
        public int Stock => _product.Stock;
        public int? CategoryId => _product.CategoryId;

        public string ThumbnailUrl
        {
            get => _thumbnailUrl;
            set => SetProperty(ref _thumbnailUrl, value);
        }

        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        private void ExecuteEditProduct()
        {
            if (_product == null) return;

            var dialog = new ProductDialog();
            var dialogViewModel = new ProductDialogViewModel(_apiService, _product);
            dialog.DataContext = dialogViewModel;

            if (dialog.ShowDialog() == true && dialogViewModel.Product != null)
            {
                UpdateProductAsync(_product.Id, dialogViewModel.Product);
            }
        }

        private async Task ExecuteDeleteProduct()
        {
            var result = MessageBox.Show($"Удалить товар '{_product.Name}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await DeleteProductAsync();
            }
        }

        public async Task DeleteProductAsync()
        {
            //IsLoading = true;
            try
            {
                var success = await _apiService.DeleteProductAsync(_product.Id);
                if (success)
                {
                    //StatusMessage = "Товар успешно удален";
                    if (_product.CategoryId == null)
                        _refreshCategoriesCommand.Execute(null);
                    else
                        _refreshCategoriesCommand.Execute(_product.CategoryId);
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении товара", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                //IsLoading = false;
            }
        }

        private async void UpdateProductAsync(int id, ProductCreateDto product)
        {
            //IsLoading = true;
            try
            {
                var updateDto = new ProductUpdateDto
                {
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    CategoryId = product.CategoryId,
                    ImageFile = product.ImageFile
                };

                var updatedProduct = await _apiService.UpdateProductAsync(id, updateDto);
                if (updatedProduct != null)
                {
                    _refreshCategoriesCommand.Execute(_product.CategoryId);
                    //StatusMessage = "Товар успешно обновлен";
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении товара", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                //IsLoading = false;
            }
        }
    }
}
