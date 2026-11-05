

using DotNetCoreLab.Application.Features.Categories.Dtos;
using MediatR;

namespace DotNetCoreLab.Application.Features.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id): IRequest<CategoryDto?>;