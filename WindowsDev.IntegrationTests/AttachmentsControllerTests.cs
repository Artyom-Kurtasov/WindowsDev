using System.Net;
using WindowsDev.IntegrationTests.Infrastructure;

namespace WindowsDev.IntegrationTests;

public class AttachmentsControllerTests : BaseIntegrationTest
{
    public AttachmentsControllerTests(PostgreApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
    {
        var response = await Client.GetAsync("api/attachments/task/17");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
