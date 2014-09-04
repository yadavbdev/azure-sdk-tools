using Microsoft.Azure.Commands.RedisCache.Models;
using Microsoft.Azure.Commands.RedisCache.Properties;
using Microsoft.Azure.Management.Redis.Models;
using Microsoft.WindowsAzure;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.RedisCache
{
    [Cmdlet(VerbsCommon.New, "AzureRedisCacheKey"), OutputType(typeof(RedisAccessKeys))]
    public class NewAzureRedisCacheKey : RedisCacheCmdletBase
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of resource group under whcih cache exists.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of redis cache.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Regenerate this key.")]
        [ValidateNotNullOrEmpty]
        [ValidateSet("Primary", "Secondary", IgnoreCase = false)]
        public string KeyType { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        public override void ExecuteCmdlet()
        {
            RedisKeyType keyTypeToRegenerated = RedisKeyType.Primary;
            if (KeyType.Equals("Secondary"))
            {
                keyTypeToRegenerated = RedisKeyType.Secondary;
            }
            
            if (!Force.IsPresent)
            {
                ConfirmAction(
                Force.IsPresent,
                string.Format(Resources.RegeneratingRedisCacheKey, Name, keyTypeToRegenerated.ToString()),
                string.Format(Resources.RegenerateRedisCacheKey, Name, keyTypeToRegenerated.ToString()),
                Name,
                () => CacheClient.RegenerateAccessKeys(ResourceGroupName, Name, keyTypeToRegenerated));
            }
            else
            {
                CacheClient.RegenerateAccessKeys(ResourceGroupName, Name, keyTypeToRegenerated);
            }

            RedisListKeysResponse keysResponse = CacheClient.GetAccessKeys(ResourceGroupName, Name);
            WriteObject(new RedisAccessKeys()
            {
                PrimaryKey = keysResponse.PrimaryKey,
                SecondaryKey = keysResponse.SecondaryKey
            });
        }
    }
}