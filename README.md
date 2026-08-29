[![](https://img.shields.io/nuget/v/Soenneker.Redis.Util.Server.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Redis.Util.Server/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.redis.util.server/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.redis.util.server/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Redis.Util.Server.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Redis.Util.Server/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.redis.util.server/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.redis.util.server/actions/workflows/codeql.yml)

# Soenneker.Redis.Util.Server

A utility library that allows for Redis Server operations Warning - all of the methods in here are generally quite heavy and only should be used during special circumstances. Scoped IoC.

## Install

```bash
dotnet add package Soenneker.Redis.Util.Server
```

## Quick start

```csharp
using Soenneker.Redis.Util.Server.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddRedisServerUtilAsSingleton();
```

Adds `IRedisServerUtil` as a singleton service.

## What you get

- `IRedisServerUtil` — A utility library that allows for Redis Server operations Warning - all of the methods in here are generally quite heavy and only should be used during special circumstances. Scoped IoC.
- `RedisServerUtilRegistrar` — A utility library for Redis server client accessibility.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IRedisServerUtil.GetKeyValuesByPrefix(cacheKey, prefix, cancellationToken)` | Deserializes the results and builds a dictionary with the keys and values. | A task whose result is the requested dictionary. |
| `IRedisServerUtil.GetKeyValuesByPrefix(redisKeyPrefix, cancellationToken)` | Deserializes the results and builds a dictionary with the keys and values. | A task whose result is the requested dictionary. |
| `IRedisServerUtil.GetKeysByPrefixList(cacheKey, prefix, cancellationToken)` | Immediately resolves the Async IEnumerable. Gets all keys (not values) that begin with the prefix. | The matching keys as a materialized collection. |
| `IRedisServerUtil.GetKeysByPrefix(redisKeyPrefix, cancellationToken)` | Gets all keys (not values) that begin with the prefix. | A task whose result is the requested async Enumerable. |
| `IRedisServerUtil.RemoveByScan(redisKeyPrefix, shouldRemove, batchSize, cancellationToken)` | Scans primary Redis endpoints and removes matching keys in pipelined batches. | The number of keys removed. |
| `IRedisServerUtil.GetKeysByPrefixList(redisKeyPrefix, cancellationToken)` | Wraps `GetKeysByPrefix(string, CancellationToken)`. Base method for `GetKeysByPrefixList(string, CancellationToken)`. Immediately resolves the Async IEnumerable. | The matching keys as a materialized collection. |
| `IRedisServerUtil.RemoveByPrefix(redisPrefixKey, fireAndForget, cancellationToken)` | Removes all keys that begin with the prefix. | A task that completes when the by prefix removal is complete. |
| `IRedisServerUtil.Flush(cancellationToken)` | Flushes redis Server. | A task that completes when the flush operation is complete. |
| `RedisServerUtilRegistrar.AddRedisServerUtilAsSingleton(services)` | Adds `IRedisServerUtil` as a singleton service. | The same service collection, so additional registrations can be chained. |
| `RedisServerUtilRegistrar.AddRedisServerUtilAsScoped(services)` | Registers Redis Server Util with a scoped lifetime. | The same service collection, so additional registrations can be chained. |

## Important behavior

- `IRedisServerUtil.GetKeysByPrefix(redisKeyPrefix, cancellationToken)`: Do not include asterisk!.
- `IRedisServerUtil.RemoveByPrefix(redisPrefixKey, fireAndForget, cancellationToken)`: Do not include asterisk!.

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
