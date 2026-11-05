using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using DotNetCoreLab.Application.Common.Interfaces;
using DotNetCoreLab.Application.Common.Models;
using DotNetCoreLab.Application.Features.Users.Commands.CreateUser;
using DotNetCoreLab.Application.Features.Users.Commands.UpdateUser;
using DotNetCoreLab.Application.Features.Users.Dtos;
using DotNetCoreLab.Application.Features.Users.Queries.GetUsers;
using DotNetCoreLab.Domain.Entities;
using DotNetCoreLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreLab.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserResponseDto>> GetAllAsync(UserQueryParameters parameters)
    {
        var query = _userRepository.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            query = query.Where(u => u.Name.ToLower().Contains(parameters.SearchTerm.ToLower()) || 
                                     u.Email.ToLower().Contains(parameters.SearchTerm.ToLower()));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(); 

        var dtos = _mapper.Map<List<UserResponseDto>>(users);

        return new PagedResult<UserResponseDto>
        {
            Items = dtos, TotalCount = totalCount, PageNumber = parameters.PageNumber, PageSize = parameters.PageSize
        };
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;
        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> CreateAsync(CreateUserRequestDto request)
    {
        var newUser = _mapper.Map<User>(request);
        
        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserResponseDto>(newUser);
    }

    public async Task UpdateAsync(int id, UpdateUserRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException($"ID'si {id} olan kullanıcı bulunamadı.");

        _mapper.Map(request, user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException($"ID'si {id} olan kullanıcı bulunamadı.");

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<User> RegisterAsync(string name, string email, string password, string role)
    {
        var mevcutKullanici = await _userRepository.AsQueryable()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (mevcutKullanici != null)
            throw new Exception("Bu e-posta adresi zaten kayıtlı!");

        var yeniKullanici = new User
        {
            Name = name,
            Email = email,
            PasswordHash = HashPassword(password),
            Role = string.IsNullOrWhiteSpace(role) ? "User" : role
        };

        await _userRepository.AddAsync(yeniKullanici);
        await _userRepository.SaveChangesAsync();

        return yeniKullanici;
    }

    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        var user = await _userRepository.AsQueryable()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
            return null;

        var girilenSifreHash = HashPassword(password);
        if (user.PasswordHash != girilenSifreHash)
            return null;

        return user;
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}