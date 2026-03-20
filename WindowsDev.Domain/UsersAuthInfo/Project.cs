using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Domain.UsersAuthInfo
{
    public class Project
    {
        [Key]
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Created {  get; set; }
    }
}
