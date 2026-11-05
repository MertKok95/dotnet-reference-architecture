using DotNetCoreLab.Domain.Entities;


namespace DotNetCoreLab.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();

    Task<Category?> GetCategoryByIdAsync(Guid id);

    Task AddAsync(Category newCategory);
}