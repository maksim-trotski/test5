using _5Elem.Client.Dialogs;
using _5Elem.Client.ViewModels;
using _5Elem.Shared.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _5Elem.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MouseLeftButtonDown += (s, e) => DragMove();
        }
    }
}