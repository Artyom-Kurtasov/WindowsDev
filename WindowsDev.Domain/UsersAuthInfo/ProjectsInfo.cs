using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Domain.UsersAuthInfo
{
    public class ProjectsInfo
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required int UserId { get; set; }
    }
}
