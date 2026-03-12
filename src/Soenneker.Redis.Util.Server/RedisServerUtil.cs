using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Redis.Client.Server.Abstract;
using Soenneker.Redis.Util.Abstract;
using Soenneker.Redis.Util.Server.Abstract;
using Soenneker.Redis.Util.Server.Utils;
using StackExchange.Redis;

namespace Soenneker.Redis.Util.Server;

/// <inheritdoc cref="IRedisServerUtil"/>
public sealed class RedisServerUtil : IRedisServerUtil
{
    private readonly ILogger<RedisServerUtil> _logger;
    private readonly IRedisUtil _redisUtil;
    private readonly IRedisServerClient _serverClient;

    public RedisServerUtil(ILogger<RedisServerUtil> logger, IRedisUtil redisUtil, IRedisServerClient serverClient)
    {
        _serverClient = serverClient;
        _logger = logger;
        _redisUtil = redisUtil;
    }

    public async ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string redisKeyPrefix, CancellationToken cancellationToken = default) where T : class
    {
        List<string>? keys = await GetKeyStringsByPrefixList(redisKeyPrefix, cancellationToken)
            .NoSync();

        if (keys is null)
            return null;

        if (keys.Count == 0)
            return new Dictionary<string, T>(0);

        var dictionary = new Dictionary<string, T>(keys.Count);

        foreach (string redisKeyStr in keys)
        {
            T? result = await _redisUtil.Get<T>(redisKeyStr, cancellationToken)
                                        .NoSync();

            if (result is not null)
                dictionary[redisKeyStr] = result;
        }

        return dictionary;
    }

    public async ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string redisKeyPrefix,
        CancellationToken cancellationToken = default)
    {
        List<string>? keys = await GetKeyStringsByPrefixList(redisKeyPrefix, cancellationToken)
            .NoSync();

        if (keys is null)
            return null;

        if (keys.Count == 0)
            return new Dictionary<string, string>(0);

        var dictionary = new Dictionary<string, string>(keys.Count);

        foreach (string redisKeyStr in keys)
        {
            string? result = await _redisUtil.GetString(redisKeyStr, cancellationToken)
                                             .NoSync();

            if (result is not null)
                dictionary[redisKeyStr] = result;
        }

        return dictionary;
    }

    public async ValueTask<Dictionary<string, T>?> GetKeyValueHashesByPrefix<T>(string redisKeyPrefix, string field,
        CancellationToken cancellationToken = default) where T : class
    {
        List<string>? keys = await GetKeyStringsByPrefixList(redisKeyPrefix, cancellationToken)
            .NoSync();

        if (keys is null)
            return null;

        if (keys.Count == 0)
            return new Dictionary<string, T>(0);

        var dictionary = new Dictionary<string, T>(keys.Count);

        foreach (string redisKeyStr in keys)
        {
            T? result = await _redisUtil.GetHash<T>(redisKeyStr, field, cancellationToken)
                                        .NoSync();

            if (result is not null)
                dictionary[redisKeyStr] = result;
        }

        return dictionary;
    }

    public ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string cacheKey, string? prefix, CancellationToken cancellationToken = default)
        where T : class
    {
        // BuildKey(...) should return a prefix without '*'. We add wildcard exactly once.
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix);
        return GetKeyValuesByPrefix<T>(redisKeyPrefix, cancellationToken);
    }

    public ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string cacheKey, string? prefix,
        CancellationToken cancellationToken = default)
    {
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix);
        return GetKeyValuesByPrefixWithoutDeserialization(redisKeyPrefix, cancellationToken);
    }

    public async ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string redisKeyPrefix, CancellationToken cancellationToken = default)
    {
        string pattern = EnsureWildcard(redisKeyPrefix);

        RedisValue redisValue = pattern; // implicit conversion
        return (await _serverClient.Get(cancellationToken)
                                   .NoSync()).KeysAsync(pattern: redisValue);
    }

    public ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string cacheKey, string? prefix, CancellationToken cancellationToken = default)
    {
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix);
        return GetKeysByPrefix(redisKeyPrefix, cancellationToken);
    }

    public async ValueTask<List<RedisKey>?> GetKeysByPrefixList(string redisKeyPrefix, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<RedisKey>? result = await GetKeysByPrefix(redisKeyPrefix, cancellationToken)
            .NoSync();

        if (result is null)
            return null;

        var list = new List<RedisKey>();

        await foreach (RedisKey item in result.ConfigureAwait(false)
                                              .WithCancellation(cancellationToken))
            list.Add(item);

        return list;
    }

    public ValueTask<List<RedisKey>?> GetKeysByPrefixList(string cacheKey, string? prefix, CancellationToken cancellationToken = default)
    {
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix);
        return GetKeysByPrefixList(redisKeyPrefix, cancellationToken);
    }

    public ValueTask RemoveByPrefix(string cacheKey, string? prefix, bool fireAndForget = false, CancellationToken cancellationToken = default)
    {
        string redisPrefixKey = RedisUtil.BuildKey(cacheKey, prefix);
        return RemoveByPrefix(redisPrefixKey, fireAndForget, cancellationToken);
    }

    public async ValueTask RemoveByPrefix(string redisPrefixKey, bool fireAndForget = false, CancellationToken cancellationToken = default)
    {
        List<string>? keys = await GetKeyStringsByPrefixList(redisPrefixKey, cancellationToken)
            .NoSync();

        if (keys is null || keys.Count == 0)
            return;

        Log.RemovingKeys(_logger, EnsureWildcard(redisPrefixKey));

        foreach (string keyStr in keys)
        {
            try
            {
                await _redisUtil.Remove(keyStr, fireAndForget, cancellationToken)
                                .NoSync();
            }
            catch (Exception e)
            {
                Log.RemoveError(_logger, EnsureWildcard(redisPrefixKey), e);
            }
        }
    }

    public async ValueTask Flush(CancellationToken cancellationToken = default)
    {
        Log.Flushing(_logger);

        try
        {
            IServer client = await _serverClient.Get(cancellationToken)
                                                .NoSync();

            await client.FlushAllDatabasesAsync()
                        .WaitAsync(cancellationToken)
                        .NoSync();

            Log.FlushedOk(_logger);
        }
        catch (Exception e)
        {
            Log.FlushError(_logger, e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string EnsureWildcard(string prefixOrPattern)
    {
        if (prefixOrPattern.Length > 0 && prefixOrPattern[^1] == '*')
            return prefixOrPattern;

        return prefixOrPattern + '*';
    }

    private async ValueTask<List<string>?> GetKeyStringsByPrefixList(string redisKeyPrefix, CancellationToken cancellationToken)
    {
        IAsyncEnumerable<RedisKey>? result = await GetKeysByPrefix(redisKeyPrefix, cancellationToken)
            .NoSync();

        if (result is null)
            return null;

        var list = new List<string>(64);

        await foreach (RedisKey item in result.ConfigureAwait(false)
                                              .WithCancellation(cancellationToken))
            list.Add(item.ToString());

        return list;
    }
}