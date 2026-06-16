namespace WindowsDev.Business.Services.TaskService
{
    public class TaskFilter
    {
        public int ProjectId { get; set; }

        public string? Seacrh { get; set; }
        public List<Domain.TasksModels.Enums.TaskStatus> Statuses { get; set; } = new();

        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
