using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Domain.UsersAuthInfo
{
    public class UserAuthInfo
    {
        [Key]
        public int UserID { get; set; }
        public required string Login {  get; set; }
        public required string PasswordHash { get; set; }
        public required byte[] Salt { get; set; }
    }
}
