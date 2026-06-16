using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Services.Localization;
using WindowsDev.Business.Services.Localization.Interfaces;
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

            ConfigureServices();
            SubscribeToExceptionEvents();

            if (!TryWarmUpDatabase())
                return;

            ApplySavedSettings();
            SetRussianCulture();
            await NavigateToAuthorizationAsync();

            ShowMainWindow();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            var configure = new Configure();
            configure.ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private bool TryWarmUpDatabase()
        {
            try
            {
                var dbHealthChecker = ServiceProvider.GetRequiredService<IDbHealthChecker>();
                var dbManager = ServiceProvider.GetRequiredService<IDbManager>();

                dbManager.ConnectionString = UserSettings.Default.ConnectionString;
                dbHealthChecker.Check();

                return true;
            }
            catch (Exception ex)
            {
                var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
                logger.LogError(ex, "Database warm-up failed");

                var loc = ServiceProvider.GetRequiredService<ILanguageChanger>();
                MessageBox.Show(
                    loc.Translate("Error_DatabaseFatal"),
                    loc.Translate("Error_Critical"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
                return false;
            }
        }

        private void ApplySavedSettings()
        {
            var language = ServiceProvider.GetRequiredService<ILanguageChanger>();
            language.ChangeLanguage(UserSettings.Default.LanguageCode);

            ThemeManager.Current.ChangeTheme(Current, UserSettings.Default.Theme);
        }

        private async Task NavigateToAuthorizationAsync()
        {
            var navigationService = ServiceProvider.GetRequiredService<INavigationService>();
            await navigationService.NavigateTo<AuthorizationViewModel>();
        }

        private void ShowMainWindow()
        {
            var main = ServiceProvider.GetRequiredService<MainWindow>();
            main.DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            main.Show();
        }

        private static void SetRussianCulture()
        {
            var culture = new CultureInfo("ru-RU");
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
        }

        private void SubscribeToExceptionEvents()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ShowErrorDialog(e.Exception, "UI Error", isTerminating: false);
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            ShowErrorDialog(exception, "System Error", e.IsTerminating);
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            ShowErrorDialog(e.Exception, "Background Task Error", isTerminating: false);
        }

        private void ShowErrorDialog(Exception? ex, string source, bool isTerminating)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
            var loc = ServiceProvider.GetRequiredService<ILanguageChanger>();

            logger.LogError(ex, source);

            if (!isTerminating)
            {
                MessageBox.Show(
                    loc.Translate("Error_Generic"),
                    loc.Translate("Error_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
            }
            else
            {
                MessageBox.Show(
                    loc.Translate("Error_CriticalShutdown"),
                    loc.Translate("Error_Critical"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}