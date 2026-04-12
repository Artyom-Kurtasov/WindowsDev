using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WindowsDev.Business.DataBase;
using WindowsDev.Business.Services.Localization;
using WindowsDev.Commands.NavigationManager;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Settings;
using WindowsDev.ViewModels.Auth;
using WindowsDev.ViewModels.Main;

namespace WindowsDev
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider? ServiceProvider { get; private set; }
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            var configure = new Configure();
            configure.ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            await WarmUpTables();

            var language = ServiceProvider.GetRequiredService<LanguageChanger>();
            var navigationStore = ServiceProvider.GetRequiredService<NavigationStore>();
            var navigationService = ServiceProvider.GetRequiredService<INavigationService>();

            language.ChangeLanguage(UserSettings.Default.LanguageCode);

            await navigationService.NavigateTo<AuthorizationViewModel>();

            var main = ServiceProvider.GetRequiredService<MainWindow>();

            main.DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();

            main.Show();
        }

        private async Task WarmUpTables()
        {
            if (ServiceProvider != null)
            {
                var dbManager = ServiceProvider.GetRequiredService<DbManager>();

                var connectionString = UserSettings.Default.ConnectionString;
                await dbManager.SetConnection(connectionString);

                using var dbContext = dbManager.Create();

                dbContext.UsersInfo.Any();
                dbContext.ProjectsInfo.Any();
                dbContext.TasksInfo.Any();
            }
        }
    }

}


