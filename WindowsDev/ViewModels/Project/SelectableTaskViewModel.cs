using WindowsDev.Contracts.UI.Tasks;

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
    public TaskInfo Task { get; init; }

    public SelectableTaskViewModel(TaskInfo task)
    {
        Task = task;
    }
}
