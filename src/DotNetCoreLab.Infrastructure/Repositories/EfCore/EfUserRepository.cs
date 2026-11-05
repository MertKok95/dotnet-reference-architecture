using DotNetCoreLab.Domain.Entities;
using DotNetCoreLab.Domain.Interfaces;
using DotNetCoreLab.Infrastructure.Persistence;

namespace DotNetCoreLab.Infrastructure.Repositories.EfCore;

public class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public EfUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<User> AsQueryable()
    {
        return _context.Users.AsQueryable();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public void Remove(User user)
    {
        _context.Users.Remove(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}