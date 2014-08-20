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
        public static PSADObject ToPSADObject(this User user)
        {
            return new PSADObject()
            {
                DisplayName = user.DisplayName,
                Id = new Guid(user.ObjectId),
                Email = user.UserPrincipalName
            };
        }

        public static PSADObject ToPSADObject(this Group group)
        {
            return new PSADObject()
            {
                DisplayName = group.DisplayName,
                Id = new Guid(group.ObjectId)
            };
        }

        public static PSADObject ToPSADObject(this AADObject obj)
        {
            return new PSADObject()
            {
                DisplayName = obj.DisplayName,
                Id = new Guid(obj.ObjectId),
                Email = obj.UserPrincipalName
            };
        }

        public static PSADObject ToPSADGroup(this AADObject obj)
        {
            return new PSADObject()
            {
                DisplayName = obj.DisplayName,
                Id = new Guid(obj.ObjectId)
            };
        }
    }
}
