namespace WindowsDev.Business.Services.PasswordManager.Hasher
{
    public class DefaultHasher : HasherBase
    {
        public override ulong HashSeed => 0941455814;
        public override ulong MixingConstant => 1755195205;
        public override int Iterations => 50000;
    }
}
