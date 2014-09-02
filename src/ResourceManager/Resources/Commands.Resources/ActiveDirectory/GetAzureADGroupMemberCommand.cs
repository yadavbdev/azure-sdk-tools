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
using Microsoft.Azure.Commands.Resources.Models.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.ActiveDirectory
{
    /// <summary>
    /// Get AD groups members.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureADGroupMember", DefaultParameterSetName = ParameterSet.Empty), OutputType(typeof(List<PSADObject>))]
    public class GetAzureADGroupMemberCommand : ActiveDirectoryBaseCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The user email address.")]
        [ValidateNotNullOrEmpty]
        public Guid GroupObjectId { get; set; }

        public override void ExecuteCmdlet()
        {
            ADObjectFilterOptions options = new ADObjectFilterOptions
            {
                Id = GroupObjectId == Guid.Empty ? null : GroupObjectId.ToString(),
                Paging = true
            };

            do
            {
                List<PSADObject> groupMembers = ActiveDirectoryClient.GetGroupMembers(options);
                List<PSObject> output = new List<PSObject>();

                groupMembers.ForEach(m => output.Add(base.ConstructPSObject(
                    null,
                    "Name", m.DisplayName,
                    "Type", m.Type,
                    "Id", m.Id,
                    "UserPrincipalName", m.GetProperty("UserPrincipalName"))));
                WriteObject(output, true);
            } while (!string.IsNullOrEmpty(options.NextLink));
        }
    }
}