namespace WindowsDev.Application.Identity.Cryptography;

public interface IDefaultHasher : IHasherBase
{
    public ulong HashSeed { get; }
    public ulong MixingConstant { get; }
    public int Iterations { get; }
}
