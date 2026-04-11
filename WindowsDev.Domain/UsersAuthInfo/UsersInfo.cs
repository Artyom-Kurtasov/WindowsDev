using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Domain.UsersAuthInfo
{
    public class UsersInfo
    {
        [Key]
        public int Id { get; set; }
        public required string Login { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required byte[] Salt { get; set; }
    }
}


