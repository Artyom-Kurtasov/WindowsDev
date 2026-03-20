namespace WindowsDev.Domain.UsersAuthInfo
{
    public class TaskItem
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; } 
        public required string Priority { get; set; }
        public required string Progress { get; set; }
        public required DateTime Created {  get; set; }
        public required DateTime DeadLine { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Attachment>? Attachments { get; set; }
    }
}
