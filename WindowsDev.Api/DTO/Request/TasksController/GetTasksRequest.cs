namespace WindowsDev.Api.DTO.Request.TasksController
{
    public class GetTasksRequest
    {
        public int ProjectId { get; set; }
        public string? Seacrh { get; set; }
        public List<Domain.Enums.TaskStatus> Statuses { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
