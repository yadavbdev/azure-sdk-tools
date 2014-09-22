using System;
using Microsoft.Azure.Utilities.HttpRecorder;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Testing;
using Microsoft.Azure.Management.Redis;

namespace Microsoft.Azure.Commands.RedisCache.Test.ScenarioTests
{
    public abstract class RedisCacheTestsBase : IDisposable
    {
        private EnvironmentSetupHelper helper;

        protected RedisCacheTestsBase()
        {
            helper = new EnvironmentSetupHelper();
        }

        protected void SetupManagementClients()
        {
            var redisManagementClient = GetRedisManagementClient();
            helper.SetupManagementClients(redisManagementClient);
        }

        protected void RunPowerShellTest(params string[] scripts)
        {
            using (UndoContext context = UndoContext.Current)
            {
                context.Start(TestUtilities.GetCallingClass(2), TestUtilities.GetCurrentMethodName(2));

                SetupManagementClients();

                helper.SetupEnvironment(AzureModule.AzureResourceManager);
                helper.SetupModules(AzureModule.AzureResourceManager, "ScenarioTests\\" + this.GetType().Name + ".ps1");

                helper.RunPowerShellTest(scripts);
            }
        }

        protected RedisManagementClient GetRedisManagementClient()
        {
            return TestBase.GetServiceClient<RedisManagementClient>(new CSMTestEnvironmentFactory());
        }

        public void Dispose()
        {
        }
    }
}
