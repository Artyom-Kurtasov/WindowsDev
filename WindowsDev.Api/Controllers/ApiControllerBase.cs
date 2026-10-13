using Microsoft.AspNetCore.Mvc;

namespace WindowsDev.Api.Controllers;

public class ApiControllerBase : ControllerBase
{
    [NonAction]
    protected ObjectResult CustomProblem(
        int statusCode,
        string title,
        string detail,
        string errorMessageCode
    )
    {
        return Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            type: "about:blank",
            instance: HttpContext.Request.Path,
            extensions: new Dictionary<string, object?> { { "errorMessageCode", errorMessageCode } }
        );
    }
}
