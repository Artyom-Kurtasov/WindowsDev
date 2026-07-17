using System.Collections.ObjectModel;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.TaskService;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Tasks;
using WindowsDev.ViewModels.Tasks.Dialog;
using WindowsDev.Views.Tasks;
using TaskStatus = WindowsDev.Domain.TasksModels.Enums.TaskStatus;

namespace WindowsDev.ViewModels.Projects
{
    public class ProjectViewModel : LocalizedViewModelBase, IRefreshableViewModel, IDisposable
    {
        private const int PageSize = 15;

        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly ITaskService _taskService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ILogger<ProjectViewModel> _logger;

        public ProjectViewModel(
            ProjectsInfo currentProject,
            IDialogCoordinator dialogCoordinator,
            INavigationService navigationService,
            ITaskService taskService,
            IDialogService dialogService,
            ILogger<ProjectViewModel> logger,
            ILanguageChanger languageChanger
        )
            : base(languageChanger)
        {
            CurrentProject = currentProject;

            _dialogCoordinator = dialogCoordinator;
            _navigationService = navigationService;
            _taskService = taskService;
            _dialogService = dialogService;
            _logger = logger;

            SwitchToMainViewCommand = new RelayCommand(SwitchToMainView);
            OpenDialogCommand = new AsyncRelayCommand(OpenTaskDialogAsync);
            OpenTaskCommand = new AsyncRelayCommandT<TasksInfo>(OpenTaskAsync);
            NextPageCommand = new AsyncRelayCommand(NextPageAsync);
            PrevPageCommand = new AsyncRelayCommand(PrevPageAsync);
            DeleteSelectedTasksCommand = new AsyncRelayCommand(DeleteSelectedTasksAsync);

            _ = LoadTasksAsync();
        }

        public ICommand DeleteSelectedTasksCommand { get; }
        public ICommand SwitchToMainViewCommand { get; }
        public ICommand OpenDialogCommand { get; }
        public ICommand OpenTaskCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }

        public ProjectsInfo? CurrentProject { get; private set; }

        public ObservableCollection<TasksInfo> Tasks { get; private set; } = new();

        public string? Name => CurrentProject?.Name;
        public string? Description => CurrentProject?.Description;

        private int _currentPage = 1;

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage == value)
                    return;

                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        private int _totalCountOfTasks;

        public int TotalCountOfPages => (int)Math.Ceiling((double)_totalCountOfTasks / PageSize);

        private string _searchFilter = string.Empty;

        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                if (_searchFilter == value)
                    return;

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
                if (_showAll == value)
                    return;

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
                if (_showClosed == value)
                    return;

                _showClosed = value;
                OnPropertyChanged(nameof(ShowClosed));

                _ = GetPageAsync();
            }
        }

        private bool _showInProgress;
        private bool disposedValue;

        public bool ShowInProgress
        {
            get => _showInProgress;
            set
            {
                if (_showInProgress == value)
                    return;

                _showInProgress = value;
                OnPropertyChanged(nameof(ShowInProgress));

                _ = GetPageAsync();
            }
        }

        public async Task RefreshAsync()
        {
            await LoadTasksAsync();
        }

        public async Task NextPageAsync()
        {
            if (CurrentPage >= TotalCountOfPages)
                return;

            CurrentPage++;
            await GetPageAsync();
        }

        public async Task PrevPageAsync()
        {
            if (CurrentPage <= 1)
                return;

            CurrentPage--;
            await GetPageAsync();
        }

        private async Task OpenTaskAsync(TasksInfo task)
        {
            await _navigationService.NavigateTo<TaskViewModel>(CurrentProject, task);
        }

        private void SwitchToMainView()
        {
            Dispose();
            _navigationService.NavigateTo<MainWindowViewModel>();
        }

        private async Task OpenTaskDialogAsync()
        {
            await _dialogService.ShowDialogAsync<TaskDialogView, CreateTaskViewModel>(
                this,
                CurrentProject!.Id
            );
        }

        private async Task LoadTasksAsync()
        {
            try
            {
                _totalCountOfTasks = await _taskService.GetTasksCountAsync(CurrentProject!.Id);

                OnPropertyChanged(nameof(TotalCountOfPages));

                await GetPageAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex);
            }
        }

        private async Task GetPageAsync()
        {
            try
            {
                var filter = new TaskFilter
                {
                    ProjectId = CurrentProject!.Id,
                    Page = CurrentPage,
                    PageSize = PageSize,
                    Seacrh = SearchFilter,
                    Statuses = GetSelectedStatuses(),
                };

                var result = await _taskService.GetTasksAsync(filter);

                Tasks.Clear();

                foreach (var task in result.Value)
                    Tasks.Add(task);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex);
            }
        }

        private async Task DeleteSelectedTasksAsync()
        {
            var tasksToDelete = Tasks.Where(x => x.IsSelected).ToList();

            foreach (var task in tasksToDelete)
            {
                try
                {
                    await _taskService.DeleteAsync(task.Id);
                    Tasks.Remove(task);
                }
                catch (Exception ex)
                {
                    TaskLogs.TaskDeleteFailed(_logger, task.Id, ex);

                    await ShowErrorAsync(ex);
                }
            }
        }

        private List<TaskStatus> GetSelectedStatuses()
        {
            if (ShowAll)
            {
                return new() { TaskStatus.Done, TaskStatus.InProgress, TaskStatus.Closed };
            }

            var statuses = new List<TaskStatus>();

            if (ShowClosed)
                statuses.Add(TaskStatus.Closed);

            if (ShowInProgress)
                statuses.Add(TaskStatus.InProgress);

            return statuses;
        }

        private async Task ShowErrorAsync(Exception exception)
        {
            TaskLogs.TaskLoadFailed(_logger, CurrentProject!.Id, exception);

            await _dialogCoordinator.ShowMessageAsync(
                this,
                Translate(DialogTitles.Error),
                Translate(CommonErrors.UnexpectedError),
                MessageDialogStyle.Affirmative
            );
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Tasks = null!;
                    CurrentProject = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
