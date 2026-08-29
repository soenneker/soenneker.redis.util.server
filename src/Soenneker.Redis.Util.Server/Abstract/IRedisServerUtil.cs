using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System;
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
    /// <typeparam name="T">Type of value handled by the Redis Server.</typeparam>
    /// <param name="cacheKey">Base cache key used to build the Redis key.</param>
    /// <param name="prefix">Prefix prepended to generated keys or names.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested dictionary.</returns>
    [Pure]
    ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string cacheKey, string? prefix, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets key values by prefix without deserialization.
    /// </summary>
    /// <param name="cacheKey">Base cache key used to build the Redis key.</param>
    /// <param name="prefix">Prefix prepended to generated keys or names.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested dictionary.</returns>
    [Pure]
    ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string cacheKey, string? prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes the results and builds a dictionary with the keys and values.
    /// </summary>
    /// <typeparam name="T">Type of value handled by the Redis Server.</typeparam>
    /// <param name="redisKeyPrefix">Prefix prepended to Redis keys.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested dictionary.</returns>
    [Pure]
    ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string redisKeyPrefix, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets key values by prefix without deserialization.
    /// </summary>
    /// <param name="redisKeyPrefix">Prefix prepended to Redis keys.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested dictionary.</returns>
    [Pure]
    ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string redisKeyPrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets key value hashes by prefix.
    /// </summary>
    /// <typeparam name="T">Type of value handled by the Redis Server.</typeparam>
    /// <param name="redisKeyPrefix">Prefix prepended to Redis keys.</param>
    /// <param name="field">Hash field to read, write, or remove.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested dictionary.</returns>
    [Pure]
    ValueTask<Dictionary<string, T>?> GetKeyValueHashesByPrefix<T>(string redisKeyPrefix, string field, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Immediately resolves the Async IEnumerable. Gets all keys (not values) that begin with the prefix.
    /// </summary>
    /// <param name="cacheKey">Base cache key used to build the Redis key.</param>
    /// <param name="prefix">Prefix prepended to generated keys or names.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the collection returned by get Keys By Prefix List.</returns>
    [Pure]
    ValueTask<List<RedisKey>?> GetKeysByPrefixList(string cacheKey, string? prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets keys by prefix.
    /// </summary>
    /// <param name="cacheKey">Base cache key used to build the Redis key.</param>
    /// <param name="prefix">Prefix prepended to generated keys or names.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested async Enumerable.</returns>
    [Pure]
    ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string cacheKey, string? prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all keys (not values) that begin with the prefix.
    /// </summary>
    /// <param name="redisKeyPrefix">Prefix prepended to Redis keys.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested async Enumerable.</returns>
    /// <remarks>Do not include asterisk!</remarks>
    [Pure]
    ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string redisKeyPrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans primary Redis endpoints and removes matching keys in pipelined batches.
    /// </summary>
    /// <param name="redisKeyPrefix">An optional key prefix used to narrow the scan.</param>
    /// <param name="shouldRemove">Determines whether a scanned key should be removed.</param>
    /// <param name="batchSize">The maximum number of delete operations issued in one pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of keys removed.</returns>
    ValueTask<long> RemoveByScan(string? redisKeyPrefix, Func<RedisKey, bool> shouldRemove, int batchSize = 500,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Wraps <see cref="GetKeysByPrefix(string, CancellationToken)"/>. Base method for <see cref="GetKeysByPrefixList(string, CancellationToken)"/>.<para/>
    /// Immediately resolves the Async IEnumerable.
    /// </summary>
    /// <param name="redisKeyPrefix">Prefix prepended to Redis keys.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the collection returned by get Keys By Prefix List.</returns>
    [Pure]
    ValueTask<List<RedisKey>?> GetKeysByPrefixList(string redisKeyPrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes by prefix.
    /// </summary>
    /// <param name="cacheKey">Base cache key used to build the Redis key.</param>
    /// <param name="prefix">Prefix prepended to generated keys or names.</param>
    /// <param name="fireAndForget">Whether fire and forget.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the by prefix removal is complete.</returns>
    ValueTask RemoveByPrefix(string cacheKey, string? prefix, bool fireAndForget = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all keys that begin with the prefix.
    /// </summary>
    /// <param name="redisPrefixKey">Redis Prefix Key for the remove by prefix operation.</param>
    /// <param name="fireAndForget">Whether fire and forget.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the by prefix removal is complete.</returns>
    /// <remarks>Do not include asterisk!</remarks>
    ValueTask RemoveByPrefix(string redisPrefixKey, bool fireAndForget = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Flushes redis Server.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the flush operation is complete.</returns>
    ValueTask Flush(CancellationToken cancellationToken = default);
}
