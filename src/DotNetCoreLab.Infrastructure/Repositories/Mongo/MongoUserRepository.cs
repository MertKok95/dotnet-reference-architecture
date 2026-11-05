using DotNetCoreLab.Domain.Entities;
using DotNetCoreLab.Domain.Interfaces;
using MongoDB.Driver;

namespace DotNetCoreLab.Infrastructure.Repositories.Mongo;

public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public MongoUserRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("Users");
    }

    public IQueryable<User> AsQueryable()
    {
        return _users.AsQueryable();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddAsync(User user)
    {
        await _users.InsertOneAsync(user);
    }

    public void Remove(User user)
    {
        _users.DeleteOne(u => u.Id == user.Id);
    }

    public Task SaveChangesAsync()
    {
        // MongoDB işlemleri anında kaydedildiği için ek bir SaveChanges gerekmez
        return Task.CompletedTask;
    }
}