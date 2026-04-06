using _5Elem.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace _5Elem.Client.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginViewModel viewModel)
        {
            try
            {
                InitializeComponent();
                DataContext = viewModel;
                MouseLeftButtonDown += (s, e) => this.DragMove();
            }
            catch(Exception ex)
            {
                var a = 1;
            }
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (DataContext is LoginViewModel loginViewModel)
            {
                loginViewModel.LoginModel.Password = passwordBox.Password;
            }
        }

        private void ConfirmPasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (DataContext is LoginViewModel loginViewModel)
            {
                loginViewModel.LoginModel.ConfirmPassword = passwordBox.Password;
            }
        }
    }
}
