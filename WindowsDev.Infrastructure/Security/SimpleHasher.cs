using WindowsDev.Application.Identity;

namespace WindowsDev.Infrastructure.Security;

internal class SimpleHasher : HasherBase, ISimpleHasher
{
    protected override ulong HashSeed => 16480028562;
    protected override ulong MixingConstant => 0004517461;
    protected override int Iterations => 100000;

    ulong ISimpleHasher.HashSeed => HashSeed;
    ulong ISimpleHasher.MixingConstant => MixingConstant;
    int ISimpleHasher.Iterations => Iterations;
}