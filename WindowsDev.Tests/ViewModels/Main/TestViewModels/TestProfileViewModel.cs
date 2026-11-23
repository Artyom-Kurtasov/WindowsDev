using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.ApiClient.ProfileClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.Tests.ViewModels.Main.TestViewModels;

internal class TestProfileViewModel : ProfileViewModel
{
    public TestProfileViewModel()
        : base(
            Mock.Of<ISecureTokenStorage>(),
            Mock.Of<IProfileApiClient>(),
            Mock.Of<IDialogService>(),
            Mock.Of<INavigationService>(),
            Mock.Of<ILogger<ProfileViewModel>>(),
            Mock.Of<ILanguageChanger>()
        )
    { }
}