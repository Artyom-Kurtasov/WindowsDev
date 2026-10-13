using WindowsDev.Contracts.Api.Projects;

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
    public GetProjectsResponse Project { get; init; }

    public SelectablelProjectViewModel(GetProjectsResponse project)
    {
        Project = project;
    }
}
