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

namespace Microsoft.Azure.Commands.Resources.Models.Authorization
{
    public class FilterRoleAssignmentsOptions
    {
        public string RoleDefinition { get; set; }

        private string scope;

        public string Scope
        {
            get
            {
                string result;
                string resourceIdentifier = ResourceIdentifier.ToString();

                if (!string.IsNullOrEmpty(scope))
                {
                    result = scope;
                }
                else if (!string.IsNullOrEmpty(resourceIdentifier))
                {
                    result = resourceIdentifier;
                }
                else
                {
                    result = new ResourceIdentifier() { Subscription = SubscriptionId }.ToString();
                }

                return result;
            }
            set
            {
                scope = value;
            }
        }

        public string SubscriptionId { get; set; }

        public ResourceIdentifier ResourceIdentifier { get; set; }

        public string PrincipalName { get; set; }
    }
}