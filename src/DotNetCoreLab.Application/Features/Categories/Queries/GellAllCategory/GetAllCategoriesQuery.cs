using DotNetCoreLab.Domain.Entities;
using MediatR;

namespace DotNetCoreLab.Application.Features.Categories.Queries.GetAllCategory;

public record GetAllCategoriesQuery : IRequest<IEnumerable<Category>>;

