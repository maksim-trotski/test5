using _5Elem.Client.Services;
using System.Configuration;
using System.Data;
using System.Windows;

namespace _5Elem.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ApiService ApiService { get; private set; }
        public static string Username { get; set; }
        public static string AuthToken { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //ApiService = new ApiService("https://localhost:7000"); // Ваш API URL
            ApiService = new ApiService("http://localhost:5000");
        }
    }
}
