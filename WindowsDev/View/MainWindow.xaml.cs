using MahApps.Metro.Controls;
using WindowsDev.ViewModels;

namespace WindowsDev
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            _mainWindowViewModel = mainWindowViewModel;
            DataContext = _mainWindowViewModel;
        }
    }

}