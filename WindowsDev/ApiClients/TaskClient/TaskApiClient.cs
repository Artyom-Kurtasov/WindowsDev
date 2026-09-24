using System.Net.Http;

namespace WindowsDev.ApiClients.TaskClient;

internal class TaskApiClient
{
    private readonly HttpClient _httpClient;

    public TaskApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task
}
