using WindowsDev.Api.DTO.Response.ProjectsController;

namespace WindowsDev.ViewModels.Main.Tabs;

internal class SelectableItemViewModel : ViewModelBase
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

    public SelectableItemViewModel(GetProjectsResponse project)
    {
        Project = project;
    }
}
