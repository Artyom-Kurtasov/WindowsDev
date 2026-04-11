using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using WindowsDev.Business.DataBase;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Domain;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.TaskService
{
    /// <summary>
    /// Handles adding and retrieving comments for tasks.
    /// </summary>
    public class AddComment
    {
        private readonly CurrentUserData _currentUserData;
        private readonly DbManager _dbManager;

        public AddComment(DbManager dbManager, CurrentUserData currentUserData)
        {
            _dbManager = dbManager;
            _currentUserData = currentUserData;
        }

        /// <summary>
        /// Adds a comment to the specified task.
        /// </summary>
        public async Task<Comments> AddComments(TasksInfo taskItem, string commentText)
        {
            var comment = new Comments
            {
                Text = commentText,
                CreatedAt = DateTime.UtcNow,
                Author = _currentUserData.Username,
                TaskId = taskItem.Id
            };

            using var dbContext = _dbManager.Create();

            await dbContext.Comments.AddAsync(comment);
            await dbContext.SaveChangesAsync();

            return comment;
        }

        /// <summary>
        /// Retrieves all comments for the specified task.
        /// </summary>
        public async Task<ObservableCollection<Comments>> GetComments(TasksInfo taskItem)
        {
            using var dbContext = _dbManager.Create();

            var comments = await dbContext.Comments
                .Where(x => x.TaskId == taskItem.Id)
                .ToListAsync();

            return new ObservableCollection<Comments>(comments);
        }
    }
}
