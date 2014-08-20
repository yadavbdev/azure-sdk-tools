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
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public PSADObject GetADObject(ADObjectFilterOptions options)
        {
            PSADObject result = null;
            string filter = string.IsNullOrEmpty(options.Id) ? options.Email : options.Id;

            if (!string.IsNullOrEmpty(filter))
            {
                try
                {
                    User user = GraphClient.User.Get(filter).User;

                    if (user != null)
                    {
                        result = user.ToPSADObject();
                    }
                }
                catch { /* The user was not found, ignore the exception */ }

                try
                {
                    Group group = GraphClient.Group.Get(filter).Group;

                    if (group != null)
                    {
                        result = group.ToPSADObject();
                    }
                }
                catch { /* The group was not found, ignore the exception */ }
            }

            return result;
        }

        public List<PSADObject> FilterUsers(ADObjectFilterOptions options)
        {
            List<PSADObject> users = new List<PSADObject>();
            UserListResult result = new UserListResult();

            if (!string.IsNullOrEmpty(options.Id) || !string.IsNullOrEmpty(options.Email))
            {
                users.Add(GetADObject(options));
            }
            else
            {
                if (options.Paging)
                {
                    if (string.IsNullOrEmpty(options.NextLink))
                    {
                        result = GraphClient.User.List(null, options.DisplayName);
                    }
                    else
                    {
                        result = GraphClient.User.ListNext(options.NextLink);
                    }

                    users.AddRange(result.Users.Select(u => u.ToPSADObject()));
                    options.NextLink = result.NextLink;
                }
                else
                {
                    result = GraphClient.User.List(null, options.DisplayName);
                    users.AddRange(result.Users.Select(u => u.ToPSADObject()));

                    while (!string.IsNullOrEmpty(result.NextLink))
                    {
                        result = GraphClient.User.ListNext(result.NextLink);
                        users.AddRange(result.Users.Select(u => u.ToPSADObject()));
                    }
                }
            }

            return users;
        }

        public List<PSADObject> FilterUsers()
        {
            return FilterUsers(new ADObjectFilterOptions());
        }

        public List<PSADObject> ListUserGroups(string principal)
        {
            List<PSADObject> result = new List<PSADObject>();
            Guid objectId = GetObjectId(new ADObjectFilterOptions { Email = principal });
            PSADObject user = GetADObject(new ADObjectFilterOptions { Id = objectId.ToString() });
            var groupsIds = GraphClient.User.GetMemberGroups(new UserGetMemberGroupsParameters { ObjectId = user.Id.ToString() }).ObjectIds;
            var groupsResult = GraphClient.Objects.GetObjectsByObjectIds(new GetObjectsParameters { Ids = groupsIds });
            result.AddRange(groupsResult.AADObject.Select(g => g.ToPSADGroup()));

            return result;
        }

        public List<PSADObject> FilterGroups(ADObjectFilterOptions options)
        {
            List<PSADObject> groups = new List<PSADObject>();

            if (!string.IsNullOrEmpty(options.Id))
            {
                groups.Add(GraphClient.Group.Get(options.Id).Group.ToPSADObject());
            }
            else if (!string.IsNullOrEmpty(options.Email))
            {
                groups.AddRange(ListUserGroups(options.Email));

                if (!string.IsNullOrEmpty(options.DisplayName))
                {
                    groups.RemoveAll(g => !g.DisplayName.Equals(options.DisplayName, StringComparison.OrdinalIgnoreCase));
                }
            }
            else
            {
                GroupListResult result = new GroupListResult();

                if (options.Paging)
                {
                    if (string.IsNullOrEmpty(options.NextLink))
                    {
                        result = GraphClient.Group.List(options.Email, options.DisplayName);
                    }
                    else
                    {
                        result = GraphClient.Group.ListNext(result.NextLink);
                    }

                    groups.AddRange(result.Groups.Select(u => u.ToPSADObject()));
                    options.NextLink = result.NextLink;
                }
                else
                {
                    result = GraphClient.Group.List(options.Email, options.DisplayName);
                    groups.AddRange(result.Groups.Select(u => u.ToPSADObject()));

                    while (!string.IsNullOrEmpty(result.NextLink))
                    {
                        result = GraphClient.Group.ListNext(result.NextLink);
                        groups.AddRange(result.Groups.Select(u => u.ToPSADObject()));
                    }
                }
            }

            return groups;
        }

        public List<PSADObject> FilterGroups()
        {
            return FilterGroups(new ADObjectFilterOptions());
        }

        public List<PSADObject> GetGroupMembers(ADObjectFilterOptions options)
        {
            List<PSADObject> members = new List<PSADObject>();

            if (!string.IsNullOrEmpty(options.Id))
            {
                members.Add(GraphClient.Group.Get(options.Id).Group.ToPSADObject());
            }
            else
            {
                PSADObject group = FilterGroups(new ADObjectFilterOptions { DisplayName = options.DisplayName }).FirstOrDefault();

                if (group != null)
                {
                    GetObjectsResult result = new GetObjectsResult();

                    if (options.Paging)
                    {
                        if (string.IsNullOrEmpty(options.NextLink))
                        {
                            result = GraphClient.Group.GetGroupMembers(group.Id.ToString());
                        }
                        else
                        {
                            result = GraphClient.Group.GetGroupMembersNext(result.NextLink);
                        }

                        members.AddRange(result.AADObject.Select(u => u.ToPSADObject()));
                        options.NextLink = result.NextLink;
                    }
                    else
                    {
                        result = GraphClient.Group.GetGroupMembers(group.Id.ToString());
                        members.AddRange(result.AADObject.Select(u => u.ToPSADObject()));

                        while (!string.IsNullOrEmpty(result.NextLink))
                        {
                            result = GraphClient.Group.GetGroupMembersNext(result.NextLink);
                            members.AddRange(result.AADObject.Select(u => u.ToPSADObject()));
                        }
                    }
                }
            }
            return members;
        }

        public Guid GetObjectId(ADObjectFilterOptions options)
        {
            // Input is object id.
            if (!string.IsNullOrEmpty(options.Id))
            {
                Guid result;
                if (Guid.TryParse(options.Id, out result))
                {
                    // Input is GUID, just use it as is.
                    return result;
                }

                throw new KeyNotFoundException(string.Format("The provided object id '{0}' can not be parsed", options.Id));
            }

            // Input is principal org id, live id or group mail.
            if (!string.IsNullOrEmpty(options.Email))
            {
                try
                {
                    PSADObject user = GetADObject(options);
                    if (user != null)
                    {
                        // Input is OrgId principal
                        return user.Id;
                    }
                }
                catch { /* Unable to retrieve the user, skip */ }

                string localPart = options.Email.Split('@').First();
                if (!string.IsNullOrEmpty(localPart))
                {
                    var users = FilterUsers();
                    var user = users.FirstOrDefault(u => u.Email.StartsWith(localPart));

                    if (user != null)
                    {
                        // Input is live id.
                        return user.Id;
                    }
                }

                var groups = FilterGroups(options);
                if (groups.Count > 0)
                {
                    // Input is group mail
                    return groups.First().Id;
                }

                throw new KeyNotFoundException(string.Format("The provided email '{0}' can not be resolved", options.Email));
            }

            if (!string.IsNullOrEmpty(options.DisplayName))
            {
                var users = FilterUsers(options);
                if (users.Count > 0)
                {
                    // Input is used display name.
                    return users.First().Id;
                }

                var groups = FilterGroups(options);
                if (groups.Count > 0)
                {
                    // Input is group display name
                    return groups.First().Id;
                }

                throw new KeyNotFoundException(string.Format("The provided display name '{0}' can not be resolved", options.DisplayName));
            }

            throw new KeyNotFoundException("Please provide the object id, email or display name filter to resolve.");
        }
    }
}
