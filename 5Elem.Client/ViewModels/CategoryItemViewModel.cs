using _5Elem.Client.Dialogs;
using _5Elem.Client.Resources;
using _5Elem.Client.Services;
using _5Elem.Client.ViewModels.Base;
using _5Elem.Shared.Models;
using System.Windows;
using System.Windows.Input;

namespace _5Elem.Client.ViewModels
{
    public class CategoryItemViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly CategoryDto _category;
        private readonly ICommand _categoryClickCommand;
        private readonly ICommand _refreshCategoriesCommand;
        private string _thumbnailUrl;

        public CategoryItemViewModel(CategoryDto category, ICommand categoryClickCommand, ICommand refreshCategoriesCommand, ApiService apiService)
        {
            _category = category;
            _categoryClickCommand = categoryClickCommand;
            _refreshCategoriesCommand = refreshCategoriesCommand;
            _apiService = apiService;
            _thumbnailUrl = category.ThumbnailUrl;

            EditCategoryCommand = new RelayCommand(async _ => await ExecuteEditCategory());
            DeleteCategoryCommand = new RelayCommand(async _ => await ExecuteDeleteCategory());
        }

        public int Id => _category.Id;
        public string Name => _category.Name;
        public string Description => _category.Description;
        public int ProductsCount => _category.ProductsCount;

        public string ThumbnailUrl
        {
            get => _thumbnailUrl;
            set => SetProperty(ref _thumbnailUrl, value);
        }

        public ICommand ClickCommand => _categoryClickCommand;
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        private async Task ExecuteEditCategory()
        {
            var dialog = new CategoryDialog();
            var dialogViewModel = new CategoryDialogViewModel(_apiService, _category);
            dialog.DataContext = dialogViewModel;

            if (dialog.ShowDialog() == true)
            {
                _refreshCategoriesCommand.Execute(null);
            }
        }

        private async Task ExecuteDeleteCategory()
        {
            var confirmationMessage = string.Format(StringConstants.DeleteCategoryConfirmation, _category.Name);

            var result = MessageBox.Show(
                confirmationMessage,
                StringConstants.DeleteCategoryTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _apiService.DeleteCategoryAsync(_category.Id);
                    if (success)
                    {
                        _refreshCategoriesCommand.Execute(null);
                        // StatusMessage = StringConstants.DeleteCategorySuccess;
                    }
                    else
                    {
                        MessageBox.Show(
                            StringConstants.DeleteCategoryErrorHasProducts,
                            StringConstants.DeleteCategoryErrorTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format(StringConstants.DeleteCategoryErrorMessage, ex.Message),
                        StringConstants.DeleteCategoryErrorTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
