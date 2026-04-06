using _5Elem.Client.ViewModels.Base;
using System.Windows.Input;

namespace _5Elem.Client.ViewModels
{
    //public class CustomMessageDialogViewModel : ViewModelBase
    //{
    //    public enum DialogResult
    //    {
    //        Yes,
    //        No,
    //        Ok
    //    }

    //    private string _message;
    //    private DialogResult _result = DialogResult.Ok;
    //    private bool _showYesNo;

    //    public CustomMessageDialogViewModel(string message, bool showYesNo = false)
    //    {
    //        _message = message;
    //        _showYesNo = showYesNo;

    //        YesCommand = new RelayCommand(_ => ExecuteYes());
    //        NoCommand = new RelayCommand(_ => ExecuteNo());
    //    }

    //    public string Message
    //    {
    //        get => _message;
    //        set => SetProperty(ref _message, value);
    //    }

    //    public bool ShowYesButton => _showYesNo;
    //    public bool ShowNoButton => _showYesNo;

    //    public DialogResult Result => _result;

    //    public ICommand YesCommand { get; }
    //    public ICommand NoCommand { get; }

    //    private void ExecuteYes()
    //    {
    //        _result = DialogResult.Yes;
    //        CloseWindow(true);
    //    }

    //    private void ExecuteNo()
    //    {
    //        _result = DialogResult.No;
    //        CloseWindow(false);
    //    }

    //    private void CloseWindow(bool result)
    //    {
    //        var window = System.Windows.Application.Current.Windows
    //            .OfType<System.Windows.Window>()
    //            .FirstOrDefault(w => w.DataContext == this);

    //        if (window != null)
    //        {
    //            window.DialogResult = result;
    //            window.Close();
    //        }
    //    }
    //}
}
