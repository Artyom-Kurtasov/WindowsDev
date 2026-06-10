using Microsoft.EntityFrameworkCore;
using WindowsDev.Business.DataBase.Interfaces;

namespace WindowsDev.Business.DataBase
{
    public class DbManager : IDbManager
    {
        public string ConnectionString { get; set; }

        public AppDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            return new AppDbContext(options);
        }
    }
}


