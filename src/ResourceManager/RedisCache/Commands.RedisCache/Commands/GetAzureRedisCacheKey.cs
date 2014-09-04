using Microsoft.Azure.Commands.RedisCache.Models;
using Microsoft.Azure.Management.Redis.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RedisCache
{
    [Cmdlet(VerbsCommon.Get, "AzureRedisCacheKey"), OutputType(typeof(RedisAccessKeys))]
    public class GetAzureRedisCacheKey : RedisCacheCmdletBase
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of resource group under whcih cache exists.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of redis cache.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override void ExecuteCmdlet()
        {
            RedisListKeysResponse keysResponse = CacheClient.GetAccessKeys(ResourceGroupName, Name);
            WriteObject(new RedisAccessKeys() {
                            PrimaryKey = keysResponse.PrimaryKey,
                            SecondaryKey = keysResponse.SecondaryKey
                       });
        }
    }
}