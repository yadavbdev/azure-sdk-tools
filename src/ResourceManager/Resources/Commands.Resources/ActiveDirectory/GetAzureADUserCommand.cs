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

using Microsoft.Azure.Commands.ActiveDirectory.Models;
using Microsoft.Azure.Commands.Resources.Models;
using Microsoft.Azure.Commands.Resources.Models.ActiveDirectory;
using Microsoft.Azure.Commands.Resources.Models.Authorization;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.ActiveDirectory
{
    /// <summary>
    /// Get AD users.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureADUser"), OutputType(typeof(List<PSADUser>))]
    public class GetAzureADUserCommand : ActiveDirectoryBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of user to return. If not specified, return all users matching other filters.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        public override void ExecuteCmdlet()
        {
            UserFilterOptions options = new UserFilterOptions
            {
                DisplayName = Name,
                Paging = true
            };

            do
            {
                WriteObject(ActiveDirectoryClient.FilterUsers(options), true);

            } while (!string.IsNullOrEmpty(options.NextLink));
        }
    }
}