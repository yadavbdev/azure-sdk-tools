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

using Microsoft.Azure.Graph.RBAC;
using Microsoft.Azure.Graph.RBAC.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using System;

namespace Microsoft.Azure.Commands.Resources.Models.Authorization
{
    public class GraphClient
    {
        private const string GraphEndpoint = "https://graph.ppe.windows.net/";

        public GraphRbacManagementClient GraphRbacClient { get; private set; }

        /// <summary>
        /// Creates new GraphClient using WindowsAzureSubscription.
        /// </summary>
        /// <param name="subscription">The WindowsAzureSubscription instance</param>
        public GraphClient(WindowsAzureSubscription subscription, AccessTokenCredential creds)
        {
            GraphRbacClient = subscription.CreateClient<GraphRbacManagementClient>(
                false,
                creds.TenantID,
                creds,
                new Uri(GraphEndpoint));
        }

        /// <summary>
        /// Gets the AD object for the specified principal name.
        /// </summary>
        /// <param name="options">The principal name options. Could be user, group or role</param>
        /// <returns>The AD object corresponding to the passed Id</returns>
        public PSActiveDirectoryObject GetActiveDirectoryObject(string principal)
        {
            PSActiveDirectoryObject userObject = new PSActiveDirectoryObject();

            User user = GraphRbacClient.User.Get(principal.ToString()).User;
            userObject.DisplayName = user.DisplayName;
            userObject.Id = new Guid(user.ObjectId);
            userObject.Principal = user.UserPrincipalName;
            userObject.Type = user.ObjectType;

            return userObject;
        }
    }
}
