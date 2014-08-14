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
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace Microsoft.Azure.Commands.Resources.Models.ActiveDirectory
{
    public class ActiveDirectoryClient
    {
        private const string GraphEndpoint = "https://graph.ppe.windows.net/";

        public GraphRbacManagementClient GraphClient { get; private set; }

        /// <summary>
        /// Creates new ActiveDirectoryClient using WindowsAzureSubscription.
        /// </summary>
        /// <param name="subscription">The WindowsAzureSubscription instance</param>
        public ActiveDirectoryClient(WindowsAzureSubscription subscription)
        {
            AccessTokenCredential creds = subscription.CreateTokenCredentials();
            GraphClient = subscription.CreateClient<GraphRbacManagementClient>(
                false,
                creds.TenantID,
                creds,
                new Uri(GraphEndpoint));
        }

        public PSADUser GetUser(UserFilterOptions options)
        {
            PSADUser result = null;

            if (!string.IsNullOrEmpty(options.Principal))
            {
                User user = GraphClient.User.Get(options.Principal).User;

                if (user != null)
                {
                    result = user.ToPSADUser();
                }
            }

            if (!string.IsNullOrEmpty(options.Id))
            {
                GetObjectsParameters parameters = new GetObjectsParameters
                {
                    Ids = new List<string> { options.Id }
                };

                var objects = GraphClient.Objects.GetObjectsByObjectIds(parameters).AADObject;

                if (objects.Count > 0)
                {
                    AADObject first = objects.First();
                    result = new PSADUser();
                    result.DisplayName = first.DisplayName;
                    result.Id = new Guid(first.ObjectId);
                    result.Principal = first.UserPrincipalName;
                }
            }

            return result;
        }

        public List<PSADUser> FilterUsers(UserFilterOptions options)
        {
            List<PSADUser> users = new List<PSADUser>();
            UserListResult result = GraphClient.User.List(null, options.DisplayName);
            users.AddRange(result.Users.Select(u => u.ToPSADUser()));

            while (!string.IsNullOrEmpty(result.NextLink))
            {
                result = GraphClient.User.ListNext(result.NextLink);
                users.AddRange(result.Users.Select(u => u.ToPSADUser()));
            }

            return users;
        }

        public List<PSADUser> FilterUsers()
        {
            return FilterUsers(new UserFilterOptions());
        }

        public List<PSADGroup> FilterGroups(GroupFilterOptions options)
        {
            List<PSADGroup> groups = new List<PSADGroup>();
            GroupListResult result = GraphClient.Group.List(options.UserPrincipal, options.DisplayName);
            groups.AddRange(result.Groups.Select(u => u.ToPSADGroup()));

            while (!string.IsNullOrEmpty(result.NextLink))
            {
                result = GraphClient.Group.ListNext(result.NextLink);
                groups.AddRange(result.Groups.Select(u => u.ToPSADGroup()));
            }

            return groups;
        }

        public List<PSADGroup> FilterGroups()
        {
            return FilterGroups(new GroupFilterOptions());
        }
    }
}
