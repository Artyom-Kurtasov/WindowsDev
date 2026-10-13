namespace WindowsDev.Application.Identity.Cryptography;

public interface ISimpleHasher : IHasherBase
{
    public ulong HashSeed { get; }
    public ulong MixingConstant { get; }
    public int Iterations { get; }
}
