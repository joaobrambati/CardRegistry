using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDI(this IServiceCollection services)
    {
        services.AddScoped<ICartaoService, CartaoService>();

        return services;
    }
}
