namespace WindowsDev.Application.Services.PasswordManager.Hasher;

internal class DefaultHasher : HasherBase
{
    protected override ulong HashSeed => 0941455814;
    protected override ulong MixingConstant => 1755195205;
    protected override int Iterations => 50000;
}