using AuthService.Filters;
using AuthService.Service.Contract;
using Microsoft.AspNetCore.Mvc;
using AuthService.Model.Dto;

namespace AuthService.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
    {

        var response = await authService.Register(model);
        if (response.Success)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        var response = await authService.Login(model);
        if (response.Success)
        {
            return Ok(response);
        }

        return BadRequest(response);
    }

}

