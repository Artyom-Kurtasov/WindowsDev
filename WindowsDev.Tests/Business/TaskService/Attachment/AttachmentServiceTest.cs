using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.TaskService.Attachment;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Tests.Business.TaskService.Attachment
{
    public class AttachmentServiceTest
    {
        private readonly Mock<IAttachmentRepository> _attachmentRepositoryMock;

        public AttachmentServiceTest()
        {
            _attachmentRepositoryMock = new Mock<IAttachmentRepository>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AddFile_WhenFilePathEmpty_ThrowArgumentException(string filePath)
        {
            var writer = new AttachmentService(_attachmentRepositoryMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await writer.AddFile(filePath, 1));

            _attachmentRepositoryMock.Verify(x => x.AddFileInfoToDatabase(It.IsAny<TaskAttachment>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task AddFile_WhenInvalidTaskId_ThrowsArgumentException(int taskId)
        {
            var writer = new AttachmentService(_attachmentRepositoryMock.Object);
            var tempFile = Path.GetTempFileName();

            await Assert.ThrowsAsync<ArgumentException>(async () => await writer.AddFile(tempFile, taskId));

            _attachmentRepositoryMock.Verify(x => x.AddFileInfoToDatabase(It.IsAny<TaskAttachment>()), Times.Never);

            File.Delete(tempFile);
        }

        [Fact]
        public async Task AddFile_WhenInvalidFilePath_ThrowsFileNotFoundException()
        {
            string invalidPath = "invalidPath";

            var writer = new AttachmentService(_attachmentRepositoryMock.Object);

            await Assert.ThrowsAsync<FileNotFoundException>(async () => await writer.AddFile(invalidPath, 1));

            _attachmentRepositoryMock.Verify(x => x.AddFileInfoToDatabase(It.IsAny<TaskAttachment>()), Times.Never);
        }
    }
}
