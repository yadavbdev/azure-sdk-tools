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

namespace Microsoft.Azure.Commands.Batch
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.Azure.Management.Batch.Models;
    using Properties;

    [Cmdlet(VerbsCommon.Set, "AzureBatchAccount"), OutputType(typeof(BatchAccountContext))]
    public class SetBatchAccountCommand : BatchCmdletBase
    {
        private const string mamlRestName = "SetAccount";

        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the Batch service account to update.")]
        [Alias("Name")]
        [ValidateNotNullOrEmpty]
        public string AccountName { get; set; }

        [Alias("Tags")]
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "An array of hashtables which represents the account tags to be replaced or updated.")]
        public Hashtable[] Tag { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group of the account being updated.")]
        public string ResourceGroupName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Overwrite the current set of tags associated with this resource.")]
        public SwitchParameter ReplaceTags { get; set; }

        public override void ExecuteCmdlet()
        {
            string accountName = this.AccountName;
            string resourceGroupName = this.ResourceGroupName;

            if (string.IsNullOrEmpty(resourceGroupName))
            {
                // use resource mgr to see if account exists and then use resource group name to do the actual lookup
                WriteVerboseWithTimestamp(String.Format(Resources.ResGroupLookup, accountName));
                resourceGroupName = BatchClient.GetGroupForAccount(accountName);
            }

            WriteVerboseWithTimestamp(Resources.SBA_Updating, accountName);
            WriteVerboseWithTimestamp(Resources.BeginMAMLCall, mamlRestName);

            BatchAccountContext context = null;
            Dictionary<string, string> tagDictionary = Helpers.CreateTagDictionary(Tag, validate: true);

            if (ReplaceTags.IsPresent)
            {
                // need to the location in order to call 
                var getResponse = BatchClient.GetAccount(resourceGroupName, accountName);

                var response = BatchClient.CreateAccount(resourceGroupName, accountName, new BatchAccountCreateParameters()
                {
                    Location = getResponse.Resource.Location,
                    Tags = tagDictionary
                });

                context = BatchAccountContext.CrackAccountResourceToNewAccountContext(response.Resource);
            }
            else
            {
                var response = BatchClient.UpdateAccount(resourceGroupName, accountName, new BatchAccountUpdateParameters()
                {
                    Tags = tagDictionary
                });

                context = BatchAccountContext.CrackAccountResourceToNewAccountContext(response.Resource);
            }

            WriteVerboseWithTimestamp(Resources.EndMAMLCall, mamlRestName);

            WriteObject(context);
        }
    }
}
