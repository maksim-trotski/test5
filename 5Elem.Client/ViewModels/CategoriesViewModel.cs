using _5Elem.Client.Dialogs;
using _5Elem.Client.Services;
using _5Elem.Client.ViewModels.Base;
using _5Elem.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace _5Elem.Client.ViewModels
{
    public class CategoriesViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<CategoryDto> _categories;
        private CategoryDto _selectedCategory;
        private bool _isLoading;
        private string _statusMessage;

        public CategoriesViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _categories = new ObservableCollection<CategoryDto>();

            AddCategoryCommand = new RelayCommand(_ => ExecuteAddCategory());
            EditCategoryCommand = new RelayCommand(async (param) => await ExecuteEditCategory(param));
            DeleteCategoryCommand = new RelayCommand(async _ => await ExecuteDeleteCategory());
            RefreshCommand = new RelayCommand(async _ => await LoadCategoriesAsync());
            CloseCommand = new RelayCommand(_ => ExecuteClose());

            LoadCategoriesAsync();
        }

        public ObservableCollection<CategoryDto> Categories
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

        public ICommand AddCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CloseCommand { get; }

        public async Task LoadCategoriesAsync()
        {
            IsLoading = true;
            try
            {
                var categories = await _apiService.GetCategoriesAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Categories.Clear();
                    foreach (var category in categories)
                    {
                        Categories.Add(category);
                    }
                    StatusMessage = $"Загружено категорий: {categories.Count}";
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
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

        private async Task ExecuteEditCategory(object parameter)
        {
            if (SelectedCategory == null) return;

            var dialog = new CategoryDialog();
            var dialogViewModel = new CategoryDialogViewModel(_apiService, SelectedCategory);
            dialog.DataContext = dialogViewModel;

            if (dialog.ShowDialog() == true)
            {
                LoadCategoriesAsync();
            }
        }

        private async Task ExecuteDeleteCategory()
        {
            if (SelectedCategory == null) return;

            var result = MessageBox.Show($"Удалить категорию '{SelectedCategory.Name}'?\n\n" +
                "Внимание: Категория будет удалена только если в ней нет товаров.",
                "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    var success = await _apiService.DeleteCategoryAsync(SelectedCategory.Id);
                    if (success)
                    {
                        await LoadCategoriesAsync();
                        StatusMessage = "Категория успешно удалена";
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
        }

        private void ExecuteClose()
        {
            var window = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }
    }
}
