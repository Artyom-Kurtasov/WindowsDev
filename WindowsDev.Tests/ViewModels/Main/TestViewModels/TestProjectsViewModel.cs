using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.NavigationManager.Interfaces;
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
            ) { }
    }
}
