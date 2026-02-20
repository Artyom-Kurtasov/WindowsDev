using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WindowsDev.Settings;
using WindowsDev.ViewModels;

namespace WindowsDev
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider? ServiceProvider { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            var configure = new Configure();
            configure.ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();

            var mainVM = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            var authVM = ServiceProvider.GetRequiredService<AuthorizationViewModel>();

            mainVM.CurrentViewModel = authVM;

            mainWindow.DataContext = mainVM;
            mainWindow.Show();
        }


    }

}
