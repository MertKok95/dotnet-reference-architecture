namespace DotNetCoreLab.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}