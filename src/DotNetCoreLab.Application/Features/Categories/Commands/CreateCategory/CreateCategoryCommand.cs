using MediatR;

namespace DotNetCoreLab.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name, string Description, bool IsActive) : IRequest<Guid>;

