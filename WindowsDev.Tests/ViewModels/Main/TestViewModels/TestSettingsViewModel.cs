using MahApps.Metro.Controls.Dialogs;
using Moq;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.Tests.ViewModels.Main.TestViewModels
{
    internal sealed class TestSettingsViewModel : SettingsViewModel
    {
        public TestSettingsViewModel()
            : base(
                Mock.Of<IDbHealthChecker>(),
                Mock.Of<IDbManager>(),
                Mock.Of<IDialogCoordinator>(),
                Mock.Of<ILanguageChanger>()
            ) { }
    }
}
