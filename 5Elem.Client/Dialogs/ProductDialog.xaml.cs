using System.Windows;

namespace _5Elem.Client.Dialogs
{
    public partial class ProductDialog : Window
    {
        public ProductDialog()
        {
           InitializeComponent();
            this.MouseLeftButtonDown += (s, e) => this.DragMove();
        }
    }
}
