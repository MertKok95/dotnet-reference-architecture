


using AutoMapper;
using DotNetCoreLab.Application.Features.Categories.Dtos;
using DotNetCoreLab.Domain.Interfaces;
using MediatR;

namespace DotNetCoreLab.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{

    private readonly IMapper _mapper;
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
       var result = await _categoryRepository.GetCategoryByIdAsync(request.Id);

        if(result is null)
            return null;

        //    return new CategoryDto{Id = result.Id, Name = result.Name, 
        //    Description = result.Description, IsActive = result.IsActive };

        return _mapper.Map<CategoryDto>(result);
    } 
}