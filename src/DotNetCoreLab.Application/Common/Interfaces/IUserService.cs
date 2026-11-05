using DotNetCoreLab.Application.Common.Models;
using DotNetCoreLab.Application.Features.Users.Commands.CreateUser;
using DotNetCoreLab.Application.Features.Users.Commands.UpdateUser;
using DotNetCoreLab.Application.Features.Users.Dtos;
using DotNetCoreLab.Application.Features.Users.Queries.GetUsers;
using DotNetCoreLab.Domain.Entities;

namespace DotNetCoreLab.Application.Common.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserResponseDto>> GetAllAsync(UserQueryParameters parameters);
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<UserResponseDto> CreateAsync(CreateUserRequestDto request);
    Task UpdateAsync(int id, UpdateUserRequestDto request);
    Task DeleteAsync(int id);
    Task<User> RegisterAsync(string name, string email, string password, string role);
    Task<User?> ValidateUserAsync(string email, string password);
}