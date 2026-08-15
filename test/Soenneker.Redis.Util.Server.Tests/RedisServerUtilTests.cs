using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Soenneker.Redis.Client.Abstract;
using Soenneker.Redis.Util.Server.Abstract;
using Soenneker.Tests.HostedUnit;
using StackExchange.Redis;

namespace Soenneker.Redis.Util.Server.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class RedisServerUtilTests : HostedUnitTest
{
    private readonly IRedisServerUtil _redisServerUtil;
    private readonly IRedisClient _redisClient;

    public RedisServerUtilTests(Host host) : base(host)
    {
        _redisServerUtil = Resolve<IRedisServerUtil>(true);
        _redisClient = Resolve<IRedisClient>(true);
    }

    [Test]
    public async System.Threading.Tasks.ValueTask Flush_should_flush()
    {
        var redisClient = Resolve<IRedisServerUtil>();

       await redisClient.Flush(System.Threading.CancellationToken.None);
    }

    [Test]
    public async Task RemoveByScan_should_filter_and_delete_in_batches()
    {
        string prefix = $"redis-server-util-test:{Guid.NewGuid():N}:";
        RedisKey removedOne = $"{prefix}remove:1";
        RedisKey removedTwo = $"{prefix}remove:2";
        RedisKey retained = $"{prefix}retain";
        ConnectionMultiplexer connection = await _redisClient.Get(CancellationToken.None);
        IDatabase database = connection.GetDatabase();

        try
        {
            await database.StringSetAsync(removedOne, "one");
            await database.StringSetAsync(removedTwo, "two");
            await database.StringSetAsync(retained, "retained");

            long removed = await _redisServerUtil.RemoveByScan(prefix, key => key != retained, 1, CancellationToken.None);

            removed.Should().Be(2);
            (await database.KeyExistsAsync(removedOne)).Should().BeFalse();
            (await database.KeyExistsAsync(removedTwo)).Should().BeFalse();
            (await database.KeyExistsAsync(retained)).Should().BeTrue();
        }
        finally
        {
            await database.KeyDeleteAsync([removedOne, removedTwo, retained]);
        }
    }
}
