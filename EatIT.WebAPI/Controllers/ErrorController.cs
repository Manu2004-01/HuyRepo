using EatIT.WebAPI.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EatIT.WebAPI.Controllers
{
    [Route("errors/{statusCode}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [HttpGet("Error")]
        public IActionResult Error(int statusCode)
        {
            return new ObjectResult(new BaseCommentResponse(statusCode));
        }
    }
}
