using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
