namespace WindowsDev.Business.Services.Registration.Validation
{
    public static class PasswordValidator
    {
        public static bool HasMinimumLength(string? password) => password?.Length >= 12;
        public static bool HasNumber(string? password) => password?.Any(char.IsDigit) == true;
        public static bool HasUppercase(string? password) => password?.Any(char.IsUpper) == true;
        public static bool HasSymbol(string? password) => password?.Any(c => !char.IsLetterOrDigit(c)) == true;
        public static bool IsValid(string? password)
        {
            return HasMinimumLength(password)
                && HasNumber(password)
                && HasUppercase(password)
                && HasSymbol(password);
        }
    }
}
