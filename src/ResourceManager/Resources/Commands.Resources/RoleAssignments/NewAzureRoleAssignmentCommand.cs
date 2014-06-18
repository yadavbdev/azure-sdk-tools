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
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Subscription id or name of where the role assignment will be created.")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionId { get; set; }

        public override void ExecuteCmdlet()
        {
            FilterRoleAssignmentsOptions parameters = new FilterRoleAssignmentsOptions()
            {
                Scope = Scope,
                SubscriptionId = SubscriptionId,
                ResourceIdentifier = new ResourceIdentifier()
                {
                    ParentResource = ParentResource,
                    ResourceGroupName = ResourceGroupName,
                    ResourceName = ResourceName,
                    ResourceType = ResourceType,
                    Subscription = CurrentSubscription.SubscriptionId
                },
                PrincipalName = Principal
            };

            WriteObject(PoliciesClient.CreateRoleAssignment(parameters));
        }
    }
}