using System.Text.Json.Serialization;

namespace WindowsDev.Contracts.Api.Common;

public class ProblemDetails
{
    public int? StatusCode { get; set; }
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public string? Type { get; set; }
    public string? Instance { get; set; }
    public string? ErrorMessageCode { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object?> Extensions {  get; set; }
}
