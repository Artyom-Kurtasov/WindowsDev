using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Domain
{
    public class Attachment
    {
        public required int Id { get; set; }
        public required string Type { get; set; }
        public required string Path { get; set; }
        public required string FileName { get; set; }
        public required int TaskId { get; set; }
        public required TasksInfo Task { get; set; }
    }
}
