using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Application.Services.Localization;
using WindowsDev.Application.Services.ProjectService;
using WindowsDev.Services.Dialogs.Interfaces;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.Tests.ViewModels.Main.TestViewModels
{
    internal sealed class TestProjectsViewModel : ProjectsViewModel
    {
        public TestProjectsViewModel()
            : base(
                Mock.Of<IDialogCoordinator>(),
                Mock.Of<IProjectService>(),
                Mock.Of<INavigationService>(),
                Mock.Of<ILogger<ProjectsViewModel>>(),
                Mock.Of<IDialogService>(),
                Mock.Of<ILanguageChanger>()
            )
        { }
    }
}
