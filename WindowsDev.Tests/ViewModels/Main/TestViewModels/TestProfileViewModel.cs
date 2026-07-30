using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Application.Services.Localization;
using WindowsDev.Application.Services.Profile;
using WindowsDev.Application.Services.UserManager;
using WindowsDev.NavigationManager;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.Tests.ViewModels.Main.TestViewModels
{
    internal class TestProfileViewModel : ProfileViewModel
    {
        public TestProfileViewModel()
            : base(
                Mock.Of<ICurrentUserService>(),
                Mock.Of<IProfileService>(),
                Mock.Of<IDialogCoordinator>(),
                Mock.Of<INavigationService>(),
                Mock.Of<ILogger<ProfileViewModel>>(),
                Mock.Of<ILanguageChanger>()
            )
        { }
    }
}
