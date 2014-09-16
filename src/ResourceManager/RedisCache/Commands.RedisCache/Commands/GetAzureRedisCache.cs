using Microsoft.Azure.Commands.RedisCache.Models;
using Microsoft.Azure.Management.Redis.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RedisCache
{
    [Cmdlet(VerbsCommon.Get, "AzureRedisCache", DefaultParameterSetName = BaseParameterSetName), OutputType(typeof(List<RedisCacheAttributes>))]
    public class GetAzureRedisCache : RedisCacheCmdletBase
    {
        internal const string BaseParameterSetName = "All In Subscription";
        internal const string ResourceGroupParameterSetName = "All In Resource Group";
        internal const string RedisCacheParameterSetName = "Specific Redis Cache";

        [Parameter(ParameterSetName = ResourceGroupParameterSetName, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of resource group under whcih want to create cache.")]
        [Parameter(ParameterSetName = RedisCacheParameterSetName, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of resource group under whcih want to create cache.")]
        public string ResourceGroupName { get; set; }

        [Parameter(ParameterSetName = RedisCacheParameterSetName, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of redis cache.")]
        public string Name { get; set; }

        public override void ExecuteCmdlet()
        {
            if (!string.IsNullOrEmpty(ResourceGroupName) && !string.IsNullOrEmpty(Name))
            {
                // Get for single cache
                WriteObject(new RedisCacheAttributes(CacheClient.GetCache(ResourceGroupName, Name), ResourceGroupName));
            }
            else
            {
                // List all cache in given resource group if avaliable otherwise all cache in given subscription
                RedisListResponse response = CacheClient.ListCaches(ResourceGroupName);
                List<RedisCacheAttributes> list = new List<RedisCacheAttributes>();
                foreach (RedisResource resource in response.Value)
                {
                    list.Add(new RedisCacheAttributes(resource, ResourceGroupName));
                }
                WriteObject(list, true);

                while (!string.IsNullOrEmpty(response.NextLink))
                {
                    // List using next link
                    response = CacheClient.ListCachesUsingNextLink(response.NextLink);
                    list = new List<RedisCacheAttributes>();
                    foreach (RedisResource resource in response.Value)
                    {
                        list.Add(new RedisCacheAttributes(resource, ResourceGroupName));
                    }
                    WriteObject(list, true);
                }
            }
        }
    }
}