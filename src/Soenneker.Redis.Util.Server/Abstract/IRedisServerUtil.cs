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

    /// <summary>
    /// Gets key values by prefix without deserialization.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="prefix">The prefix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    [Pure]
    ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string cacheKey, string? prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes the results and builds a dictionary with the keys and values. 
    /// </summary>
    [Pure]
    ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string redisKeyPrefix, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets key values by prefix without deserialization.
    /// </summary>
    /// <param name="redisKeyPrefix">The redis key prefix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    [Pure]
    ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string redisKeyPrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets key value hashes by prefix.
    /// </summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="redisKeyPrefix">The redis key prefix.</param>
    /// <param name="field">The field.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    [Pure]
    ValueTask<Dictionary<string, T>?> GetKeyValueHashesByPrefix<T>(string redisKeyPrefix, string field, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Immediately resolves the Async IEnumerable. Gets all keys (not values) that begin with the prefix.
    /// </summary>
    [Pure]
    ValueTask<List<RedisKey>?> GetKeysByPrefixList(string cacheKey, string? prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets keys by prefix.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="prefix">The prefix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
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
    /// Removes by prefix.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="prefix">The prefix.</param>
    /// <param name="fireAndForget">The fire and forget.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask RemoveByPrefix(string cacheKey, string? prefix, bool fireAndForget = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all keys that begin with the prefix.
    /// </summary>
    /// <remarks>Do not include asterisk!</remarks>
    ValueTask RemoveByPrefix(string redisPrefixKey, bool fireAndForget = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the flush operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask Flush(CancellationToken cancellationToken = default);
}