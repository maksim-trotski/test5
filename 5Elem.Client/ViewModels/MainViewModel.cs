using _5Elem.Client.Dialogs;
using _5Elem.Client.Services;
using _5Elem.Client.ViewModels.Base;
using _5Elem.Client.Views;
using _5Elem.Shared.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace _5Elem.Client.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<object> _displayItems;
        private CategoryDto _currentCategory;
        private bool _isLoading;
        private string _statusMessage;
        private System.Timers.Timer _refreshTimer;

        public MainViewModel()
        {
            _apiService = App.ApiService;
            _displayItems = new ObservableCollection<object>();

            NavigateToRootCommand = new RelayCommand(_ => ExecuteNavigateToRoot());

            AddCategoryCommand = new RelayCommand(_ => ExecuteAddCategory());
            CategoryClickCommand = new RelayCommand(async (param) => await ExecuteCategoryClick(param));
            AddProductCommand = new RelayCommand(_ => ExecuteAddProduct());

            RefreshCommand = new RelayCommand(async (param) => {
                if (param != null && param is ProductDto product)
                    await LoadProductsByCategoryAsync(product.CategoryId ?? -1);
                else
                    await LoadCategoriesAsync();
            });

            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            CloseCommand = new RelayCommand(_ => ExecuteClose());

            InitializeTimer();
            LoadCategoriesAsync();
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

        public string CurrentCategoryName => CurrentCategory?.Name ?? "Каталог";

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
                    StatusMessage = $"Категории: {categories.Count}";
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
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
                    StatusMessage = $"Товаров в категории: {products.Count}";
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
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
                    StatusMessage = "Категория успешно удалена";
                    await LoadCategoriesAsync();
                }
                else
                {
                    MessageBox.Show("Нельзя удалить категорию, в которой есть товары", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteAddCategory()
        {
            var dialog = new CategoryDialog();
            var dialogViewModel = new CategoryDialogViewModel(_apiService);
            dialog.DataContext = dialogViewModel;

            if (dialog.ShowDialog() == true)
            {
                LoadCategoriesAsync();
            }
        }

        private void ExecuteNavigateToRoot()
        {
            LoadCategoriesAsync();
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

        private void ExecuteAddProduct()
        {
            var dialog = new ProductDialog();
            var dialogViewModel = new ProductDialogViewModel(_apiService, CurrentCategory?.Id);
            dialog.DataContext = dialogViewModel;

            if (dialog.ShowDialog() == true && dialogViewModel.Product != null)
            {
                if (CurrentCategory == null)
                    LoadCategoriesAsync();
                else
                    LoadProductsByCategoryAsync(CurrentCategory.Id);
            }
        }

        private void ExecuteLogout()
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();

            var loginWindow = new LoginWindow();
            var loginViewModel = new LoginViewModel();
            loginWindow.DataContext = loginViewModel;
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
