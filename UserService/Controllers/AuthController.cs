using Microsoft.AspNetCore.Mvc;
using UserService.Filters;
using UserService.Model.Dto;
using UserService.Service.Contract;

namespace UserService.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
    {
        await authService.Register(model);
        return Created();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        return Ok(await authService.Login(model));
    }
}
