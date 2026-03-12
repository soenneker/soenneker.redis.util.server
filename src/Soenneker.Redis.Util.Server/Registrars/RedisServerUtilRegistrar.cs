using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Redis.Client.Server.Registrars;
using Soenneker.Redis.Util.Registrars;
using Soenneker.Redis.Util.Server.Abstract;

namespace Soenneker.Redis.Util.Server.Registrars;

/// <summary>
/// A utility library for Redis server client accessibility
/// </summary>
public static class RedisServerUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IRedisServerUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddRedisServerUtilAsSingleton(this IServiceCollection services)
    {
        services.AddRedisServerClientAsSingleton().AddRedisUtilAsSingleton().TryAddSingleton<IRedisServerUtil, RedisServerUtil>();

        return services;
    }

    public static IServiceCollection AddRedisServerUtilAsScoped(this IServiceCollection services)
    {
        services.AddRedisServerClientAsSingleton().AddRedisUtilAsScoped().TryAddScoped<IRedisServerUtil, RedisServerUtil>();

        return services;
    }
}