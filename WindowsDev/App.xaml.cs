using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Services.Localization;
using WindowsDev.Commands.NavigationManager;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Settings;
using WindowsDev.ViewModels.Auth;
using WindowsDev.ViewModels.Main;

namespace WindowsDev
{
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; } = null!;
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            var configure = new Configure();
            configure.ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            SubscribeToExceptionEvents();

            WarmUpTables();

            var language = ServiceProvider.GetRequiredService<LanguageChanger>();
            var navigationStore = ServiceProvider.GetRequiredService<NavigationStore>();
            var navigationService = ServiceProvider.GetRequiredService<INavigationService>();

            language.ChangeLanguage(UserSettings.Default.LanguageCode);

            await navigationService.NavigateTo<AuthorizationViewModel>();

            var main = ServiceProvider.GetRequiredService<MainWindow>();

            main.DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();

            SetCulture();

            main.Show();
        }

        private void WarmUpTables()
        {
            if (ServiceProvider != null)
            {
                try
                {
                    var dbHealthChecker = ServiceProvider.GetRequiredService<IDbHealthChecker>();
                    var dbManager = ServiceProvider.GetRequiredService<IDbManager>();

                    var connectionString = UserSettings.Default.ConnectionString;
                    dbManager.ConnectionString = connectionString;

                    dbHealthChecker.Check();
                }

                catch (Exception ex)
                {
                    var logger = ServiceProvider.GetRequiredService<ILogger<App>>();

                    logger.LogError($"{ex}");

                    var loc = ServiceProvider.GetRequiredService<LanguageChanger>();
                    MessageBox.Show(
                        loc.Translate("Error_DatabaseFatal"),
                        loc.Translate("Error_Critical"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);


                    Shutdown();
                }
            }
        }

        private void SetCulture()
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            CultureInfo.CurrentUICulture = new CultureInfo("ru-RU");

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        private void SubscribeToExceptionEvents()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ShowUserMessage(e.Exception, "UI Error");
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            ShowUserMessage(exception, "System Error", e.IsTerminating);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            ShowUserMessage(e.Exception, "Background Task Error");
        }

        private void ShowUserMessage(Exception ex, string source, bool isTerminating = false)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
            var loc = ServiceProvider.GetRequiredService<LanguageChanger>();
            logger.LogError(ex, source);

            if (!isTerminating)
            {
                MessageBox.Show(
                    loc.Translate("Error_Generic"),
                    loc.Translate("Error_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                Shutdown();
            }
            else
            {
                MessageBox.Show(
                    loc.Translate("Error_CriticalShutdown"),
                    loc.Translate("Error_Critical"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }

}


