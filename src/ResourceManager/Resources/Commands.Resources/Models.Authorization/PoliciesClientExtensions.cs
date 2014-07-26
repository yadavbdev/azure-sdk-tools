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

using Microsoft.Azure.Management.Authorization.Models;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Commands.Resources.Models.Authorization
{
    internal static class PoliciesClientExtensions
    {
        public static PSRoleDefinition ToPSRoleDefinition(this RoleDefinition role)
        {
            PSRoleDefinition roleDefinition = null;

            if (role != null)
            {
                roleDefinition = new PSRoleDefinition
                {
                    Name = role.Properties.RoleName,
                    Actions = new List<string>(role.Properties.Permissions.SelectMany(r => r.Actions)),
                    NoActions = new List<string>(role.Properties.Permissions.SelectMany(r => r.NotActions)),
                    Id = role.Id
                };
            }

            return roleDefinition;
        }

        public static PSRoleAssignment ToPSRoleAssignment(this RoleAssignment role, PoliciesClient policyClient, GraphClient graphClient)
        {
            PSRoleDefinition roleDefinition = policyClient.GetRoleDefinition(role.Properties.RoleDefinitionId);
            return new PSRoleAssignment()
            {
                Id = role.Id,
                Principal = graphClient.GetActiveDirectoryObject(role.Properties.PrincipalId.ToString()).Principal,
                Actions = roleDefinition.Actions,
                NoActions = roleDefinition.NoActions,
                RoleDefinitionName = roleDefinition.Name,
                Scope = role.Properties.Scope
            };
        }
    }
}
