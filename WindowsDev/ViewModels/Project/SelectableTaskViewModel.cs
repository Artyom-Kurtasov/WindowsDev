using WindowsDev.Contracts.Api.Tasks;

namespace WindowsDev.ViewModels.Project;

internal class SelectableTaskViewModel : ViewModelBase
{
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }
    public GetTasksResponse Task { get; init; }

    public SelectableTaskViewModel(GetTasksResponse task)
    {
        Task = task;
    }
}
