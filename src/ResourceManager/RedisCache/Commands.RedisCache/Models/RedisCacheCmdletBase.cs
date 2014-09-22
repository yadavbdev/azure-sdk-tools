using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.Azure.Commands.RedisCache
{
    /// <summary>
    /// The base class for all Microsoft Azure Redis Cache Management Cmdlets
    /// </summary>
    public abstract class RedisCacheCmdletBase : AzurePSCmdlet
    {
        private RedisCacheClient cacheClient;

        public RedisCacheClient CacheClient
        {
            get
            {
                if (cacheClient == null)
                {
                    cacheClient = new RedisCacheClient(CurrentContext);
                }
                return cacheClient;
            }

            set 
            {
                cacheClient = value; 
            }
        }
    }
}