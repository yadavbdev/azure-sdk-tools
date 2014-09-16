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

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestSetNonExistingRedisCacheTest()
        {
            RunPowerShellTest("Test-SetNonExistingRedisCacheTest");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestCreateExistingRedisCacheTest()
        {
            RunPowerShellTest("Test-CreateExistingRedisCacheTest");
        }
    }
}
