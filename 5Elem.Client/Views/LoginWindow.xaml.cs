using _5Elem.Client.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace _5Elem.Client.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            MouseLeftButtonDown += (s, e) => this.DragMove();
            DataContext = new LoginViewModel();
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            var placeholder = passwordBox.Template.FindName("Placeholder", passwordBox) as TextBlock;

            if (placeholder != null)
            {
                placeholder.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            if (DataContext is LoginViewModel loginViewModel)
            {
                loginViewModel.LoginModel.Password = passwordBox.Password;
            }
        }

        private void ConfirmPasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            var placeholder = passwordBox.Template.FindName("Placeholder", passwordBox) as TextBlock;

            if (placeholder != null)
            {
                placeholder.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            if (DataContext is LoginViewModel loginViewModel)
            {
                loginViewModel.LoginModel.ConfirmPassword = passwordBox.Password;
            }
        }
    }
}
