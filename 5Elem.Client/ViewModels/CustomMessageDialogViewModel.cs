using _5Elem.Client.ViewModels.Base;
using System.Windows.Input;

namespace _5Elem.Client.ViewModels
{
    public class CustomMessageDialogViewModel : ViewModelBase
    {
        public enum DialogResultType
        {
            Yes,
            No,
            Ok
        }

        private string _message;
        private string _title;
        private DialogResultType _result = DialogResultType.Ok;
        private bool _showYesNo;

        public event Action<bool?> RequestClose;

        public CustomMessageDialogViewModel()
        {
            YesCommand = new RelayCommand(_ => ExecuteYes());
            NoCommand = new RelayCommand(_ => ExecuteNo());
            OkCommand = new RelayCommand(_ => ExecuteOk());
        }

        public void Initialize(string message, string title, bool showYesNo)
        {
            Message = message;
            Title = title;
            _showYesNo = showYesNo;
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool ShowYesButton => _showYesNo;
        public bool ShowNoButton => _showYesNo;
        public bool ShowOkButton => !_showYesNo;

        public DialogResultType Result => _result;

        public ICommand YesCommand { get; }
        public ICommand NoCommand { get; }
        public ICommand OkCommand { get; }

        private void ExecuteYes()
        {
            _result = DialogResultType.Yes;
            RequestClose?.Invoke(true);
        }

        private void ExecuteNo()
        {
            _result = DialogResultType.No;
            RequestClose?.Invoke(false);
        }

        private void ExecuteOk()
        {
            _result = DialogResultType.Ok;
            RequestClose?.Invoke(true);
        }
    }
}
