using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Testing;
using Xunit;

namespace Microsoft.Azure.Commands.RedisCache.Test.ScenarioTests
{
    public class RedisCacheTests : RedisCacheTestsBase
    {
        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestRedisCache()
        {
            RunPowerShellTest("Test-RedisCache");
        }       
    }
}
