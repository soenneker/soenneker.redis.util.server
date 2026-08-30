[![](https://img.shields.io/nuget/v/Soenneker.Redis.Util.Server.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Redis.Util.Server/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.redis.util.server/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.redis.util.server/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Redis.Util.Server.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Redis.Util.Server/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.redis.util.server/build-and-test.yml?label=build%20and%20test&style=for-the-badge)](https://github.com/soenneker/soenneker.redis.util.server/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.redis.util.server/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.redis.util.server/actions/workflows/codeql.yml)

# Soenneker.Redis.Util.Server

Server-level Redis scanning, bulk lookup, bulk deletion, and database-flush operations.

These operations can scan or delete large portions of Redis. Keep them out of request hot paths and restrict access to trusted administrative workflows.

## Installation and registration

```bash
dotnet add package Soenneker.Redis.Util.Server
```

```csharp
using Soenneker.Redis.Util.Server.Registrars;

services.AddRedisServerUtilAsScoped();
```

The scoped registrar keeps the Redis client and server client singleton while scoping the utility wrappers. Disposing a scope therefore does not close the shared connection.

Configuration uses `Azure:Redis:ConnectionString`, as described by `Soenneker.Redis.Client`. The client enables administrative commands; the Redis credentials still need server permission for operations such as `FLUSHALL`.

## Read keys by prefix

```csharp
IReadOnlyDictionary<string, Order>? orders =
    await redisServer.GetKeyValuesByPrefix<Order>("orders", cancellationToken);

List<RedisKey>? keys =
    await redisServer.GetKeysByPrefixList("orders:pending", cancellationToken);
```

The utility appends a trailing `*` when one is absent. Prefix strings are Redis glob patterns, so characters such as `?`, `[`, and `]` retain their Redis pattern meaning. The prefix lookup methods use the server endpoint selected by `IRedisServerClient`; they are not a cluster-wide aggregation.

## Remove keys across writable endpoints

For cluster-aware cleanup, prefer `RemoveByScan`. It scans every connected non-replica endpoint and pipelines matching deletes:

```csharp
long removed = await redisServer.RemoveByScan(
    "sessions:",
    key => key.ToString().EndsWith(":expired", StringComparison.Ordinal),
    batchSize: 500,
    cancellationToken);
```

A `null` or empty prefix scans all keys, leaving `shouldRemove` as the only filter. Keep the predicate narrow and test it before using this against production data.

`RemoveByPrefix` is the older single-endpoint path. Its `fireAndForget` parameter queues individual removals through `IRedisUtil`; use `false` when completion matters.

## Flush

```csharp
await redisServer.Flush(cancellationToken);
```

`Flush` issues `FLUSHALL` to the selected server endpoint and destroys all databases on that server. The method logs server failures rather than returning a success result or rethrowing them, so verify the resulting state when an administrative workflow requires confirmation.
