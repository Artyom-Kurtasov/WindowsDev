using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.NavigationManager.Interfaces;
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
                AppXamlLogs.LogDatabaseWarmUpFailed(logger, ex);

                var loc = ServiceProvider.GetRequiredService<ILanguageChanger>();
                MessageBox.Show(
                    loc.Translate(AppXamlErrors.DatabaseError),
                    loc.Translate(DialogTitles.CriticalError),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

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
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag))
            );
        }

        private void SubscribeToExceptionEvents()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnDispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e
        )
        {
            e.Handled = true;
            var exception = e.Exception;
            var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
            AppXamlLogs.LogDispatcherUnhandledException(logger, exception);
            ShowErrorDialog(e.Exception, isTerminating: false);
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
            AppXamlLogs.LogAppDomainUnhandledException(logger, exception);
            ShowErrorDialog(exception, e.IsTerminating);
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            var exception = e.Exception;
            var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
            AppXamlLogs.LogUnobservedTaskException(logger, exception);
            ShowErrorDialog(e.Exception, isTerminating: false);
        }

        private void ShowErrorDialog(Exception? ex, bool isTerminating)
        {
            var loc = ServiceProvider.GetRequiredService<ILanguageChanger>();

            if (!isTerminating)
            {
                MessageBox.Show(
                    loc.Translate(AppXamlErrors.GenericError),
                    loc.Translate(DialogTitles.Error),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                Shutdown();
            }
            else
            {
                MessageBox.Show(
                    loc.Translate(AppXamlErrors.CriticalError),
                    loc.Translate(DialogTitles.CriticalError),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
