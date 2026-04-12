using Npgsql;

namespace WindowsDev.Business.DataBase
{
    public class DbManager
    {
        private string _connectionString;

        public async Task<bool> SetConnection(string connectionString)
        {
            if (CheckConnectionString(connectionString))
            {
                if (await TestConnectionAsync(connectionString))
                {
                    _connectionString = connectionString;
                    return true;
                }
            }
            return false;
        }

        public AppDbContext Create()
        {
            return new AppDbContext(_connectionString);
        }

        private bool CheckConnectionString(string connectionString)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();
                return true;
            }
            catch (Npgsql.NpgsqlException)
            {
                return false;
            }
        }
    }
}


