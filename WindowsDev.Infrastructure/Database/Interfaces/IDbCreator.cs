namespace WindowsDev.Infrastructure.Database.Interfaces
{
    public interface IDbCreator
    {
        AppDbContext Create();
    }
}
