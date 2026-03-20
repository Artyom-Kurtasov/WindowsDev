using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Domain
{
    public class Comment
    {
        public required int Id { get; set; }
        public required string Text { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required string Author { get; set; }
        public required int TaskId { get; set; }
        public required TaskItem Task { get; set; }
    }
}
