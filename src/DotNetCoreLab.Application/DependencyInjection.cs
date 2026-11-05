using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using DotNetCoreLab.Application.Services;
using DotNetCoreLab.Application.Common.Behaviors;
using DotNetCoreLab.Application.Common.Interfaces;

namespace DotNetCoreLab.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        services.AddAutoMapper(cfg => 
        {
            cfg.AddMaps(typeof(DependencyInjection).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(RequestValidationPipe<,>));
        });

        return services;
    }
}