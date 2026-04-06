using _5Elem.Client.Dialogs;
using _5Elem.Client.Services;
using _5Elem.Client.ViewModels;
using _5Elem.Client.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Windows;

namespace _5Elem.Client
{
    public partial class App : Application
    {
        public static ApiService ApiService { get; private set; }
        public static string Username { get; set; }

        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            //ApiService = new ApiService("https://localhost:7000");
            ApiService = new ApiService("http://localhost:5000");

            try
            {
                var services = new ServiceCollection();

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();
                services.AddSingleton<IConfiguration>(configuration);

                services.AddSingleton<ApiService>(sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    var baseUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
                    return new ApiService(baseUrl);
                });

                services.AddSingleton<IServiceProvider>(sp => sp);
                //services.AddTransient<MainViewModel>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<CategoryDialogViewModel>();
                services.AddTransient<ProductDialogViewModel>();

                //services.AddTransient<MainWindow>();
                services.AddTransient<LoginWindow>();
                services.AddTransient<CategoryDialog>();
                services.AddTransient<ProductDialog>();

                _serviceProvider = services.BuildServiceProvider();

                var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                MainWindow = loginWindow;
                loginWindow.Show();

                //System.Diagnostics.Debug.WriteLine($"MainWindow: {MainWindow?.Title}");
                //System.Diagnostics.Debug.WriteLine($"Windows count: {Application.Current.Windows.Count}");
            }
            catch (Exception ex) {
                MessageBox.Show($"Ошибка при запуске: {ex.Message}\n\n{ex.StackTrace}");
                Shutdown();
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
