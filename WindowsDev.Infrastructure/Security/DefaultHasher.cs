using WindowsDev.Application.Identity;

namespace WindowsDev.Infrastructure.Security;

internal class DefaultHasher : HasherBase, IDefaultHasher
{
    protected override ulong HashSeed => 0941455814;
    protected override ulong MixingConstant => 1755195205;
    protected override int Iterations => 50000;

    ulong IDefaultHasher.HashSeed => HashSeed;
    ulong IDefaultHasher.MixingConstant => MixingConstant;
    int IDefaultHasher.Iterations => Iterations;
}