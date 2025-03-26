using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Soenneker.Redis.Util.Server.Abstract;

/// <summary>
/// A utility library that allows for Redis Server operations <para/>
/// Warning - all of the methods in here are generally quite heavy and only should be used during special circumstances.<para/>
/// Scoped IoC
/// </summary>
public interface IRedisServerUtil
{
    /// <summary>
    /// Deserializes the results and builds a dictionary with the keys and values.
    /// </summary>
    [Pure]
    ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string cacheKey, string? prefix, CancellationToken cancellationToken = default) where T : class;

    [Pure]
    ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string cacheKey, string? prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes the results and builds a dictionary with the keys and values. 
    /// </summary>
    [Pure]
    ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string redisKeyPrefix, CancellationToken cancellationToken = default) where T : class;

    [Pure]
    ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string redisKeyPrefix, CancellationToken cancellationToken = default);

    [Pure]
    ValueTask<Dictionary<string, T>?> GetKeyValueHashesByPrefix<T>(string redisKeyPrefix, string field, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Immediately resolves the Async IEnumerable. Gets all keys (not values) that begin with the prefix.
    /// </summary>
    [Pure]
    ValueTask<List<RedisKey>?> GetKeysByPrefixList(string cacheKey, string? prefix, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="GetKeysByPrefix(string)"/>
    [Pure]
    ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string cacheKey, string? prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all keys (not values) that begin with the prefix.
    /// </summary>
    /// <remarks>Do not include asterisk!</remarks>
    [Pure]
    ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string redisKeyPrefix, CancellationToken cancellationToken = default);

    /// <summary> Wraps <see cref="GetKeysByPrefix(string)"/>. Base method for <see cref="GetKeysByPrefixList(string)"/>.<para/>
    /// Immediately resolves the Async IEnumerable.</summary>
    [Pure]
    ValueTask<List<RedisKey>?> GetKeysByPrefixList(string redisKeyPrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds the key before calling RemoveByPrefix
    /// <inheritdoc cref="RemoveByPrefix(string, bool)"/>
    /// </summary>
    ValueTask RemoveByPrefix(string cacheKey, string? prefix, bool fireAndForget = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all keys that begin with the prefix.
    /// </summary>
    /// <remarks>Do not include asterisk!</remarks>
    ValueTask RemoveByPrefix(string redisPrefixKey, bool fireAndForget = false, CancellationToken cancellationToken = default);

    ValueTask Flush(CancellationToken cancellationToken = default);
}