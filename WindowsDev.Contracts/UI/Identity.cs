namespace WindowsDev.Contracts.UI;

public record AccessTokenInfo(string Token);
public record LoginForm(string Login, string Password);
public record RecoveryCodeInfo(int RecoveryCode);
public record RegisterForm(string Login, string Password, string Username);
