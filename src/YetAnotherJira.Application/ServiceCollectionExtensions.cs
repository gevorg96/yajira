using MediatR;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherJira.Application.Behaviours;

namespace YetAnotherJira.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));
        
        return services;
    }
}