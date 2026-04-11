using System.Windows.Input;

namespace WindowsDev.Infrastructure
{
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool>? canExecute = null)
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

        public async void Execute(object? parameter)
        {
            if (parameter is not T param)
                return;

            if (!CanExecute(param))
                return;

            await _execute(param);
        }

        /// <summary>
        /// Notifies the UI that the command execution state has changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

