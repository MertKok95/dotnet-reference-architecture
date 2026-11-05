using DotNetCoreLab.Domain.Entities;

namespace DotNetCoreLab.Domain.Interfaces;

public interface IUserRepository
{
    IQueryable<User> AsQueryable();
    Task<User?> GetByIdAsync(int id);
    Task AddAsync(User user);
    void Remove(User user);
    Task SaveChangesAsync();
}