// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System.Management.Automation;
using Microsoft.Azure.Commands.ManagedCache.Models;
using Microsoft.Azure.Management.ManagedCache.Models;

namespace Microsoft.Azure.Commands.ManagedCache
{
    [Cmdlet(VerbsCommon.New, "AzureManagedCacheNamedCache "), OutputType(typeof(PSCacheServiceWithNamedCaches))]
    public class NewAzureManagedCacheNamedCache : ManagedCacheCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string NamedCache { get; set; }

        [Parameter(Mandatory = false)]
        [ValidateSet("Absolute", "Sliding", "Never", IgnoreCase = false)]
        public string ExpiryPolicy { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public int ExpiryTime { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter WithNotifications { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter WithHighAvailability { get; set; }
        
        [Parameter(Mandatory = false)]
        public SwitchParameter WithEviction { get; set; }
        
        public override void ExecuteCmdlet()
        {
            string cacheServiceName = CacheClient.NormalizeCacheServiceName(Name);
            CacheClient.ProgressRecorder = (p) => { WriteVerbose(p); };
            WriteObject(new PSCacheServiceWithNamedCaches(CacheClient.AddNamedCache(cacheServiceName, NamedCache, ExpiryPolicy, ExpiryTime,
                WithEviction.IsPresent, WithNotifications.IsPresent, WithHighAvailability.IsPresent)));
        }
    }
}