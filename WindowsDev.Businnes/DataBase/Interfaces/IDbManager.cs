namespace WindowsDev.Business.DataBase.Interfaces
{
    public interface IDbManager
    {
        string ConnectionString { get; set; }
        AppDbContext Create();
    }
}
