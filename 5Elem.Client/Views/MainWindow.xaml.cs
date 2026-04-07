using _5Elem.Client.ViewModels;
using System.Windows;

namespace _5Elem.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            MouseLeftButtonDown += (s, e) => DragMove();
        }
    }
}