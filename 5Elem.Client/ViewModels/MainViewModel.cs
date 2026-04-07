using _5Elem.Client.Dialogs;
using _5Elem.Client.Resources;
using _5Elem.Client.Services;
using _5Elem.Client.ViewModels.Base;
using _5Elem.Client.Views;
using _5Elem.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace _5Elem.Client.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private readonly IServiceProvider _serviceProvider;
        private ObservableCollection<object> _displayItems;
        private CategoryDto _currentCategory;
        private bool _isLoading;
        private string _statusMessage;
        private System.Timers.Timer _refreshTimer;

        public MainViewModel(ApiService apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            _displayItems = new ObservableCollection<object>();

            NavigateToRootCommand = new RelayCommand(async _ => await ExecuteNavigateToRoot());

            AddCategoryCommand = new RelayCommand(async _ => await ExecuteAddCategory());
            CategoryClickCommand = new RelayCommand(async (param) => await ExecuteCategoryClick(param));
            AddProductCommand = new RelayCommand(async _ => await ExecuteAddProduct());

            RefreshCommand = new RelayCommand(async (param) => {
                if (param != null && param is ProductDto product)
                    await LoadProductsByCategoryAsync(product.CategoryId ?? -1);
                else
                    await LoadCategoriesAsync();
            });

            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            CloseCommand = new RelayCommand(_ => ExecuteClose());

            InitializeTimer();
            _ = LoadCategoriesAsync();
        }

        public ObservableCollection<object> DisplayItems
        {
            get => _displayItems;
            set => SetProperty(ref _displayItems, value);
        }

        public CategoryDto CurrentCategory
        {
            get => _currentCategory;
            set
            {
                SetProperty(ref _currentCategory, value);
                OnPropertyChanged(nameof(CurrentCategoryName));
            }
        }

        public string CurrentCategoryName => CurrentCategory?.Name ?? StringConstants.RootCategoryName;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string Username => App.Username;

        public ICommand NavigateToRootCommand { get; }
        public ICommand CategoryClickCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand CloseCommand { get; }

        private void InitializeTimer()
        {
            _refreshTimer = new System.Timers.Timer(5000);
            _refreshTimer.Elapsed += async (sender, e) =>
            {
                if (CurrentCategory == null)
                    await LoadCategoriesAsync();
                else
                    await LoadProductsByCategoryAsync(CurrentCategory.Id);
            };
            _refreshTimer.Start();
        }

        public async Task LoadCategoriesAsync()
        {
            IsLoading = true;
            try
            {
                var categories = await _apiService.GetCategoriesAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DisplayItems.Clear();
                    foreach (var category in categories)
                    {
                        var item = new CategoryItemViewModel(category, CategoryClickCommand, RefreshCommand, _apiService);
                        DisplayItems.Add(item);
                    }
                    CurrentCategory = null;
                    StatusMessage = string.Format(StringConstants.CategoriesStatus, categories.Count);
                });
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(StringConstants.ErrorPrefix, ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadProductsByCategoryAsync(int categoryId)
        {
            IsLoading = true;
            try
            {
                var products = await _apiService.GetProductsByCategoryAsync(categoryId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DisplayItems.Clear();
                    foreach (var product in products)
                    {
                        var item = new ProductItemViewModel(product, RefreshCommand, _apiService);
                        DisplayItems.Add(item);
                    }
                    StatusMessage = string.Format(StringConstants.ProductsStatus, products.Count);
                });
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(StringConstants.ErrorPrefix, ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task NavigateToCategoryAsync(CategoryDto category)
        {
            CurrentCategory = category;
            await LoadProductsByCategoryAsync(category.Id);
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            IsLoading = true;
            try
            {
                var success = await _apiService.DeleteCategoryAsync(categoryId);
                if (success)
                {
                    StatusMessage = StringConstants.DeleteCategorySuccess;
                    await LoadCategoriesAsync();
                }
                else
                {
                    MessageBox.Show(StringConstants.DeleteCategoryErrorHasProducts, StringConstants.DeleteCategoryErrorTitle,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(StringConstants.DeleteCategoryErrorMessage, ex.Message), StringConstants.DeleteCategoryErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteAddCategory()
        {
            var dialog = new CategoryDialog();
            var dialogViewModel = new CategoryDialogViewModel(_apiService);
            dialog.DataContext = dialogViewModel;

            if (dialog.ShowDialog() == true)
            {
                await LoadCategoriesAsync();
            }
        }

        private async Task ExecuteNavigateToRoot()
        {
            await LoadCategoriesAsync();
        }

        private async Task ExecuteCategoryClick(object parameter)
        {
            if (parameter is CategoryItemViewModel categoryItem)
            {
                var category = await _apiService.GetCategoryByIdAsync(categoryItem.Id);
                if (category != null)
                {
                    CurrentCategory = category;
                    await LoadProductsByCategoryAsync(category.Id);
                }
            }
        }

        private async Task ExecuteAddProduct()
        {
            var dialog = new ProductDialog();
            var dialogViewModel = new ProductDialogViewModel(_apiService, CurrentCategory?.Id);
            dialog.DataContext = dialogViewModel;

            if (dialog.ShowDialog() == true && dialogViewModel.Product != null)
            {
                if (CurrentCategory == null)
                    await LoadCategoriesAsync();
                else
                    await LoadProductsByCategoryAsync(CurrentCategory.Id);
            }
        }

        private void ExecuteLogout()
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();

            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w is MainWindow)?.Close();
        }

        private void ExecuteClose()
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();
            Application.Current.Shutdown();
        }
    }
}
