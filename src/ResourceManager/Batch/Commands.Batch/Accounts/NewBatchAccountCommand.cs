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
    using Microsoft.WindowsAzure;
    using Microsoft.Azure.Management.Batch.Models;
    using Microsoft.Azure.Commands.Batch.Properties;

    [Cmdlet(VerbsCommon.New, "AzureBatchAccount"), OutputType(typeof(BatchAccountContext))]
    public class NewBatchAccountCommand : BatchCmdletBase
    {
        private const string mamlRestName = "NewAccount";

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the Batch service account to create.")]
        [Alias("Name")]
        [ValidateNotNullOrEmpty]
        public string AccountName { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The region where the account will be created.")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the resource group where the account will be created.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Alias("Tags")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "An array of hashtables which represents account tags.")]
        public Hashtable[] Tag { get; set; }

        public override void ExecuteCmdlet()
        {
            // use the group lookup to validate whether account already exists. We don't care about the returned
            // group name nor the exception
            WriteVerboseWithTimestamp(Resources.NBA_LookupAccount);
            if (BatchClient.GetGroupForAccountNoThrow(this.AccountName) != null)
            {
                throw new CloudException(Resources.NBA_AccountAlreadyExists);
            }

            WriteVerboseWithTimestamp(Resources.BeginMAMLCall, mamlRestName);

            Dictionary<string, string> tagDictionary = Helpers.CreateTagDictionary(Tag, validate: true);

            var response = BatchClient.CreateAccount(this.ResourceGroupName, this.AccountName, new BatchAccountCreateParameters()
                {
                    Location = this.Location,
                    Tags = tagDictionary
                });

            WriteVerboseWithTimestamp(Resources.EndMAMLCall, mamlRestName);

            var context = BatchAccountContext.CrackAccountResourceToNewAccountContext(response.Resource);
            WriteObject(context);
        }
    }
}
