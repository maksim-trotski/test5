using _5Elem.Client.Services;
using Microsoft.Extensions.Configuration;
using System.Windows;

namespace _5Elem.Client
{
    public partial class App : Application
    {
        public static ApiService ApiService { get; private set; }
        public static string Username { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();

            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

            ApiService = new ApiService(baseUrl);
        }
    }
}
