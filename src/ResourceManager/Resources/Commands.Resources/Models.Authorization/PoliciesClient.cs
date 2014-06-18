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

using Microsoft.Azure.Policy;
using Microsoft.Azure.Policy.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Commands.Resources.Models.Authorization
{
    public class PoliciesClient
    {
        private const string ResourceManagerAppId = "797f4846-ba00-4fd7-ba43-dac1f8f63013";

        public IPolicyManagementClient PolicyClient { get; set; }

        public GraphClient GraphClient { get; set; }

        /// <summary>
        /// Creates PoliciesClient using WindowsAzureSubscription instance.
        /// </summary>
        /// <param name="subscription">The WindowsAzureSubscription instance</param>
        public PoliciesClient(WindowsAzureSubscription subscription)
            : this(subscription.CreateClientFromEndpoint<PolicyManagementClient>(new Uri("https://aad-pas-nova-by1-001.cloudapp.net/")))
        {
            GraphClient = new GraphClient(subscription);
        }

        /// <summary>
        /// Creates PoliciesClient using specified IPolicyManagementClient.
        /// </summary>
        /// <param name="policyClient">The IPolicyManagementClient instance</param>
        public PoliciesClient(IPolicyManagementClient policyClient)
        {
            PolicyClient = policyClient;
        }

        /// <summary>
        /// Parameterless constructor for mocking
        /// </summary>
        public PoliciesClient()
        {

        }

        public PSRoleDefinition GetRoleDefinition(string roleId)
        {
            return PolicyClient.RoleDefinitions.Get(roleId).RoleDefinition.ToPSRoleDefinition();
        }

        /// <summary>
        /// Filters the existing role Definitions.
        /// </summary>
        /// <param name="roleDefinitionName">The role name</param>
        /// <returns>The matched role Definitions</returns>
        public List<PSRoleDefinition> FilterRoleDefinitions(string roleDefinitionName)
        {
            List<PSRoleDefinition> result = new List<PSRoleDefinition>();

            if (string.IsNullOrEmpty(roleDefinitionName))
            {
                result.AddRange(PolicyClient.RoleDefinitions.List(null).RoleDefinitions.Select(r => r.ToPSRoleDefinition()));
            }
            else
            {
                var rd = PolicyClient.RoleDefinitions.List(roleDefinitionName).RoleDefinitions;
                var test = rd.Select(r => r.ToPSRoleDefinition());
                result.Add(PolicyClient.RoleDefinitions.List(roleDefinitionName).RoleDefinitions.Select(r => r.ToPSRoleDefinition()).FirstOrDefault());
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
            string principalId = GraphClient.GetPrincipalId(parameters.PrincipalName).Id;
            string roleAssignmentId = Guid.NewGuid().ToString();
            string roleDefinitionId = FilterRoleDefinitions(parameters.RoleDefinition).First().Id;

            RoleAssignmentCreateParameters createParameters = new RoleAssignmentCreateParameters()
            {
                RoleAssignment = new RoleAssignment()
                {
                    AppId = ResourceManagerAppId,
                    PrincipalId = principalId,
                    RoleAssignmentId = roleAssignmentId,
                    RoleId = roleDefinitionId,
                    Scope = parameters.Scope
                }
            };

            PolicyClient.RoleAssignments.CreateOrUpdate(createParameters);
            return PolicyClient.RoleAssignments.Get(roleAssignmentId).RoleAssignment.ToPSRoleAssignment(this, GraphClient);
        }

        /// <summary>
        /// Filters role assignments based on the passed options.
        /// </summary>
        /// <param name="options">The filtering options</param>
        /// <returns>The filtered role assignments</returns>
        public List<PSRoleAssignment> FilterRoleAssigbments(FilterRoleAssignmentsOptions options)
        {
            List<PSRoleAssignment> result = new List<PSRoleAssignment>();

            RoleAssignmentAtScopeAndAboveParameters listAboveParams = new RoleAssignmentAtScopeAndAboveParameters()
            {
                AppId = ResourceManagerAppId,
                Scope = options.Scope
            };

            result.AddRange(PolicyClient.RoleAssignments.ListRoleAssignmentsAtScopeAndAbove(listAboveParams)
                .RoleAssignments
                .Select(r => r.ToPSRoleAssignment(this, GraphClient)));

            return result;
        }

        /// <summary>
        /// Deletes a role assignments based on the used options.
        /// </summary>
        /// <param name="options">The role assignment filtering options</param>
        /// <returns>The deleted role assignments</returns>
        public List<PSRoleAssignment> RemoveRoleAssignment(FilterRoleAssignmentsOptions options)
        {
            List<PSRoleAssignment> roleAssignments = FilterRoleAssigbments(options);

            foreach (PSRoleAssignment roleAssignment in roleAssignments)
            {
                PolicyClient.RoleAssignments.Delete(roleAssignment.Id);
            }

            return roleAssignments;
        }
    }
}
