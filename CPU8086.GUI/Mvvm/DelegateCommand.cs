using System;
using System.Windows.Input;

// Kompatibilitaets-Shim fuer DevExpress.Mvvm.DelegateCommand / DelegateCommand<T>.
// Schlanke ICommand-Implementierung mit identischer API (RaiseCanExecuteChanged()).
// Bewusst keine RelayCommand<T>-Wrapper: dessen CanExecute-Semantik fuer Werttypen
// (z. B. StreamByte) bei null-Parametern weicht von DevExpress ab.
namespace DevExpress.Mvvm
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _execute();
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke(Cast(parameter)) ?? true;

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _execute(Cast(parameter));
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        private static T Cast(object parameter) => parameter is T value ? value : default;
    }
}
