using _5Elem.Client.Resources;
using _5Elem.Client.Services;
using _5Elem.Client.ViewModels.Base;
using _5Elem.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace _5Elem.Client.ViewModels
{
    public class CategoryDialogViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private CategoryCreateDto _category;
        private bool _isLoading;
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
                Title = StringConstants.EditCategoryTitle;
            }
            else
            {
                Title = StringConstants.AddCategoryTitle;
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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool CanSave => !IsLoading && !string.IsNullOrWhiteSpace(Category?.Name);

        public ICommand SelectImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void ExecuteSelectImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = StringConstants.ImageFileFilter,
                Title = StringConstants.SelectImageTitle
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
                ImageName = Path.GetFileName(_selectedImagePath);
            }
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

                    var fileStream = new FileStream(
                        _selectedImagePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        8192,
                        useAsync: true);

                        Category.ImageFile = new FormFile(
                            fileStream,
                            0,
                            fileStream.Length,
                            fileInfo.Name,
                            fileInfo.Name)
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
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var window = Application.Current.Windows
                            .OfType<Window>()
                            .FirstOrDefault(w => w.DataContext == this);
                        window?.Close();
                    });
                }
                else
                {
                    ErrorMessage = StringConstants.SaveError;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"{StringConstants.ErrorPrefix}{ex.Message}";
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