using DotNetCoreLab.Application.Features.Categories.Commands.CreateCategory;
using DotNetCoreLab.Application.Features.Categories.Dtos;
using DotNetCoreLab.Application.Features.Categories.Queries.GetAllCategory;
using DotNetCoreLab.Application.Features.Categories.Queries.GetCategoryById;
using DotNetCoreLab.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetCoreLab.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoryController(IMediator mediator) : ControllerBase
{
    
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] CreateCategoryCommand command)
    {
        Guid result =  await mediator.Send(command);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        GetAllCategoriesQuery query = new GetAllCategoriesQuery();
        IEnumerable<Category> result = await mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(id);
        CategoryDto? result = await mediator.Send(query, cancellationToken);

        return Ok(result);
    }

}