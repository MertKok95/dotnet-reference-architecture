using AutoMapper;
using DotNetCoreLab.Application.Features.Users.Commands.CreateUser;
using DotNetCoreLab.Application.Features.Users.Commands.UpdateUser;
using DotNetCoreLab.Application.Features.Users.Dtos;

using DotNetCoreLab.Domain.Entities;

namespace DotNetCoreLab.Application.Features.Users.Profiles;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<CreateUserRequestDto, User>();
        CreateMap<UpdateUserRequestDto, User>();
        CreateMap<User, UserResponseDto>();
    }
}