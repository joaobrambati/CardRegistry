using Domain.Services;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDI(this IServiceCollection services)
    {
        services.AddScoped<ICartaoRepository, CartaoRepository>();
        services.AddScoped<IHashService, HashService>();

        return services;
    }
}
