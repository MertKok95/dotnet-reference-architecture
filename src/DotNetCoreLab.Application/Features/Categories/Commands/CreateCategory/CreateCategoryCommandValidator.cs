using FluentValidation;

namespace DotNetCoreLab.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x=> x.Name).NotEmpty().WithMessage("Category Name is not empty")
        .MaximumLength(100).WithMessage("Category Name length should be 100 as Maximum");

        RuleFor(x=> x.Description).NotEmpty().WithMessage("Description is not empty")
        .MaximumLength(500).WithMessage("Description length limit is 500");
    }
}