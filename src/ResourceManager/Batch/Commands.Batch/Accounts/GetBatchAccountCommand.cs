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

using Microsoft.Azure.Commands.Batch.Properties;
using Microsoft.Azure.Management.Batch.Models;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Batch
{
    [Cmdlet(VerbsCommon.Get, "AzureBatchAccount"), OutputType(typeof(BatchAccountContext))]
    public class GetBatchAccountCommand : BatchCmdletBase
    {
        private const string mamlRestName = "Get";

        [Alias("Name")]
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the Batch service account name to query.")]
        [ValidateNotNullOrEmpty]
        public string AccountName { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group associated with the account being queried.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Filter list of accounts using the key and optional value of the specified tag.")]
        public Hashtable Tag { get; set; }

        public override void ExecuteCmdlet()
        {
            string accountName = this.AccountName;
            string resourceGroupName = this.ResourceGroupName;

            if (string.IsNullOrEmpty(accountName))
            {
                // no account name so we're doing some sort of list. If no resource group, then list all accounts under the
                // subscription otherwise all accounts in the resource group.
                if (resourceGroupName == null)
                {
                    WriteVerboseWithTimestamp(Resources.GBA_AllAccounts);
                }
                else
                {
                    WriteVerboseWithTimestamp(Resources.GBA_ResGroupAccounts, resourceGroupName);
                }

                WriteVerboseWithTimestamp(Resources.BeginMAMLCall, mamlRestName);
                var response = BatchClient.ListAccounts(new AccountListParameters { ResourceGroupName = resourceGroupName });
                WriteVerboseWithTimestamp(Resources.EndMAMLCall, mamlRestName);

                // filter out the accounts if a tag was specified
                IList<AccountResource> accounts = new List<AccountResource>();
                if (Tag != null && Tag.Count > 0)
                {
                    accounts = Helpers.FilterAccounts(response.Accounts, Tag);
                }
                else
                {
                    accounts = response.Accounts;
                }

                foreach (AccountResource resource in accounts)
                {
                    var context = BatchAccountContext.ConvertAccountResourceToNewAccountContext(resource);
                    WriteObject(context);
                }

                var nextLink = response.NextLink;

                while (nextLink != null)
                {
                    WriteVerboseWithTimestamp(Resources.BeginMAMLCall, mamlRestName);
                    response = BatchClient.ListNextAccounts(nextLink);
                    WriteVerboseWithTimestamp(Resources.EndMAMLCall, mamlRestName);

                    foreach (AccountResource resource in response.Accounts)
                    {
                        var context = BatchAccountContext.ConvertAccountResourceToNewAccountContext(resource);
                        WriteObject(context);
                    }

                    nextLink = response.NextLink;
                }
            }
            else
            {
                // single account lookup - find its resource group if not specified
                if (string.IsNullOrEmpty(resourceGroupName))
                {
                    // use resource mgr to see if account exists and then use resource group name to do the actual lookup
                    WriteVerboseWithTimestamp(Resources.ResGroupLookup, accountName);
                    resourceGroupName = BatchClient.GetGroupForAccount(accountName);
                }

                WriteVerboseWithTimestamp(Resources.BeginMAMLCall, mamlRestName);
                var response = BatchClient.GetAccount(resourceGroupName, accountName);
                WriteVerboseWithTimestamp(Resources.EndMAMLCall, mamlRestName);

                var context = BatchAccountContext.ConvertAccountResourceToNewAccountContext(response.Resource);
                WriteObject(context);
            }
        }
    }
}
