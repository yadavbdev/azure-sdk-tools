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

using Microsoft.Azure.Commands.Resources.Models.ActiveDirectory;
using Microsoft.Azure.Management.Authorization;
using Microsoft.Azure.Management.Authorization.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Azure.Commands.Resources.Models.Authorization
{
    public class AuthorizationClient
    {
        /// <summary>
        /// This queue is used by the tests to assign fixed role assignment
        /// names every time the test runs.
        /// </summary>
        public static Queue<Guid> RoleAssignmentNames { get; set; }

        public IAuthorizationManagementClient AuthorizationManagementClient { get; set; }

        public ActiveDirectoryClient ActiveDirectoryClient { get; set; }

        private Guid GetPrincipalId(string principal)
        {
            Guid result;

            if (Guid.TryParse(principal, out result))
            {
                // Input is GUID, just use it as is.
                return result;
            }

            UserFilterOptions userOptions = new UserFilterOptions();
            GroupFilterOptions groupOptions = new GroupFilterOptions();
            bool filterUsers = false;
            bool filterGroups = false;
            string localPart = null;

            if (principal.Contains(' '))
            {
                Debug.Assert(!principal.Contains('@'));
                // The input is user/group display name
                userOptions.DisplayName = principal;
                groupOptions.DisplayName = principal;
                filterUsers = true;
                filterGroups = true;
            }

            if (principal.Contains('@'))
            {
                Debug.Assert(!principal.Contains(' '));
                PSADUser user = ActiveDirectoryClient.GetUser(new UserFilterOptions() { Principal = principal });

                if (user != null)
                {
                    return user.Id;
                }

                groupOptions.Mail = principal;
                localPart = principal.Split('@').First();
                filterGroups = true;
            }

            if (filterUsers)
            {
                var users = ActiveDirectoryClient.FilterUsers(userOptions);
                if (users.Count > 0)
                {
                    return users.First().Id;
                }
            }

            if (filterGroups)
            {
                var groups = ActiveDirectoryClient.FilterGroups(groupOptions);
                if (groups.Count > 0)
                {
                    return groups.First().Id;
                }
            }

            if (string.IsNullOrEmpty(localPart))
            {
                var users = ActiveDirectoryClient.FilterUsers();
                var user = users.FirstOrDefault(u => u.Principal.StartsWith(localPart));

                if (user != null)
                {
                    // The input is live id.
                    return user.Id;
                }
            }

            throw new KeyNotFoundException(string.Format("The user principal '{0}' can not be resolve", principal));
        }

        static AuthorizationClient()
        {
            RoleAssignmentNames = new Queue<Guid>();
        }

        /// <summary>
        /// Creates PoliciesClient using WindowsAzureSubscription instance.
        /// </summary>
        /// <param name="subscription">The WindowsAzureSubscription instance</param>
        public AuthorizationClient(WindowsAzureSubscription subscription)
        {
            ActiveDirectoryClient = new ActiveDirectoryClient(subscription);
            AuthorizationManagementClient = subscription.CreateClientFromResourceManagerEndpoint<AuthorizationManagementClient>();
        }

        public PSRoleDefinition GetRoleDefinition(string roleId)
        {
            return AuthorizationManagementClient.RoleDefinitions.GetById(roleId).RoleDefinition.ToPSRoleDefinition();
        }

        /// <summary>
        /// Filters the existing role Definitions.
        /// </summary>
        /// <param name="name">The role name</param>
        /// <returns>The matched role Definitions</returns>
        public List<PSRoleDefinition> FilterRoleDefinitions(string name)
        {
            List<PSRoleDefinition> result = new List<PSRoleDefinition>();

            if (string.IsNullOrEmpty(name))
            {
                result.AddRange(AuthorizationManagementClient.RoleDefinitions.List().RoleDefinitions.Select(r => r.ToPSRoleDefinition()));
            }
            else
            {
                result.Add(AuthorizationManagementClient.RoleDefinitions.List().RoleDefinitions
                    .FirstOrDefault(r => r.Properties.RoleName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .ToPSRoleDefinition());
            }

            return result;
        }

        /// <summary>
        /// Creates new role assignment.
        /// </summary>
        /// <param name="parameters">The create parameters</param>
        /// <returns>The created role assignment object</returns>
        public PSRoleAssignment CreateRoleAssignment(FilterRoleAssignmentsOptions parameters)
        {
            Guid principalId = GetPrincipalId(parameters.Principal);

            Guid roleAssignmentId = RoleAssignmentNames.Count == 0 ? Guid.NewGuid() : RoleAssignmentNames.Dequeue();
            string roleDefinitionId = FilterRoleDefinitions(parameters.RoleDefinition).First().Id;

            RoleAssignmentCreateParameters createParameters = new RoleAssignmentCreateParameters
            {
                PrincipalId = principalId,
                RoleDefinitionId = roleDefinitionId
            };

            AuthorizationManagementClient.RoleAssignments.Create(parameters.Scope, roleAssignmentId, createParameters);
            return AuthorizationManagementClient.RoleAssignments.Get(parameters.Scope, roleAssignmentId).RoleAssignment.ToPSRoleAssignment(this, ActiveDirectoryClient);
        }

        /// <summary>
        /// Filters role assignments based on the passed options.
        /// </summary>
        /// <param name="options">The filtering options</param>
        /// <returns>The filtered role assignments</returns>
        public List<PSRoleAssignment> FilterRoleAssignments(FilterRoleAssignmentsOptions options)
        {
            List<PSRoleAssignment> result = new List<PSRoleAssignment>();
            ListAssignmentsFilterParameters parameters = new ListAssignmentsFilterParameters
            {
                AtScope = true
            };

            result.AddRange(AuthorizationManagementClient.RoleAssignments
                .ListForScope(options.Scope, parameters)
                .RoleAssignments.Select(r => r.ToPSRoleAssignment(this, ActiveDirectoryClient)));

            if (!string.IsNullOrEmpty(options.Principal))
            {
                result = result.Where(r => r.Principal.Equals(options.Principal, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(options.RoleDefinition))
            {
                result = result.Where(r => r.RoleDefinitionName.Equals(options.RoleDefinition, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return result;
        }

        /// <summary>
        /// Deletes a role assignments based on the used options.
        /// </summary>
        /// <param name="options">The role assignment filtering options</param>
        /// <returns>The deleted role assignments</returns>
        public PSRoleAssignment RemoveRoleAssignment(FilterRoleAssignmentsOptions options)
        {
            PSRoleAssignment roleAssignment = FilterRoleAssignments(options).FirstOrDefault();

            if (roleAssignment != null)
            {
                AuthorizationManagementClient.RoleAssignments.DeleteById(roleAssignment.Id);
            }

            return roleAssignment;
        }
    }
}
