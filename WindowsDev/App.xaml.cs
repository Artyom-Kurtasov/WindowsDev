using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WindowsDev.Commands.NavigationManager;
using WindowsDev.Commands.NavigationManager.Interfaces;
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

            var navigationStore = ServiceProvider.GetRequiredService<NavigationStore>();
            var navigationService = ServiceProvider.GetRequiredService<INavigationService>();

            navigationService.NavigateTo<AuthorizationViewModel>();

            var main = ServiceProvider.GetRequiredService<MainWindow>();

            main.DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();

            main.Show();
        }


    }

}
