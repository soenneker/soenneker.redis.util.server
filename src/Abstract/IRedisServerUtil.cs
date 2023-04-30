using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
    ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string cacheKey, string? prefix) where T : class;

    [Pure]
    ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string cacheKey, string? prefix);

    /// <summary>
    /// Deserializes the results and builds a dictionary with the keys and values. 
    /// </summary>
    [Pure]
    ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string redisKeyPrefix) where T : class;

    [Pure]
    ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string redisKeyPrefix);

    [Pure]
    ValueTask<Dictionary<string, T>?> GetKeyValueHashesByPrefix<T>(string redisKeyPrefix, string field) where T : class;

    /// <summary>
    /// Immediately resolves the Async IEnumerable. Gets all keys (not values) that begin with the prefix.
    /// </summary>
    [Pure]
    ValueTask<List<RedisKey>?> GetKeysByPrefixList(string cacheKey, string? prefix);

    /// <inheritdoc cref="GetKeysByPrefix(string)"/>
    [Pure]
    ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string cacheKey, string? prefix);

    /// <summary>
    /// Gets all keys (not values) that begin with the prefix.
    /// </summary>
    /// <remarks>Do not include asterisk!</remarks>
    [Pure]
    ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string redisKeyPrefix);

    /// <summary> Wraps <see cref="GetKeysByPrefix(string)"/>. Base method for <see cref="GetKeysByPrefixList(string)"/>.<para/>
    /// Immediately resolves the Async IEnumerable.</summary>
    [Pure]
    ValueTask<List<RedisKey>?> GetKeysByPrefixList(string redisKeyPrefix);

    /// <inheritdoc cref="RemoveByPrefix(string, bool)"/>
    ValueTask RemoveByPrefix(string cacheKey, string? prefix = null, bool fireAndForget = false);

    /// <summary>
    /// Removes all keys that begin with the prefix.
    /// </summary>
    /// <remarks>Do not include asterisk!</remarks>
    ValueTask RemoveByPrefix(string redisPrefixKey, bool fireAndForget = false);

    ValueTask Flush();
}