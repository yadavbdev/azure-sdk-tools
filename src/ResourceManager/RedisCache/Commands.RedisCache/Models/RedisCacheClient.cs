using Microsoft.Azure.Management.Redis;
using Microsoft.Azure.Management.Redis.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.Commands.RedisCache
{
    public class RedisCacheClient
    {
        private RedisManagementClient _client;
        public RedisCacheClient(WindowsAzureSubscription currentSubscription)
        {
            _client = currentSubscription.CreateClient<RedisManagementClient>();
        }
        public RedisCacheClient() { }

        public RedisCreateOrUpdateResponse CreateOrUpdateCache(string resourceGroupName, string cacheName, string location, string redisVersion, string skuFamily, int skuCapacity, string skuName, string maxMemoryPolicy)
        {
            RedisCreateOrUpdateParameters parameters = new RedisCreateOrUpdateParameters
                                                    {
                                                        Location = location,
                                                        Properties = new RedisProperties
                                                        {
                                                            RedisVersion = redisVersion,
                                                            Sku = new Sku() { 
                                                                Name = skuName,
                                                                Family = skuFamily,
                                                                Capacity = skuCapacity
                                                            }
                                                        }
                                                    };

            if (!string.IsNullOrEmpty(maxMemoryPolicy))
            {
                parameters.Properties.MaxMemoryPolicy = maxMemoryPolicy;
            }
            RedisCreateOrUpdateResponse response = _client.Redis.CreateOrUpdate(resourceGroupName: resourceGroupName, name: cacheName, parameters: parameters);
            return response;
        }

        public OperationResponse DeleteCache(string resourceGroupName, string cacheName)
        {
            return _client.Redis.Delete(resourceGroupName: resourceGroupName, name: cacheName);
        }

        public RedisGetResponse GetCache(string resourceGroupName, string cacheName)
        {
            return _client.Redis.Get(resourceGroupName: resourceGroupName, name: cacheName);
        }

        public RedisListResponse ListCaches(string resourceGroupName)
        {
            if (string.IsNullOrEmpty(resourceGroupName))
            {
                return _client.Redis.List(null);
            }
            else
            {
                return _client.Redis.List(resourceGroupName: resourceGroupName);
            }
        }

        public RedisListResponse ListCachesUsingNextLink(string nextLink)
        {
            return _client.Redis.ListNext(nextLink: nextLink);
        }
        
        public OperationResponse RegenerateAccessKeys(string resourceGroupName, string cacheName, RedisKeyType keyType)
        {
            return _client.Redis.RegenerateKey(resourceGroupName: resourceGroupName, name: cacheName, parameters: new RedisRegenerateKeyParameters() { KeyType = keyType });
        }

        public RedisListKeysResponse GetAccessKeys(string resourceGroupName, string cacheName)
        {
            return _client.Redis.ListKeys(resourceGroupName: resourceGroupName, name: cacheName);
        }
    }
}
