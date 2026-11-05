using DotNetCoreLab.Application.Features.Categories.Events;
using DotNetCoreLab.Domain.Entities;
using DotNetCoreLab.Domain.Interfaces;
using MediatR;

namespace DotNetCoreLab.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{

    private readonly IMediator _mediator;
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandHandler(IMediator mediator, ICategoryRepository categoryRepository)
    {
        _mediator = mediator;
        _categoryRepository = categoryRepository;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        Category newCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        };

        await _categoryRepository.AddAsync(newCategory);

        await _mediator.Publish(new CategoryCreatedEvent(newCategory.Id, newCategory.Name), cancellationToken);

        return newCategory.Id;
    }
}