using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Model.Dto;
using UserService.Service.Contract;

namespace UserService.Controllers;

[ApiController]
[Authorize]
[Route("api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLogged()
    {
        return Ok(await userService.GetLoggedUser());
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] UserDto userDto)
    {
        return Ok(await userService.Update(userDto));
    }

    [HttpGet("delete")]
    public async Task<IActionResult> Delete()
    {
        await userService.Delete();
        return NoContent();
    }

}
