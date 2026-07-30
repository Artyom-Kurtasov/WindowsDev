using MahApps.Metro.Controls.Dialogs;
using Moq;
using WindowsDev.Application.DatabaseInterfaces;
using WindowsDev.Application.Services.Localization;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.Tests.ViewModels.Main.TestViewModels
{
    internal sealed class TestSettingsViewModel : SettingsViewModel
    {
        public TestSettingsViewModel()
            : base(
                Mock.Of<IDbHealthChecker>(),
                Mock.Of<IDialogCoordinator>(),
                Mock.Of<ILanguageChanger>(),
                Mock.Of<IDatabaseConfig>()
            )
        { }
    }
}
