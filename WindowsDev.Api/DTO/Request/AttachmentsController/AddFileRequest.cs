using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request.AttachmentsController
{
    public class AddFileRequest
    {
        public required string FileName { get; set; } = string.Empty;
        public required int TaskId { get; set; }
    }
}
