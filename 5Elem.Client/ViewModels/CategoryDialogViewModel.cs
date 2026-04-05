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
    public class CategoryDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private CategoryCreateDto _category;
        private string _selectedImagePath;
        private string _imageName;
        private string _errorMessage;
        private int? _editId;

        public CategoryDialogViewModel(ApiService apiService, CategoryDto existingCategory = null)
        {
            _apiService = apiService;
            _category = new CategoryCreateDto();

            if (existingCategory != null)
            {
                _editId = existingCategory.Id;
                _category.Name = existingCategory.Name;
                _category.Description = existingCategory.Description;
                Title = "Редактирование категории";
            }
            else
            {
                Title = "Добавление категории";
            }

            SelectImageCommand = new RelayCommand(_ => ExecuteSelectImage());
            SaveCommand = new RelayCommand(_ => ExecuteSave(), _ => CanSave);
            CancelCommand = new RelayCommand(_ => ExecuteCancel());
        }

        public CategoryCreateDto Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
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
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool IsLoading { get; set; } = false;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool CanSave => !IsLoading && !string.IsNullOrWhiteSpace(Category?.Name);

        public ICommand SelectImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void ExecuteSelectImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Выберите изображение для категории"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
                ImageName = Path.GetFileName(_selectedImagePath);
            }
        }

        private async void ExecuteSave()
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

                    Category.ImageFile = new FormFile(stream, 0, fileBytes.Length,
                        fileInfo.Name, fileInfo.Name)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = GetContentType(fileInfo.Extension)
                    };
                }

                CategoryDto result = null;

                if (_editId.HasValue)
                {
                    var updateDto = new CategoryUpdateDto
                    {
                        Name = Category.Name,
                        Description = Category.Description,
                        ImageFile = Category.ImageFile
                    };
                    result = await _apiService.UpdateCategoryAsync(_editId.Value, updateDto);
                }
                else
                {
                    result = await _apiService.CreateCategoryAsync(Category);
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
                    ErrorMessage = "Ошибка при сохранении категории";
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
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}