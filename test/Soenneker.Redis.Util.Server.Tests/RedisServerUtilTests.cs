using Soenneker.Redis.Util.Server.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.Redis.Util.Server.Tests;

[Collection("Collection")]
public class RedisServerUtilTests : FixturedUnitTest
{
    public RedisServerUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public async void Flush_should_flush()
    {
        var redisClient = Resolve<IRedisServerUtil>();

       await redisClient.Flush();
    }
}