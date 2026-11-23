using WindowsDev.Contracts.UI.Projects;

namespace WindowsDev.ViewModels.Main.Tabs;

internal class SelectablelProjectViewModel : ViewModelBase
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
    public ProjectInfo Project { get; init; }

    public SelectablelProjectViewModel(ProjectInfo project)
    {
        Project = project;
    }
}
