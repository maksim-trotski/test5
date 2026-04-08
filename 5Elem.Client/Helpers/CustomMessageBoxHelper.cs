using _5Elem.Client.Dialogs;
using _5Elem.Client.ViewModels;

namespace _5Elem.Client.Helpers
{
    public static class CustomMessageBoxHelper
    {
        public static bool? Show(string message, string title, bool showYesNo = false)
        {
            var viewModel = new CustomMessageDialogViewModel();
            viewModel.Initialize(message, title, showYesNo);

            var dialog = new CustomMessageDialog()
            {
                DataContext = viewModel
            };

            viewModel.RequestClose += result =>
            {
                dialog.DialogResult = result;
                dialog.Close();
            };

            return dialog.ShowDialog();
        }

        public static void ShowError(string message, string title)
        {
            Show(message, title, false);
        }

        public static void ShowWarning(string message, string title)
        {
            Show(message, title, false);
        }

        public static bool ShowConfirm(string message, string title)
        {
            return Show(message, title, true) == true;
        }
    }
}
