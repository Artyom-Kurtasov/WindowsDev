using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Domain
{
    public class TaskAttachment
    {
        public int Id { get; set; }
        public required string FileExtension { get; set; }
        public required string FilePath { get; set; }
        public required string FileName { get; set; }
        public required double FileSize { get; set; }
        public required int TaskId { get; set; }
        public TasksInfo Task { get; set; }
    }
}
