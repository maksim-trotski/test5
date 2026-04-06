using System.Windows;

namespace _5Elem.Client.Dialogs
{
    public partial class CategoryDialog : Window
    {
        public CategoryDialog()
        {
           InitializeComponent();
            this.MouseLeftButtonDown += (s, e) => this.DragMove();
        }
    }
}
