using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Domain.UsersLoginAndPasswords
{
    public class UsersLoginAndPasswords
    {
        [Key]
        public int Id { get; set; }
        public string UserLogin {  get; set; }
        public string UserPassword { get; set; }
    }
}
