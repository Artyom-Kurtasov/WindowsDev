namespace WindowsDev.Services.Dialogs;

internal class DialogContextProvider : IDialogContextProvider
{
    private object? _context;
    public object? Context
    {
        get => _context;
        set
        {
            if (_context != value) 
                _context = value;
        }
    }
}
