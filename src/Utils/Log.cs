using Microsoft.Extensions.Logging;
using System;

namespace Soenneker.Redis.Util.Server.Utils;

internal static class Log
{
    private static readonly Action<ILogger, string, Exception?> _removingKeys = LoggerMessage.Define<string>(LogLevel.Warning,
        new EventId(1001, nameof(RemovingKeys)), ">> REDIS: Removing keys matching: {key} ...");

    private static readonly Action<ILogger, string, Exception?> _removeError = LoggerMessage.Define<string>(LogLevel.Error,
        new EventId(1002, nameof(RemoveError)), ">> REDIS: Error removing keys matching: {key}");

    private static readonly Action<ILogger, Exception?> _flushing =
        LoggerMessage.Define(LogLevel.Warning, new EventId(1003, nameof(Flushing)), ">> RedisServerClient: Flushing...");

    private static readonly Action<ILogger, Exception?> _flushedOk =
        LoggerMessage.Define(LogLevel.Debug, new EventId(1004, nameof(FlushedOk)), ">> RedisServerClient: Flushed successfully");

    private static readonly Action<ILogger, Exception?> _flushError = LoggerMessage.Define(LogLevel.Error, new EventId(1005, nameof(FlushError)),
        ">> RedisServerClient: Error flushing redis server");

    public static void RemovingKeys(ILogger logger, string key) => _removingKeys(logger, key, null);

    public static void RemoveError(ILogger logger, string key, Exception exception) => _removeError(logger, key, exception);

    public static void Flushing(ILogger logger) => _flushing(logger, null);

    public static void FlushedOk(ILogger logger) => _flushedOk(logger, null);

    public static void FlushError(ILogger logger, Exception exception) => _flushError(logger, exception);
}