using Microsoft.Azure.Commands.RedisCache.Models;
using Microsoft.Azure.Commands.RedisCache.Properties;
using Microsoft.Azure.Management.Redis.Models;
using Microsoft.WindowsAzure;
using System;
using System.Management.Automation;
using SkuStrings = Microsoft.Azure.Management.Redis.Models.SkuName;
using MaxMemoryPolicyStrings = Microsoft.Azure.Management.Redis.Models.MaxMemoryPolicy;

namespace Microsoft.Azure.Commands.RedisCache
{
    [Cmdlet(VerbsCommon.New, "AzureRedisCache"), OutputType(typeof(RedisCacheAttributesWithAccessKeys))]
    public class NewAzureRedisCache : RedisCacheCmdletBase
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of resource group under whcih want to create cache.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Name of redis cache.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Location where want to create cache.")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = false, HelpMessage = "Redis version.")]
        public string RedisVersion { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = false, HelpMessage = "Size of redis cache. Valid values: C0, C1, C2, C3, C4, C5, C6, 250MB, 1GB, 2.5GB, 6GB, 13GB, 26GB, 53GB")]
        [ValidateSet(SizeConverter.C0String, SizeConverter.C1String, SizeConverter.C2String, SizeConverter.C3String, SizeConverter.C4String, SizeConverter.C5String,
            SizeConverter.C6String, SizeConverter.C0, SizeConverter.C1, SizeConverter.C2, SizeConverter.C3, SizeConverter.C4, SizeConverter.C5, SizeConverter.C6, IgnoreCase = false)]
        public string Size { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = false, HelpMessage = "Wheather want to create Basic (1 Node) or Standard (2 Node) cache.")]
        [ValidateSet(SkuStrings.Basic, SkuStrings.Standard, IgnoreCase = false)]
        public string Sku { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Mandatory = false, HelpMessage = "MaxMemoryPolicy property of redis cache. Valid values: AllKeysLRU, AllKeysRandom, NoEviction, VolatileLRU, VolatileRandom, VolatileTTL")]
        [ValidateSet(MaxMemoryPolicyStrings.AllKeysLRU, MaxMemoryPolicyStrings.AllKeysRandom, MaxMemoryPolicyStrings.NoEviction, 
            MaxMemoryPolicyStrings.VolatileLRU, MaxMemoryPolicyStrings.VolatileRandom, MaxMemoryPolicyStrings.VolatileTTL, IgnoreCase = false)]
        public string MaxMemoryPolicy { get; set;}

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        public override void ExecuteCmdlet()
        {
            string skuFamily;

            int skuCapacity = 1;

            bool confirmationRequired = true;

            if (string.IsNullOrEmpty(RedisVersion))
            {
                RedisVersion = "2.8";
            }

            if (string.IsNullOrEmpty(Size))
            {
                Size = SizeConverter.C1String;
            }
            else
            {
                Size = SizeConverter.GetSizeInRedisSpecificFormat(Size);
            }

            // Size to SkuFamily and SkuCapacity conversion
            skuFamily = Size.Substring(0, 1);
            int.TryParse(Size.Substring(1), out skuCapacity);

            if (string.IsNullOrEmpty(Sku))
            {
                Sku = SkuStrings.Standard;
            }

            // If Force flag is not avaliable than check if cache is already available or not
            if (!Force.IsPresent)
            {
                try
                {
                    CacheClient.GetCache(ResourceGroupName, Name);
                }
                catch (CloudException ex)
                {
                    if (ex.ErrorCode == "ResourceNotFound" || ex.Message.Contains("ResourceNotFound"))
                    {
                        // cache does not exists so confirmation is not required
                        confirmationRequired = false;
                    }
                    else if (ex.ErrorCode == "ResourceGroupNotFound" || ex.Message.Contains("ResourceGroupNotFound"))
                    {
                        // resource group not found that let create or update throw error don't throw from here
                    }
                    else
                    { 
                        // all other exceptions should be thrown
                        throw;
                    }
                }
            }

            // If Force is not present and cache exists than 
            if (!Force.IsPresent && confirmationRequired)
            {
                ConfirmAction(
                Force.IsPresent,
                string.Format(Resources.UpdatingRedisCache, Name),
                string.Format(Resources.UpdateRedisCache, Name),
                Name,
                () => WriteObject(new RedisCacheAttributesWithAccessKeys(CacheClient.CreateOrUpdateCache(ResourceGroupName, Name, Location, RedisVersion, skuFamily, skuCapacity, Sku, MaxMemoryPolicy), ResourceGroupName)));
            }
            else
            {
                WriteObject(new RedisCacheAttributesWithAccessKeys(CacheClient.CreateOrUpdateCache(ResourceGroupName, Name, Location, RedisVersion, skuFamily, skuCapacity, Sku, MaxMemoryPolicy), ResourceGroupName));
            }
        }
    }
}