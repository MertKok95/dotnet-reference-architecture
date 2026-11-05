using DotNetCoreLab.Domain.Entities;
using DotNetCoreLab.Domain.Interfaces;
using DotNetCoreLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore; 

namespace DotNetCoreLab.Infrastructure.Repositories;

public class EfCategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;

    public EfCategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Category newCategory)
    {
        await _dbContext.AddAsync(newCategory);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _dbContext.Set<Category>().ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await _dbContext.Set<Category>().FirstOrDefaultAsync(c => c.Id == id);
    }
}