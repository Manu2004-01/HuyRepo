using EatIT.Infrastructure.Data;
using EatIT.WebAPI.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EatIT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BugController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public BugController(ApplicationDBContext context) 
        {
            _context = context;
        }

        [HttpGet("not-found")]
        public ActionResult GetNotFound()
        {
            var user = _context.Users.Find(50);
            if (user == null)
            {
                return NotFound(new BaseCommentResponse(404));
            }
            return Ok(user);
        }

        [HttpGet("Server-error")]
        public ActionResult GetServerError()
        {
            var user = _context.Users.Find(50);
            user.UserName = "";
            return Ok();
        }

        [HttpGet("Bad-Request/{id}")]
        public ActionResult GetNotFoundRequest()
        {
            return Ok();
        }

        [HttpGet("Bad-Request")]
        public ActionResult GetBadRequest()
        {
            return BadRequest();
        }
    }
}
