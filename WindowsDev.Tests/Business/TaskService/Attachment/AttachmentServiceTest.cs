using Moq;
using WindowsDev.Application.RepositoriesInterfaces;
using WindowsDev.Application.Services.TaskService.Attachment;
using WindowsDev.Application.Services.TaskService.Attachment.FileService;
using WindowsDev.Application.Services.TaskService.Attachment.FileServiceInterfaces;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Tests.Business.TaskService.Attachment
{
    public class AttachmentServiceTest
    {
        private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock;
        private readonly Mock<IFilePicker> _filePickerMock;
        private readonly Mock<IFileOpener> _fileOpenerMock;

        public AttachmentServiceTest()
        {
            _attachmentRepositoryMock = new Mock<IAttachmentRepository>();
            _filePickerMock = new Mock<IFilePicker>();
            _fileOpenerMock = new Mock<IFileOpener>();
        }

        private AttachmentService CreateService()
        {
            return new AttachmentService(
                _attachmentRepositoryMock.Object,
                _filePickerMock.Object,
                _fileOpenerMock.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task AddFile_WhenTaskIdInvalid_ThrowsException(int taskId)
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.AddFile(taskId));

            _attachmentRepositoryMock.Verify(
                x => x.AddFileInfoToDatabase(It.IsAny<TaskAttachment>()),
                Times.Never
            );
        }

        [Fact]
        public async Task GetAttachmentsAsync_WhenCalled_ReturnsAttachments()
        {
            var taskId = 1;

            var attachments = new List<TaskAttachment>
            {
                new()
                {
                    Id = 1,
                    FileName = "test.txt",
                    FilePath = "C:\\test.txt",
                    TaskId = taskId,
                    FileExtension = ".txt",
                    FileSize = 100,
                },
            };

            _attachmentRepositoryMock
                .Setup(x => x.GetAttachmentsAsync(taskId))
                .ReturnsAsync(attachments);

            var service = CreateService();

            var result = await service.GetAttachmentsAsync(taskId);

            Assert.Single(result);
            Assert.Equal("test.txt", result[0].FileName);

            _attachmentRepositoryMock.Verify(x => x.GetAttachmentsAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task GetAttachmentsAsync_WhenRepositoryReturnsEmpty_ReturnsEmptyList()
        {
            var taskId = 1;

            _attachmentRepositoryMock
                .Setup(x => x.GetAttachmentsAsync(taskId))
                .ReturnsAsync(new List<TaskAttachment>());

            var service = CreateService();

            var result = await service.GetAttachmentsAsync(taskId);

            Assert.Empty(result);
        }

        private TaskAttachment CreateAttachment()
        {
            return new TaskAttachment
            {
                Id = 1,
                FileName = "test.txt",
                FilePath = "C:\\test.txt",
                FileExtension = ".txt",
                FileSize = 100,
                TaskId = 1,
            };
        }
    }
}