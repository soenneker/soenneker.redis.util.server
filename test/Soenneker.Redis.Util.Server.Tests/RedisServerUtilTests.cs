using Soenneker.Redis.Util.Server.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Redis.Util.Server.Tests;

[Collection("Collection")]
public class RedisServerUtilTests : FixturedUnitTest
{
    public RedisServerUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public async System.Threading.Tasks.ValueTask Flush_should_flush()
    {
        var redisClient = Resolve<IRedisServerUtil>();

       await redisClient.Flush(CancellationToken);
    }
}