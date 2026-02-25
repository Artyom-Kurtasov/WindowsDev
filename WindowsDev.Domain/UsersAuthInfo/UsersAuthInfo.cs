using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WindowsDev.Domain.UsersAuthInfo
{
    [Table("UsersAuthInfo")]
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
