using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.UserManager;
using WindowsDev.Domain;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService
{
    /// <summary>
    /// Handles adding and retrieving comments for tasks.
    /// </summary>
    public class AddComment
    {
        private readonly CurrentUserData _currentUserData;
        private readonly AppDbContext _appDbContext;

        public AddComment(AppDbContext appDbContext, CurrentUserData currentUserData)
        {
            _appDbContext = appDbContext;
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
                Author = _currentUserData.Login,
                TaskId = taskItem.Id
            };

            await _appDbContext.Comments.AddAsync(comment);
            await _appDbContext.SaveChangesAsync();

            return comment;
        }

        /// <summary>
        /// Retrieves all comments for the specified task.
        /// </summary>
        public async Task<ObservableCollection<Comments>> GetComments(TasksInfo taskItem)
        {
            var comments = await _appDbContext.Comments
                .Where(x => x.TaskId == taskItem.Id)
                .ToListAsync();

            return new ObservableCollection<Comments>(comments);
        }
    }
}