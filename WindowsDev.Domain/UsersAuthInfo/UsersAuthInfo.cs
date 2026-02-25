using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Domain.UsersAuthInfo
{
    public class UsersAuthInfo
    {
        [Key]
        public int Id { get; set; }
        public required string UserLogin {  get; set; }
        public required string UserPasswordHash { get; set; }
        public required string UserEmail { get; set; }
        public required string UserPasswordSalt { get; set; }
    }
}
