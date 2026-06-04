using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using X_Chang.API.Models;
using X_Chang.CORE.Interfaces;
using X_Chang.CORE.Services;
using X_Chang.CORE.Settings;

namespace X_Chang.CORE.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ExchangeDivisasDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(3)));

        services.Configure<SessionSettings>(configuration.GetSection("SessionSettings"));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrdenService, OrdenService>();
        services.AddScoped<IOfertaService, OfertaService>();

        return services;
    }
}
