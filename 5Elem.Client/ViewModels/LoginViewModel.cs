using _5Elem.Client.Models;
using _5Elem.Client.Resources;
using _5Elem.Client.Services;
using _5Elem.Client.ViewModels.Base;
using System.Windows;
using System.Windows.Input;

namespace _5Elem.Client.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private LoginModel _loginModel;
        private bool _isLoginMode;
        private string _errorMessage;

        public LoginViewModel()
        {
            _apiService = App.ApiService;
            _loginModel = new LoginModel();
            _isLoginMode = true;

            LoginCommand = new RelayCommand(async _ => await ExecuteLoginAsync());
            RegisterCommand = new RelayCommand(async _ => await ExecuteRegisterAsync());
            SwitchModeCommand = new RelayCommand(_ => ExecuteSwitchMode());
            CloseCommand = new RelayCommand(_ => ExecuteClose());
        }

        public LoginModel LoginModel
        {
            get => _loginModel;
            set => SetProperty(ref _loginModel, value);
        }

        public bool IsLoginMode
        {
            get => _isLoginMode;
            set => SetProperty(ref _isLoginMode, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand SwitchModeCommand { get; }
        public ICommand CloseCommand { get; }

        private async Task ExecuteLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(LoginModel.Username) ||
                string.IsNullOrWhiteSpace(LoginModel.Password))
            {
                ErrorMessage = StringConstants.LoginEmptyFields;
                return;
            }

            try
            {
                var success = await _apiService.LoginAsync(LoginModel.Username, LoginModel.Password);

                if (success)
                {
                    var mainWindow = new MainWindow();
                    var mainViewModel = new MainViewModel();
                    mainWindow.DataContext = mainViewModel;
                    mainWindow.Show();

                    Application.Current.Windows[0]?.Close();
                }
                else
                {
                    ErrorMessage = StringConstants.LoginInvalidCredentials;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format(StringConstants.LoginGenericError, ex.Message);
            }
        }

        private async Task ExecuteRegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(LoginModel.Username) ||
                string.IsNullOrWhiteSpace(LoginModel.Email) ||
                string.IsNullOrWhiteSpace(LoginModel.Password))
            {
                ErrorMessage = StringConstants.RegisterEmptyFields;
                return;
            }

            if (LoginModel.Password != LoginModel.ConfirmPassword)
            {
                ErrorMessage = StringConstants.RegisterPasswordsDoNotMatch;
                return;
            }

            try
            {
                var success = await _apiService.RegisterAsync(
                    LoginModel.Username,
                    LoginModel.Email,
                    LoginModel.Password);

                if (success)
                {
                    var mainWindow = new MainWindow();
                    var mainViewModel = new MainViewModel();
                    mainWindow.DataContext = mainViewModel;
                    mainWindow.Show();

                    Application.Current.Windows[0]?.Close();
                }
                else
                {
                    ErrorMessage = StringConstants.RegisterUserAlreadyExists;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format(StringConstants.LoginGenericError, ex.Message);
            }
        }

        private void ExecuteSwitchMode()
        {
            IsLoginMode = !IsLoginMode;
            ErrorMessage = string.Empty;
            LoginModel.Password = string.Empty;
            LoginModel.ConfirmPassword = string.Empty;
        }

        private void ExecuteClose()
        {
            Application.Current.Shutdown();
        }
    }
}
