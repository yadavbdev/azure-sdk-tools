using Microsoft.Azure.Commands.RedisCache.Models;
using Microsoft.Azure.Commands.RedisCache.Properties;
using Microsoft.Azure.Management.Redis.Models;
using Microsoft.WindowsAzure;
using System;
using System.Management.Automation;
using SkuStrings = Microsoft.Azure.Management.Redis.Models.Sku;
using MaxMemoryPolicyStrings = Microsoft.Azure.Management.Redis.Models.MaxMemoryPolicy;

namespace Microsoft.Azure.Commands.RedisCache
{
    [Cmdlet(VerbsCommon.Remove, "AzureRedisCache"), OutputType(typeof(bool))]
    public class RemoveAzureRedisCache : RedisCacheCmdletBase
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of resource group under whcih cache exists.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of redis cache to be removed.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        public override void ExecuteCmdlet()
        {
            if (!Force.IsPresent)
            {
                ConfirmAction(
                Force.IsPresent,
                string.Format(Resources.RemovingRedisCache, Name),
                string.Format(Resources.RemoveRedisCache, Name),
                Name,
                () => CacheClient.DeleteCache(ResourceGroupName, Name));
            }
            else
            {
                CacheClient.DeleteCache(ResourceGroupName, Name);
            }

            if (PassThru)
            {
                WriteObject(true);
            }
        }
    }
}