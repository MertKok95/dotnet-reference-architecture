using DotNetCoreLab.Application.Common.Interfaces;
using DotNetCoreLab.Application.Features.Users.Commands.CreateUser;
using DotNetCoreLab.Application.Features.Users.Commands.UpdateUser;
using DotNetCoreLab.Application.Features.Users.Queries.GetUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetCoreLab.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserQueryParameters parameters)
    {
        var pagedResult = await _userService.GetAllAsync(parameters);
        return Ok(pagedResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
    {
        var newUser = await _userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
    }

    [HttpGet("error-test")]
    public async Task<IActionResult> ErrorTest()
    {
        throw new Exception("Veritabanı bağlantısı koptu veya kritik bir kodlama hatası yapıldı!");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequestDto request)
    {
        await _userService.UpdateAsync(id, request);
        return NoContent(); 
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }

    public record RegisterRequest(string Name, string Email, string Password, string Role = "User");
    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = await _userService.RegisterAsync(request.Name, request.Email, request.Password, request.Role);
        return Ok(new { Mesaj = "Kayıt başarılı!", user.Id, user.Email, user.Role });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] ITokenService tokenService)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("E-posta ve şifre boş olamaz.");

        // Veritabanından kullanıcıyı ve şifresini doğrula
        var user = await _userService.ValidateUserAsync(request.Email, request.Password);
        if (user == null)
            return Unauthorized(new { Mesaj = "E-posta veya şifre hatalı!" });

        // Doğrulanan gerçek kullanıcının bilgileri ve Rolü ile Token üret
        var token = tokenService.GenerateToken(user);
        
        return Ok(new { Token = token, Role = user.Role });
    }
}