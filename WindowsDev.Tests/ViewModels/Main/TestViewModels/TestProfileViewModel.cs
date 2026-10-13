using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Application.Users;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.Tests.ViewModels.Main.TestViewModels;

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