using AutoMapper;
using DotNetCoreLab.Application.Features.Categories.Commands.CreateCategory;
using DotNetCoreLab.Application.Features.Categories.Dtos;

// CategoryDto'yu nereye taşıdıysan onun using'ini ekle (Örn: using DotNetCoreLab.Application.Features.Categories.Queries.GetCategoryById;)
using DotNetCoreLab.Domain.Entities;

namespace DotNetCoreLab.Application.Features.Categories.Profiles;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        // 1. Veritabanından (Entity) okuyup dışarıya (DTO) çıkarken:
        CreateMap<Category, CategoryDto>();

        // 2. Dışarıdan (Command) gelip veritabanına (Entity) kaydederken:
        CreateMap<CreateCategoryCommand, Category>();
    }
}