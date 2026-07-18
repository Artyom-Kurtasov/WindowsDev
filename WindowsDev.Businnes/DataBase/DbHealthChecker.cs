using WindowsDev.Business.DataBase.Interfaces;

namespace WindowsDev.Business.DataBase
{
    public class DbHealthChecker : IDbHealthChecker
    {
        private readonly IDbManager _dbManager;

        public DbHealthChecker(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public void Check()
        {
            using var dbContext = _dbManager.Create();

            if (!dbContext.Database.CanConnect())
                throw new Exception("Database is not reachable");

            CheckUsers(dbContext);
            CheckProjects(dbContext);
            CheckAttachments(dbContext);
            CheckTasks(dbContext);
            CheckComments(dbContext);
        }

        private void CheckUsers(AppDbContext appDbContext)
        {
            _ = appDbContext.UsersInfo
                .Select(u => new
                {
                    u.Id,
                    u.Login,
                    u.Username,
                    u.PasswordHash,
                    u.HashMethod,
                    u.Salt
                })
                .FirstOrDefault();
        }

        private void CheckProjects(AppDbContext appDbContext)
        {
            _ = appDbContext.ProjectsInfo
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.UserId,
                    p.CreatedAt,
                    p.Description
                })
                .FirstOrDefault();
        }

        private void CheckTasks(AppDbContext appDbContext)
        {
            _ = appDbContext.TasksInfo
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Description,
                    t.Progress,
                    t.ProjectId,
                    t.Priority,
                    t.Status,
                    t.DeadLine,
                    t.CreatedAt
                })
                .FirstOrDefault();
        }

        private void CheckAttachments(AppDbContext appDbContext)
        {
            _ = appDbContext.Attachments
                .Select(a => new
                {
                    a.Id,
                    a.FilePath,
                    a.FileExtension,
                    a.FileSize,
                    a.FileName,
                    a.TaskId
                })
                .FirstOrDefault();
        }

        private void CheckComments(AppDbContext appDbContext)
        {
            _ = appDbContext.Comments
                .Select(c => new
                {
                    c.Id,
                    c.Text,
                    c.CreatedAt,
                    c.Author,
                    c.TaskId
                })
                .FirstOrDefault();
        }
    }
}