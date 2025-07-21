using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Redis.Client.Server.Abstract;
using Soenneker.Redis.Util.Abstract;
using Soenneker.Redis.Util.Server.Abstract;
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
        List<RedisKey>? keys = await GetKeysByPrefixList(redisKeyPrefix, cancellationToken).NoSync();

        if (keys == null)
            return null;

        var dictionary = new Dictionary<string, T>();

        foreach (RedisKey redisKey in keys)
        {
            var redisKeyStr = redisKey.ToString();

            T? result = await _redisUtil.Get<T>(redisKeyStr, cancellationToken).NoSync();

            if (result != null)
                dictionary.TryAdd(redisKeyStr, result);
        }

        return dictionary;
    }

    public async ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string redisKeyPrefix, CancellationToken cancellationToken = default)
    {
        List<RedisKey>? keys = await GetKeysByPrefixList(redisKeyPrefix, cancellationToken).NoSync();

        if (keys == null)
            return null;

        var dictionary = new Dictionary<string, string>();

        foreach (RedisKey redisKey in keys)
        {
            var redisKeyStr = redisKey.ToString();

            string? result = await _redisUtil.GetString(redisKeyStr, cancellationToken).NoSync();

            if (result != null)
                dictionary.TryAdd(redisKeyStr, result);
        }

        return dictionary;
    }

    public async ValueTask<Dictionary<string, T>?> GetKeyValueHashesByPrefix<T>(string redisKeyPrefix, string field, CancellationToken cancellationToken = default) where T : class
    {
        List<RedisKey>? keys = await GetKeysByPrefixList(redisKeyPrefix, cancellationToken).NoSync();

        if (keys == null)
            return null;

        var dictionary = new Dictionary<string, T>();

        foreach (RedisKey redisKey in keys)
        {
            var redisKeyStr = redisKey.ToString();

            T? result = await _redisUtil.GetHash<T>(redisKeyStr, field, cancellationToken).NoSync();

            if (result != null)
                dictionary.TryAdd(redisKeyStr, result);
        }

        return dictionary;
    }

    public ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string cacheKey, string? prefix, CancellationToken cancellationToken = default) where T : class
    {
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix) + '*';

        return GetKeyValuesByPrefix<T>(redisKeyPrefix, cancellationToken);
    }

    public ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string cacheKey, string? prefix, CancellationToken cancellationToken = default)
    {
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix) + '*';

        return GetKeyValuesByPrefixWithoutDeserialization(redisKeyPrefix, cancellationToken);
    }

    public async ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string redisKeyPrefix, CancellationToken cancellationToken = default)
    {
        var keyPattern = $"{redisKeyPrefix}*";

        var redisValue = new RedisValue(keyPattern);

        return (await _serverClient.Get(cancellationToken).NoSync()).KeysAsync(pattern: redisValue);
    }

    public ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string cacheKey, string? prefix, CancellationToken cancellationToken = default)
    {
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix) + '*';
        return GetKeysByPrefix(redisKeyPrefix, cancellationToken);
    }

    public async ValueTask<List<RedisKey>?> GetKeysByPrefixList(string redisKeyPrefix, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<RedisKey>? result = await GetKeysByPrefix(redisKeyPrefix, cancellationToken).NoSync();

        if (result == null)
            return null;

        var list = new List<RedisKey>();

        await foreach (RedisKey item in result.ConfigureAwait(false).WithCancellation(cancellationToken))
        {
            list.Add(item);
        }

        return list;
    }

    public ValueTask<List<RedisKey>?> GetKeysByPrefixList(string cacheKey, string? prefix, CancellationToken cancellationToken = default)
    {
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix) + '*';

        return GetKeysByPrefixList(redisKeyPrefix, cancellationToken);
    }

    public ValueTask RemoveByPrefix(string cacheKey, string? prefix, bool fireAndForget = false, CancellationToken cancellationToken = default)
    {
        string redisPrefixKey = RedisUtil.BuildKey(cacheKey, prefix) + '*';

        return RemoveByPrefix(redisPrefixKey, fireAndForget, cancellationToken);
    }

    public async ValueTask RemoveByPrefix(string redisPrefixKey, bool fireAndForget = false, CancellationToken cancellationToken = default)
    {
        List<RedisKey>? keys = await GetKeysByPrefixList(redisPrefixKey, cancellationToken).NoSync();

        if (keys == null)
            return;

        _logger.LogWarning(">> REDIS: Removing keys matching: {key} ...", redisPrefixKey);

        foreach (RedisKey key in keys)
        {
            try
            {
                var keyStr = key.ToString();

                await _redisUtil.Remove(keyStr, fireAndForget, cancellationToken).NoSync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, ">> REDIS: Error removing keys matching: {key}", redisPrefixKey);
            }
        }
    }

    public async ValueTask Flush(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(">> RedisServerClient: Flushing...");

        try
        {
            IServer client = await _serverClient.Get(cancellationToken).NoSync();

            await client.FlushAllDatabasesAsync().WaitAsync(cancellationToken).NoSync();

            _logger.LogDebug(">> RedisServerClient: Flushed successfully");
        }
        catch (Exception e)
        {
            _logger.LogError(e, ">> RedisServerClient: Error flushing redis server");
        }
    }
}