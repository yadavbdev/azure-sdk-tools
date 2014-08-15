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

using Microsoft.Azure.Graph.RBAC.Models;
using System;

namespace Microsoft.Azure.Commands.Resources.Models.ActiveDirectory
{
    internal static class ActiveDirectoryClientExtensions
    {
        public static PSADUser ToPSADUser(this User user)
        {
            return new PSADUser()
            {
                DisplayName = user.DisplayName,
                Id = new Guid(user.ObjectId),
                Principal = user.UserPrincipalName
            };
        }

        public static PSADGroup ToPSADGroup(this Group group)
        {
            return new PSADGroup()
            {
                DisplayName = group.DisplayName,
                Id = new Guid(group.ObjectId)
            };
        }

        public static PSADUser ToPSADUser(this AADObject obj)
        {
            return new PSADUser()
            {
                DisplayName = obj.DisplayName,
                Id = new Guid(obj.ObjectId),
                Principal = obj.UserPrincipalName
            };
        }

        public static PSADGroup ToPSADGroup(this AADObject obj)
        {
            return new PSADGroup()
            {
                DisplayName = obj.DisplayName,
                Id = new Guid(obj.ObjectId)
            };
        }
    }
}
