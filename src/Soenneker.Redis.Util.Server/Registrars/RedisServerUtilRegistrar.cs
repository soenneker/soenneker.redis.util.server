using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Redis.Client.Server.Registrars;
using Soenneker.Redis.Util.Registrars;
using Soenneker.Redis.Util.Server.Abstract;

namespace Soenneker.Redis.Util.Server.Registrars;

/// <summary>
/// Registers server-level Redis utilities and their transport dependencies.
/// </summary>
public static class RedisServerUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IRedisServerUtil"/> and all Redis dependencies as singleton services.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddRedisServerUtilAsSingleton(this IServiceCollection services)
    {
        services.AddRedisServerClientAsSingleton().AddRedisUtilAsSingleton().TryAddSingleton<IRedisServerUtil, RedisServerUtil>();

        return services;
    }

    /// <summary>
    /// Adds a scoped <see cref="IRedisServerUtil"/> while retaining singleton Redis client transports.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddRedisServerUtilAsScoped(this IServiceCollection services)
    {
        services.AddRedisServerClientAsSingleton().AddRedisUtilAsScoped().TryAddScoped<IRedisServerUtil, RedisServerUtil>();

        return services;
    }
}
