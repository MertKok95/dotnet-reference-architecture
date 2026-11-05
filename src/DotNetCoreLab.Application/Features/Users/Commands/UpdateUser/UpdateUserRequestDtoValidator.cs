using DotNetCoreLab.Application.Features.Users.Commands.UpdateUser;
using FluentValidation;

namespace DotNetCoreLab.Application.Validators;

public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}