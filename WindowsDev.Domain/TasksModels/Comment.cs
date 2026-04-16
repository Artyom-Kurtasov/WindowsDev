using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Domain.TasksModels
{
    public class Comments
    {
        [Key]
        public int Id { get; set; }
        public required string Text { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required string Author { get; set; }
        public required int TaskId { get; set; }
        public TasksInfo? Task { get; set; }
    }
}


