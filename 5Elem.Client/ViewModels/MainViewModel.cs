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
        private ObservableCollection<ProductDto> _products;
        private ProductDto _selectedProduct;
        private bool _isLoading;
        private string _statusMessage;
        private System.Timers.Timer _refreshTimer;

        public MainViewModel()
        {
            _apiService = App.ApiService;
            _products = new ObservableCollection<ProductDto>();

            AddProductCommand = new RelayCommand(_ => ExecuteAddProduct());
            EditProductCommand = new RelayCommand(_ => ExecuteEditProduct(), _ => SelectedProduct != null);
            DeleteProductCommand = new RelayCommand(async _ => await ExecuteDeleteProduct(), _ => SelectedProduct != null);
            RefreshCommand = new RelayCommand(async _ => await LoadProductsAsync());
            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            CloseCommand = new RelayCommand(_ => ExecuteClose());

            InitializeTimer();
            LoadProductsAsync();
        }

        public ObservableCollection<ProductDto> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ProductDto SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                SetProperty(ref _selectedProduct, value);
                //(EditProductCommand as RelayCommand)?.RaiseCanExecuteChanged();
                //(DeleteProductCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

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
                await LoadProductsAsync();
            };
            _refreshTimer.Start();
        }

        public async Task LoadProductsAsync()
        {
            IsLoading = true;
            try
            {
                var products = await _apiService.GetProductsAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Products.Clear();
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }
                    StatusMessage = $"Загружено товаров: {products.Count}";
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = $"Ошибка загрузки: {ex.Message}";
                    MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteAddProduct()
        {
            var dialog = new ProductDialog();
            var dialogViewModel = new ProductDialogViewModel();
            dialog.DataContext = dialogViewModel;

            if (dialog.ShowDialog() == true && dialogViewModel.Product != null)
            {
                CreateProductAsync(dialogViewModel.Product);
            }
        }

        private async void CreateProductAsync(ProductCreateDto product)
        {
            IsLoading = true;
            try
            {
                var newProduct = await _apiService.CreateProductAsync(product);
                if (newProduct != null)
                {
                    await LoadProductsAsync();
                    StatusMessage = "Товар успешно добавлен";
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении товара", "Ошибка",
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
                IsLoading = false;
            }
        }

        private void ExecuteEditProduct()
        {
            if (SelectedProduct == null) return;

            var dialog = new ProductDialog();
            var dialogViewModel = new ProductDialogViewModel(SelectedProduct);
            dialog.DataContext = dialogViewModel;

            if (dialog.ShowDialog() == true && dialogViewModel.Product != null)
            {
                UpdateProductAsync(SelectedProduct.Id, dialogViewModel.Product);
            }
        }

        private async void UpdateProductAsync(int id, ProductCreateDto product)
        {
            IsLoading = true;
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
                    await LoadProductsAsync();
                    StatusMessage = "Товар успешно обновлен";
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
                IsLoading = false;
            }
        }

        private async Task ExecuteDeleteProduct()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show($"Удалить товар '{SelectedProduct.Name}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    var success = await _apiService.DeleteProductAsync(SelectedProduct.Id);
                    if (success)
                    {
                        await LoadProductsAsync();
                        StatusMessage = "Товар успешно удален";
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
                    IsLoading = false;
                }
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
