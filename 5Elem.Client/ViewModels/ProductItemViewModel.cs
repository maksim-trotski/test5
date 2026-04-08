using _5Elem.Client.Dialogs;
using _5Elem.Client.Helpers;
using _5Elem.Client.Resources;
using _5Elem.Client.Services;
using _5Elem.Client.ViewModels.Base;
using _5Elem.Shared.Models;
using System.Windows.Input;

namespace _5Elem.Client.ViewModels
{
    public class ProductItemViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly ProductDto _product;
        private readonly ICommand _refreshCategoriesCommand;
        private string _thumbnailUrl;

        public ProductItemViewModel(ProductDto product, ICommand refreshCategoriesCommand, ApiService apiService)
        {
            _apiService = apiService;
            _product = product;
            _refreshCategoriesCommand = refreshCategoriesCommand;
            _thumbnailUrl = product.ThumbnailUrl;

            EditProductCommand = new RelayCommand(async _ => await ExecuteEditProduct());
            DeleteProductCommand = new RelayCommand(async _ => await ExecuteDeleteProduct());
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

        private async Task ExecuteEditProduct()
        {
            if (_product == null) return;

            var dialog = new ProductDialog();
            var dialogViewModel = new ProductDialogViewModel(_apiService, _product);
            dialog.DataContext = dialogViewModel;

            if (dialog.ShowDialog() == true && dialogViewModel.Product != null)
            {
                await UpdateProductAsync(_product.Id, dialogViewModel.Product);
            }
        }

        private async Task ExecuteDeleteProduct()
        {
            var result = CustomMessageBoxHelper.ShowConfirm(
                string.Format(StringConstants.DeleteProductConfirmation, _product.Name),
                StringConstants.ConfirmationTitle);

            if (result)
            {
                await DeleteProductAsync();
            }
        }

        public async Task DeleteProductAsync()
        {
            try
            {
                var success = await _apiService.DeleteProductAsync(_product.Id);
                if (success)
                {
                    if (_product.CategoryId == null)
                        _refreshCategoriesCommand.Execute(null);
                    else
                        _refreshCategoriesCommand.Execute(_product.CategoryId);
                }
                else
                {
                    CustomMessageBoxHelper.ShowError(
                        StringConstants.DeleteProductError,
                        StringConstants.DeleteProductErrorTitle);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxHelper.ShowError(
                    string.Format(StringConstants.DeleteProductError, ex.Message),
                    StringConstants.DeleteProductErrorTitle);
            }
        }

        private async Task UpdateProductAsync(int id, ProductCreateDto product)
        {
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
                }
                else
                {
                    CustomMessageBoxHelper.ShowError(
                        StringConstants.ProductSaveError,
                        StringConstants.DeleteProductErrorTitle);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxHelper.ShowError(
                    string.Format(StringConstants.ErrorPrefix, ex.Message),
                    StringConstants.DeleteProductErrorTitle);
            }
        }
    }
}
