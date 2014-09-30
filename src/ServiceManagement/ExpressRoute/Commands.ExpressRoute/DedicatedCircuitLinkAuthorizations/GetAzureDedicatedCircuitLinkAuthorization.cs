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

namespace Microsoft.WindowsAzure.Commands.ExpressRoute
{
    using Microsoft.WindowsAzure.Management.ExpressRoute.Models;
    using System.Management.Automation;
    
    [Cmdlet(VerbsCommon.Get, "AzureDedicatedCircuitLinkAuthorization"), OutputType(typeof(AzureDedicatedCircuitLinkAuthorization))]
    public class GetAzureDedicatedCircuitLinkAuthorizationCommand : ExpressRouteBaseCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "Service Key representing the Azure Dedicated Circuit")]
        [ValidateGuid]
        [ValidateNotNullOrEmpty]
        public string ServiceKey { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true,
            HelpMessage = "Authorization Identifier")]
        [ValidateGuid]
        [ValidateNotNullOrEmpty]
        public string AuthId { get; set; }

        public override void ExecuteCmdlet()
        {
            if (!string.IsNullOrEmpty(AuthId))
            {
                GetByAuthId();
            }
            else
            {
                GetNoAuthId();
            }          
        }

        private void GetByAuthId()
        {
            var linkAuth = ExpressRouteClient.GetAzureDedicatedCircuitLinkAuthorization(ServiceKey, AuthId);
            WriteObject(linkAuth);
        }

        private void GetNoAuthId()
        {
            var linkAuths = ExpressRouteClient.ListAzureDedicatedCircuitLinkAuthorizations(ServiceKey);
            WriteObject(linkAuths, true);
        }


    }
}
