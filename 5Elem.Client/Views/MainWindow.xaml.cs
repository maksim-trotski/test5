using System.Windows;

namespace _5Elem.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
           InitializeComponent();
            this.MouseLeftButtonDown += (s, e) => this.DragMove();
        }
    }
}