using Soenneker.Redis.Util.Server.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Redis.Util.Server.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class RedisServerUtilTests : HostedUnitTest
{
    public RedisServerUtilTests(Host host) : base(host)
    {
    }

    [Test]
    public async System.Threading.Tasks.ValueTask Flush_should_flush()
    {
        var redisClient = Resolve<IRedisServerUtil>();

       await redisClient.Flush(CancellationToken);
    }
}