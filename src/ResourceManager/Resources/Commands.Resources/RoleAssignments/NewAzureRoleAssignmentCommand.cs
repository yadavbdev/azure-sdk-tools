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

using Microsoft.Azure.Commands.Resources.Models;
using Microsoft.Azure.Commands.Resources.Models.Authorization;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Resources
{
    /// <summary>
    /// Creates new role assignment.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureRoleAssignment"), OutputType(typeof(PSRoleAssignment))]
    public class NewAzureRoleAssignmentCommand : RoleAssignmentBaseCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the resource to assign the role to.")]
        [ValidateNotNullOrEmpty]
        public string Principal { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Role to assign the principals with.")]
        [ValidateNotNullOrEmpty]
        public string RoleDefinitionName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Scope of the role assignment. In the format of relative URI. If not specified, will assign the role at subscription level. If specified, it can either start with \"/subscriptions/<id>\" or the part after that. If it's latter, the current subscription id will be used.")]
        [ValidateNotNullOrEmpty]
        public string Scope { get; set; }

        public override void ExecuteCmdlet()
        {
            FilterRoleAssignmentsOptions parameters = new FilterRoleAssignmentsOptions()
            {
                Scope = Scope,
                Subscription = CurrentSubscription.SubscriptionId,
                RoleDefinition = RoleDefinitionName,
                Principal = Principal,
                ResourceIdentifier = new ResourceIdentifier()
                {
                    ParentResource = ParentResource,
                    ResourceGroupName = ResourceGroupName,
                    ResourceName = ResourceName,
                    ResourceType = ResourceType,
                    Subscription = CurrentSubscription.SubscriptionId,
                }
            };

            WriteObject(PoliciesClient.CreateRoleAssignment(parameters));
        }
    }
}