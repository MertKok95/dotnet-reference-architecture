using DotNetCoreLab.Domain.Entities;

namespace DotNetCoreLab.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}