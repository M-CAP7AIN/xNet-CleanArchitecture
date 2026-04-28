using Application.Controller.Auth.Commands;
using Application.Controller.Auth.Queries;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    public class AuthController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
    {
       
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var result = await mediator.Send(command);

            if (!result.Success)
                return BadRequest(result.Errors);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await mediator.Send(command);

            if (!result.Success)
                return Unauthorized(result.Errors);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = currentUserService.UserId.ToString();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new GetCurrentUserQuery(userId);

            var result = await mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "Welcome Admin!" });
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand refreshToken)
        {
            var result = await mediator.Send(refreshToken);

            if (!result.Success)
                return Unauthorized(result.Errors);

            return Ok(result);
        }
    }
}
