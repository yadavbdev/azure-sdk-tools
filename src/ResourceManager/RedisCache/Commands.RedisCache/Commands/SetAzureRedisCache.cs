using Microsoft.Azure.Commands.RedisCache.Models;
using Microsoft.Azure.Management.Redis.Models;
using Microsoft.WindowsAzure;
using System.Management.Automation;
using MaxMemoryPolicyStrings = Microsoft.Azure.Management.Redis.Models.MaxMemoryPolicy;

namespace Microsoft.Azure.Commands.RedisCache
{
    [Cmdlet(VerbsCommon.Set, "AzureRedisCache", DefaultParameterSetName = MaxMemoryParameterSetName), OutputType(typeof(RedisCacheAttributesWithAccessKeys))]
    public class SetAzureRedisCache : RedisCacheCmdletBase
    {
        internal const string MaxMemoryParameterSetName = "Only MaxMemoryPolicy";

        [Parameter(ParameterSetName = MaxMemoryParameterSetName, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of resource group under which you want to create cache.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(ParameterSetName = MaxMemoryParameterSetName, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of redis cache.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(ParameterSetName = MaxMemoryParameterSetName, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "MaxMemoryPolicy property of redis cache. Valid values: AllKeysLRU, AllKeysRandom, NoEviction, VolatileLRU, VolatileRandom, VolatileTTL")]
        [ValidateSet(MaxMemoryPolicyStrings.AllKeysLRU, MaxMemoryPolicyStrings.AllKeysRandom, MaxMemoryPolicyStrings.NoEviction, 
            MaxMemoryPolicyStrings.VolatileLRU, MaxMemoryPolicyStrings.VolatileRandom, MaxMemoryPolicyStrings.VolatileTTL, IgnoreCase = false)]
        public string MaxMemoryPolicy { get; set;}

        public override void ExecuteCmdlet()
        {
            try
            {
                RedisGetResponse response = CacheClient.GetCache(ResourceGroupName, Name);
                WriteObject(new RedisCacheAttributesWithAccessKeys(
                    CacheClient.CreateOrUpdateCache(ResourceGroupName, Name, response.Location, response.Properties.RedisVersion, 
                        response.Properties.Sku.Family, response.Properties.Sku.Capacity, response.Properties.Sku.Name, MaxMemoryPolicy), 
                    ResourceGroupName));
            }
            catch (CloudException)
            {
                throw;
            }
        }
    }
}