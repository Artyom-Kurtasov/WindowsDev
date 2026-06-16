namespace WindowsDev.Business.Services.PasswordManager.Hasher
{
    public class SimpleHasher : HasherBase
    {
        public override ulong HashSeed => 16480028562;
        public override ulong MixingConstant => 0004517461;
        public override int Iterations => 100000;
    }
}
