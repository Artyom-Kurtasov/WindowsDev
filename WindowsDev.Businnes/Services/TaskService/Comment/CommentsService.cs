using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.TaskService.Comment.Interfaces;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService.Comment
{
    public class CommentsService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICurrentUserService _currentUserService;

        public CommentsService(ICommentRepository commentRepository, ICurrentUserService currentUserService)
        {
            _commentRepository = commentRepository;
            _currentUserService = currentUserService;
        }

        public async Task<Comments> AddComment(int taskId, string commentText)
        {
            if (taskId < 1)
                throw new ArgumentException("Invalid task id", nameof(taskId));
            if (string.IsNullOrWhiteSpace(commentText))
                throw new ArgumentException("Comment is empty", nameof(commentText));

            var comment = new Comments
            {
                Text = commentText,
                CreatedAt = DateTime.UtcNow,
                Author = _currentUserService.Username,
                TaskId = taskId
            };

            await _commentRepository.AddComments(comment);

            return comment;
        }

        public async Task<List<Comments>> GetComments(int taskId)
        {
            return await _commentRepository.GetComments(taskId);
        }
    }
}

