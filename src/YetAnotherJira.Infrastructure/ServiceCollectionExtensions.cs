using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Services;
using YetAnotherJira.Infrastructure.Services;

namespace YetAnotherJira.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TicketDbContext>(options => options
            .UseNpgsql(configuration.GetConnectionString("YaJiraDb"))
            .UseSnakeCaseNamingConvention());

        services.AddScoped<ITicketDbContext>(provider => provider.GetRequiredService<TicketDbContext>());
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}