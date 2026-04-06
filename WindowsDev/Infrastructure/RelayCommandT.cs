using System.Windows.Input;

namespace WindowsDev.Infrastructure
{
    internal class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter is not T param)
                return false;

           return _canExecute?.Invoke(param) ?? true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not T param)
                return;

            if (!CanExecute(param))
                return;

            _execute(param);
        }

        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Notifies the UI that the execution status has changed.
        /// </summary>
        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

