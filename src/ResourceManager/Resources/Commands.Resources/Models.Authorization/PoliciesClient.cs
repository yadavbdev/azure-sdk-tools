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
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Commands.Resources.Models.Authorization
{
    public class PoliciesClient
    {
        public IPolicyManagementClient PolicyClient { get; set; }

        public PoliciesClient(WindowsAzureSubscription subscription)
            : this(subscription.CreateClientFromResourceManagerEndpoint<PolicyManagementClient>())
        {

        }

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
                result.Add(PolicyClient.RoleDefinitions.List(roleDefinitionName).RoleDefinitions.Select(r => r.ToPSRoleDefinition()).FirstOrDefault());
            }

            return result;
        }
    }
}
