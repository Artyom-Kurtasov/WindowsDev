using WindowsDev.Application.Primitives;
using WindowsDev.Application.RepositoriesInterfaces;
using WindowsDev.Application.Services.UserManager;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Services.TaskService.Comment
{
    public class CommentsService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICurrentUserService _currentUserService;

        public CommentsService(
            ICommentRepository commentRepository,
            ICurrentUserService currentUserService
        )
        {
            _commentRepository = commentRepository;
            _currentUserService = currentUserService;
        }

        public async Task<Result<TaskComment>> AddComment(int taskId, string commentText)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(taskId);

            var comment = new TaskComment
            {
                Text = commentText,
                CreatedAt = DateTime.UtcNow,
                Author = _currentUserService.Username,
                TaskId = taskId,
            };

            await _commentRepository.AddComments(comment);

            return Result<TaskComment>.Success(comment);
        }

        public async Task<List<TaskComment>> GetComments(int taskId)
        {
            return await _commentRepository.GetComments(taskId);
        }
    }
}
