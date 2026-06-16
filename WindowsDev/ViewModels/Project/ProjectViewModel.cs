using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.Business.Services.TaskService;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Tasks;
using WindowsDev.ViewModels.Tasks.Dialog;
using WindowsDev.Views.Tasks;
using TaskStatus = WindowsDev.Domain.TasksModels.Enums.TaskStatus;

namespace WindowsDev.ViewModels.Projects
{
    public class ProjectViewModel : ViewModelBase, IRefreshableViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly ITaskService _taskService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ILogger<ProjectViewModel> _logger;

        private readonly int _pageSize = 15;

        public ProjectViewModel(ProjectsInfo currentProject,
            IDialogCoordinator dialogCoordinator,
            INavigationService navigationService,
            ITaskService taskService,
            IDialogService dialogService,
            ILogger<ProjectViewModel> logger)
        {
            CurrentProject = currentProject;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _taskService = taskService;
            _dialogCoordinator = dialogCoordinator;
            _logger = logger;

            SwitchToMainViewCommand = new RelayCommand(SwitchToMainView, CanSwitchToMainView);
            OpenDialogCommand = new AsyncRelayCommand(OpenDialog);
            OpenTaskCommand = new AsyncRelayCommandT<TasksInfo>(OpenTask);
            NextPageCommand = new AsyncRelayCommand(NextPage);
            PrevPageCommand = new AsyncRelayCommand(PrevPage);
            DeleteSelectedTasksCommand = new AsyncRelayCommand(DeleteSelectedTasks);

            _ = LoadTasksAsync();
        }

        public ICommand DeleteSelectedTasksCommand { get; }
        public ICommand SwitchToMainViewCommand { get; }
        public ICommand OpenDialogCommand { get; }
        public ICommand OpenTaskCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }

        public ProjectsInfo CurrentProject { get; }
        public ObservableCollection<TasksInfo> TaskItem { get; } = new();

        public string Name => CurrentProject.Name;
        public string? Description => CurrentProject.Description;

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        private int _totalCountOfTasks;
        public int TotalCountOfPages => (int)Math.Ceiling((double)_totalCountOfTasks / _pageSize);

        private string _searchFilter = string.Empty;
        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                _searchFilter = value;
                OnPropertyChanged(nameof(SearchFilter));
                _ = GetPageAsync();
            }
        }

        private bool _showAll = true;
        public bool ShowAll
        {
            get => _showAll;
            set
            {
                _showAll = value;
                OnPropertyChanged(nameof(ShowAll));
                _ = GetPageAsync();
            }
        }

        private bool _showClosed;
        public bool ShowClosed
        {
            get => _showClosed;
            set
            {
                _showClosed = value;
                OnPropertyChanged(nameof(ShowClosed));
                _ = GetPageAsync();
            }
        }

        private bool _showInProgress;
        public bool ShowInProgress
        {
            get => _showInProgress;
            set
            {
                _showInProgress = value;
                OnPropertyChanged(nameof(ShowInProgress));
                _ = GetPageAsync();
            }
        }

        public async Task RefreshAsync()
        {
            await LoadTasksAsync();
        }

        private async Task LoadTasksAsync()
        {
            _totalCountOfTasks = await _taskService.GetTasksCountAsync(CurrentProject.Id);
            OnPropertyChanged(nameof(TotalCountOfPages));
            await GetPageAsync();
        }

        private async Task OpenTask(TasksInfo task)
        {
            await _navigationService.NavigateTo<TaskViewModel>(CurrentProject, task);
        }

        private void SwitchToMainView() =>
            _navigationService.NavigateTo<MainWindowViewModel>();

        private bool CanSwitchToMainView() => true;

        private async Task OpenDialog()
        {
            await _dialogService.ShowDialogAsync<
                TaskDialogView,
                CreateTaskViewModel>(this, CurrentProject.Id);
        }

        private async Task NextPage()
        {
            if (CurrentPage < TotalCountOfPages)
            {
                CurrentPage++;
                await GetPageAsync();
            }
        }

        private async Task PrevPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await GetPageAsync();
            }
        }

        private async Task GetPageAsync()
        {
            var statuses = new List<TaskStatus>();

            // ShowAll overrides individual toggles —
            // when on, all statuses are included to simplify the filter UI
            if (ShowAll)
            {
                statuses.Add(TaskStatus.Done);
                statuses.Add(TaskStatus.InProgress);
                statuses.Add(TaskStatus.Closed);
            }
            else
            {
                if (ShowClosed)
                    statuses.Add(TaskStatus.Closed);

                if (ShowInProgress)
                    statuses.Add(TaskStatus.InProgress);
            }

            try
            {
                var filter = new TaskFilter
                {
                    ProjectId = CurrentProject.Id,
                    Page = CurrentPage,
                    PageSize = _pageSize,
                    Seacrh = SearchFilter,
                    Statuses = statuses
                };

                var tasks = await _taskService.GetTasksAsync(filter);

                TaskItem.Clear();

                foreach (var task in tasks)
                    TaskItem.Add(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to tasks for project {projectId}", CurrentProject.Id);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate("Error_LoadTasks"),
                    MessageDialogStyle.Affirmative);
            }
        }

        private async Task DeleteSelectedTasks()
        {
            try
            {
                var tasksToDelete = TaskItem
                    .Where(x => x.IsSelected)
                    .ToList();

                foreach (var task in tasksToDelete)
                {
                    await _taskService.DeleteAsync(task.Id);
                    TaskItem.Remove(task);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete selected tasks");
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate("Error_DeleteTasks"),
                    MessageDialogStyle.Affirmative);
            }
        }
    }
}
