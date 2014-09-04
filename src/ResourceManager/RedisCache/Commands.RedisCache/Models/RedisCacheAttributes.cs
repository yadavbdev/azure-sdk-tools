using Microsoft.Azure.Management.Redis.Models;
using System.Net;

namespace Microsoft.Azure.Commands.RedisCache.Models
{
    class RedisCacheAttributes
    {
        public RedisCacheAttributes(RedisResource cache, string resourceGroupName)
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
            ResourceGroupName = resourceGroupName;
        }

        public RedisCacheAttributes(RedisGetResponse cache, string resourceGroupName)
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
            ResourceGroupName = resourceGroupName;
        }

        public RedisCacheAttributes() { }

        private string _resourceGroupName;
        public string ResourceGroupName 
        {
            get
            {
                return _resourceGroupName;
            }

            protected set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _resourceGroupName = value;
                }
                else
                { 
                    // if resource group name is null (when try to get all cache in given subscription it will be null) we have to fetch it from Id.
                    _resourceGroupName = Id.Split('/')[4];
                }
            }
        }

        public string Id { get; protected set; }

        public string Location { get; protected set; }

        public string Name { get; protected set; }

        public string Type { get; protected set; }

        public string HostName { get; protected set; }

        public int Port { get; protected set; }

        public string ProvisioningState { get; protected set; }

        public int SslPort { get; protected set; }

        public string MaxMemoryPolicy { get; protected set; }

        public string RedisVersion { get; protected set; }

        public string Size { get; protected set; }

        public string Sku { get; protected set; }
    }
}