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
using Microsoft.Azure.Commands.Network.NetworkSecurityGroup.Model;

namespace Microsoft.Azure.Commands.Network.NetworkSecurityGroup.Subnet
{
    [Cmdlet(VerbsCommon.Get, "AzureNetworkSecurityGroup"), OutputType(typeof(INetworkSecurityGroup))]
    public class GetAzureNetworkSecurityGroupForSubnet : NetworkCmdletBase
    {

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string VirtualNetworkName { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string SubnetName { get; set; }

        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public SwitchParameter DetailLevel { get; set; }

        public override void ExecuteCmdlet()
        {
            string securityGroupName = Client.GetNetworkSecurityGroupForSubnet(VirtualNetworkName, SubnetName);
            INetworkSecurityGroup securityGroup = Client.GetNetworkSecurityGroup(securityGroupName, DetailLevel);
            WriteObject(securityGroup);
        }
    }
}
