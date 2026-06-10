using System.Reflection;
using FluentValidation;
using GameCatalogue.Application.Behaviours;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GameCatalogue.Application;

/// <summary>
/// Dependency injection registrations for the Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR, FluentValidation validators and pipeline behaviours.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
