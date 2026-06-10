using System.ComponentModel.DataAnnotations;
using WindowsDev.Domain.UsersModels.Enums;

namespace WindowsDev.Domain.UsersModels
{
    public class UsersInfo
    {
        [Key]
        public int Id { get; set; }
        public required string Login { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required byte[] Salt { get; set; }
        public required HashMethod HashMethod { get; set; }
        public string? RecoveryCodeHash { get; set; }
        public byte[]? RecoveryCodeSalt { get; set; }
    }
}


