using _5Elem.Client.Services;
using _5Elem.Client.ViewModels;
using _5Elem.Client.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace _5Elem.Client
{
    public partial class App : Application
    {
        public static string Username { get; set; }

        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();

            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

            var services = new ServiceCollection();

            services.AddSingleton<ApiService>(sp =>
            {
                return new ApiService(baseUrl);
            });

            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<LoginWindow>();

            _serviceProvider = services.BuildServiceProvider();

            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            MainWindow = loginWindow;
            loginWindow.Show();
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
