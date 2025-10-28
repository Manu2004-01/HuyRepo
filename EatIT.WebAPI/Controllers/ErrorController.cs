using EatIT.WebAPI.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EatIT.WebAPI.Controllers
{
    [Route("errors")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [HttpGet("{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            return new ObjectResult(new BaseCommentResponse(statusCode));
        }
    }
}
