using System.Windows.Input;

namespace WindowsDev.Infrastructure
{
    class AsyncRelayCommandWithParam<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool>? _canExecute;

        public AsyncRelayCommandWithParam(Func<T, Task> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T)parameter) ?? true;

        public async void Execute(object? parameter) => await _execute((T)parameter);

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
