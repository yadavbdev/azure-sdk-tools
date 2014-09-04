using Microsoft.Azure.Management.Redis.Models;
using System.Net;

namespace Microsoft.Azure.Commands.RedisCache.Models
{
    class RedisCacheAttributesWithAccessKeys : RedisCacheAttributes
    {
        public RedisCacheAttributesWithAccessKeys(RedisCreateOrUpdateResponse cache, string resourceGroupName)
        {
            Id = cache.Id;
            Location = cache.Location;
            Name = cache.Name;
            Type = cache.Type;
            HostName = cache.Properties.HostName;
            Port = cache.Properties.Port;
            ProvisioningState = cache.Properties.ProvisioningState;
            SslPort = cache.Properties.SslPort;
            MaxMemoryPolicy = cache.Properties.MaxMemoryPolicy;
            RedisVersion = cache.Properties.RedisVersion;
            Size = SizeConverter.GetSizeInUserSpecificFormat(cache.Properties.Sku.Family, cache.Properties.Sku.Capacity);
            Sku = cache.Properties.Sku.Name;
            
            PrimaryKey = cache.Properties.AccessKeys.PrimaryKey;
            SecondaryKey = cache.Properties.AccessKeys.SecondaryKey;
            ResourceGroupName = resourceGroupName;
        }

        public string PrimaryKey { get; private set; }
        public string SecondaryKey { get; private set; }
    }
}