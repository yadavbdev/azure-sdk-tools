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
using Microsoft.Azure.Commands.Network.NetworkSecurityGroup.Utilities;

namespace Microsoft.Azure.Commands.Network.NetworkSecurityGroup
{
    [Cmdlet(VerbsCommon.Remove, "AzureNetworkSecurityRule"), OutputType(typeof(INetworkSecurityGroup))]
    public class RemoveAzureNetworkSecurityRule : NetworkSecurityGroupConfigurationBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the Network Security Rule to remove.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override void ExecuteCmdlet()
        {
            Client.RemoveNetworkSecurityRule(NetworkSecurityGroup.GetInstance().Name, Name);
            WriteObject(Client.GetNetworkSecurityGroup(NetworkSecurityGroup.GetInstance().Name, true));
        }
    }
}
