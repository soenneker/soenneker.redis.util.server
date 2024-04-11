using System;
using System.Collections.Generic;
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
public class RedisServerUtil : IRedisServerUtil
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

    public async ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string redisKeyPrefix) where T : class
    {
        List<RedisKey>? keys = await GetKeysByPrefixList(redisKeyPrefix).NoSync();

        if (keys == null)
            return null;

        var dictionary = new Dictionary<string, T>();

        foreach (RedisKey redisKey in keys)
        {
            var redisKeyStr = redisKey.ToString();

            var result = await _redisUtil.Get<T>(redisKeyStr).NoSync();

            if (result != null)
                dictionary.TryAdd(redisKeyStr, result);
        }

        return dictionary;
    }

    public async ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string redisKeyPrefix)
    {
        List<RedisKey>? keys = await GetKeysByPrefixList(redisKeyPrefix).NoSync();

        if (keys == null)
            return null;

        var dictionary = new Dictionary<string, string>();

        foreach (RedisKey redisKey in keys)
        {
            var redisKeyStr = redisKey.ToString();

            string? result = await _redisUtil.GetString(redisKeyStr).NoSync();

            if (result != null)
                dictionary.TryAdd(redisKeyStr, result);
        }

        return dictionary;
    }

    public async ValueTask<Dictionary<string, T>?> GetKeyValueHashesByPrefix<T>(string redisKeyPrefix, string field) where T : class
    {
        List<RedisKey>? keys = await GetKeysByPrefixList(redisKeyPrefix).NoSync();

        if (keys == null)
            return null;

        var dictionary = new Dictionary<string, T>();

        foreach (RedisKey redisKey in keys)
        {
            var redisKeyStr = redisKey.ToString();

            T? result = await _redisUtil.GetHash<T>(redisKeyStr, field).NoSync();

            if (result != null)
                dictionary.TryAdd(redisKeyStr, result);
        }

        return dictionary;
    }

    public ValueTask<Dictionary<string, T>?> GetKeyValuesByPrefix<T>(string cacheKey, string? prefix) where T : class
    {
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix) + '*';

        return GetKeyValuesByPrefix<T>(redisKeyPrefix);
    }

    public ValueTask<Dictionary<string, string>?> GetKeyValuesByPrefixWithoutDeserialization(string cacheKey, string? prefix)
    {
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix) + '*';

        return GetKeyValuesByPrefixWithoutDeserialization(redisKeyPrefix);
    }

    public async ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string redisKeyPrefix)
    {
        var keyPattern = $"{redisKeyPrefix}*";

        var redisValue = new RedisValue(keyPattern);

        IAsyncEnumerable<RedisKey> keys = (await _serverClient.Get().NoSync()).KeysAsync(pattern: redisValue);

        return keys;
    }

    public ValueTask<IAsyncEnumerable<RedisKey>?> GetKeysByPrefix(string cacheKey, string? prefix)
    {
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix) + '*';
        return GetKeysByPrefix(redisKeyPrefix);
    }

    public async ValueTask<List<RedisKey>?> GetKeysByPrefixList(string redisKeyPrefix)
    {
        IAsyncEnumerable<RedisKey>? result = await GetKeysByPrefix(redisKeyPrefix).NoSync();

        if (result == null)
            return null;

        var list = new List<RedisKey>();

        await foreach (RedisKey item in result.ConfigureAwait(false))
        {
            list.Add(item);
        }

        return list;
    }

    public ValueTask<List<RedisKey>?> GetKeysByPrefixList(string cacheKey, string? prefix)
    {
        string redisKeyPrefix = RedisUtil.BuildKey(cacheKey, prefix) + '*';

        return GetKeysByPrefixList(redisKeyPrefix);
    }

    public ValueTask RemoveByPrefix(string cacheKey, string? prefix, bool fireAndForget = false)
    {
        string redisPrefixKey = RedisUtil.BuildKey(cacheKey, prefix) + '*';

        return RemoveByPrefix(redisPrefixKey, fireAndForget);
    }

    public async ValueTask RemoveByPrefix(string redisPrefixKey, bool fireAndForget = false)
    {
        List<RedisKey>? keys = await GetKeysByPrefixList(redisPrefixKey).NoSync();

        if (keys == null)
            return;

        _logger.LogWarning(">> REDIS: Removing keys matching: {key} ...", redisPrefixKey);

        foreach (RedisKey key in keys)
        {
            try
            {
                var keyStr = key.ToString();

                await _redisUtil.Remove(keyStr, fireAndForget).NoSync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, ">> REDIS: Error removing keys matching: {key}", redisPrefixKey);
            }
        }
    }

    public async ValueTask Flush()
    {
        _logger.LogWarning(">> RedisServerClient: Flushing...");

        try
        {
            await (await _serverClient.Get().NoSync()).FlushAllDatabasesAsync().NoSync();

            _logger.LogDebug(">> RedisServerClient: Flushed successfully");
        }
        catch (Exception e)
        {
            _logger.LogError(e, ">> RedisServerClient: Error flushing redis server");
        }
    }
}