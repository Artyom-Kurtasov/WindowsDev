namespace WindowsDev.Business.Services.Registration
{
    public class PasswordValidator
    {
        public bool HasMinimumLength(string? password) => password?.Length >= 12;
        public bool HasNumber(string? password) => password?.Any(char.IsDigit) == true;
        public bool HasUppercase(string? password) => password?.Any(char.IsUpper) == true;
        public bool HasSymbol(string? password) => password?.Any(c => !char.IsLetterOrDigit(c)) == true;
    }
}
