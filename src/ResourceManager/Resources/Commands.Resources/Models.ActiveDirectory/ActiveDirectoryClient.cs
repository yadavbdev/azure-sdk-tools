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

        private const string GroupType = "Group";

        private const string UserType = "User";

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
            string filter = string.IsNullOrEmpty(options.Id) ? options.Principal : options.Id;

            if (!string.IsNullOrEmpty(filter))
            {
                User user = GraphClient.User.Get(filter).User;

                if (user != null)
                {
                    result = user.ToPSADUser();
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

        public List<PSADGroup> ListUserGroups(string principal)
        {
            List<PSADGroup> result = new List<PSADGroup>();
            Guid objectId = GetObjectId(principal);
            PSADUser user = GetUser(new UserFilterOptions { Id = objectId.ToString() });
            var groupsIds = GraphClient.User.GetMemberGroups(new UserGetMemberGroupsParameters { ObjectId = user.Id.ToString() }).ObjectIds;
            var groupsResult = GraphClient.Objects.GetObjectsByObjectIds(new GetObjectsParameters { Ids = groupsIds });
            result.AddRange(groupsResult.AADObject.Select(g => g.ToPSADGroup()));

            return result;
        }

        public List<PSADGroup> FilterGroups(GroupFilterOptions options)
        {
            List<PSADGroup> groups = new List<PSADGroup>();

            if (!string.IsNullOrEmpty(options.UserPrincipal))
            {
                groups.AddRange(ListUserGroups(options.UserPrincipal));

                if (!string.IsNullOrEmpty(options.DisplayName))
                {
                    groups.RemoveAll(g => !g.DisplayName.Equals(options.DisplayName, StringComparison.OrdinalIgnoreCase));
                }
            }
            else
            {
                GroupListResult result = GraphClient.Group.List(options.Mail, options.DisplayName);
                groups.AddRange(result.Groups.Select(u => u.ToPSADGroup()));

                while (!string.IsNullOrEmpty(result.NextLink))
                {
                    result = GraphClient.Group.ListNext(result.NextLink);
                    groups.AddRange(result.Groups.Select(u => u.ToPSADGroup()));
                }
            }

            return groups;
        }

        public List<PSADGroup> FilterGroups()
        {
            return FilterGroups(new GroupFilterOptions());
        }

        public List<PSADObject> GetGroupMembers(string groupName)
        {
            List<PSADObject> result = new List<PSADObject>();
            PSADGroup group = FilterGroups(new GroupFilterOptions { DisplayName = groupName }).FirstOrDefault();

            if (group != null)
            {
                var objects = GraphClient.Group.GetGroupMembers(group.Id.ToString()).AADObject;

                foreach (var item in objects)
                {
                    if (item.ObjectType == GroupType)
                    {
                        result.Add(item.ToPSADGroup());
                    }
                    else if (item.ObjectType == UserType)
                    {
                        result.Add(item.ToPSADUser());
                    }
                }
            }

            return result;
        }

        public Guid GetObjectId(string principal)
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

            if (principal.Contains('@'))
            {
                try
                {
                    PSADUser user = GetUser(new UserFilterOptions() { Principal = principal });
                    if (user != null)
                    {
                        return user.Id;
                    }
                }
                catch { /* Unable to retrieve the user, skip */ }

                groupOptions.Mail = principal;
                localPart = principal.Split('@').First();
                filterGroups = true;
            }
            else
            {
                // The input is user/group display name
                userOptions.DisplayName = principal;
                groupOptions.DisplayName = principal;
                filterUsers = true;
                filterGroups = true;
            }

            if (filterUsers)
            {
                var users = FilterUsers(userOptions);
                if (users.Count > 0)
                {
                    return users.First().Id;
                }
            }

            if (!string.IsNullOrEmpty(localPart))
            {
                var users = FilterUsers();
                var user = users.FirstOrDefault(u => u.Principal.StartsWith(localPart));

                if (user != null)
                {
                    // The input is live id.
                    return user.Id;
                }
            }

            if (filterGroups)
            {
                var groups = FilterGroups(groupOptions);
                if (groups.Count > 0)
                {
                    return groups.First().Id;
                }
            }

            throw new KeyNotFoundException(string.Format("The user principal '{0}' can not be resolve", principal));
        }
    }
}
