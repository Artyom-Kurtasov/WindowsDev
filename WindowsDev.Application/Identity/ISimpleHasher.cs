namespace WindowsDev.Application.Identity;

public interface ISimpleHasher : IHasherBase
{
    public ulong HashSeed { get; }
    public ulong MixingConstant { get; }
    public int Iterations { get; }
}
