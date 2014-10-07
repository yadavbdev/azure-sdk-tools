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

namespace Microsoft.Azure.Commands.Batch.Test.Accounts
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.Azure.Commands.Batch;
    using Microsoft.Azure.Management.Batch.Models;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Commands.ScenarioTest;
    using Moq;
    using Xunit;

    public class RegenBatchAccountKeyCommandTests
    {
        private RegenBatchAccountKeyCommand cmdlet;
        private Mock<BatchClient> batchClientMock;
        private Mock<ICommandRuntime> commandRuntimeMock;

        public RegenBatchAccountKeyCommandTests()
        {
            batchClientMock = new Mock<BatchClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new RegenBatchAccountKeyCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                BatchClient = batchClientMock.Object
            };
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RegenBatchAccountKeysWithResourceLookup()
        {
            RegenBatchAccountKeysTest(true);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RegenBatchAccountKeysWithoutResourceLookup()
        {
            RegenBatchAccountKeysTest(false);
        }

        private void RegenBatchAccountKeysTest(bool lookupAccountResource)
        {
            List<BatchAccountContext> pipelineOutput = new List<BatchAccountContext>();
            string newPrimaryKey = "newPrimaryKey";
            string newSecondaryKey = "newSecondaryKey";

            string accountName = "account01";
            string resourceGroup = "resourceGroup";
            AccountResource accountResource = BatchTestHelpers.CreateAccountResource(accountName, resourceGroup);

            BatchAccountGetResponse getResponse = new BatchAccountGetResponse() { Resource = accountResource };
            batchClientMock.Setup(b => b.GetAccount(resourceGroup, accountName)).Returns(getResponse);

            BatchAccountRegenerateKeyResponse keyResponse = new BatchAccountRegenerateKeyResponse() { PrimaryKey = newPrimaryKey, SecondaryKey = newSecondaryKey };
            batchClientMock.Setup(b => b.RegenerateKeys(resourceGroup, accountName, It.IsAny<BatchAccountRegenerateKeyParameters>())).Returns(keyResponse);

            BatchAccountContext expected = BatchAccountContext.CrackAccountResourceToNewAccountContext(accountResource);
            expected.PrimaryAccountKey = newPrimaryKey;
            expected.SecondaryAccountKey = newSecondaryKey;

            cmdlet.AccountName = accountName;

            if (lookupAccountResource)
            {
                cmdlet.ResourceGroupName = null;
                batchClientMock.Setup(b => b.GetGroupForAccountNoThrow(accountName)).Returns(resourceGroup);
            }
            else
            {
                cmdlet.ResourceGroupName = resourceGroup;
            }
            commandRuntimeMock.Setup(r => r.WriteObject(It.IsAny<BatchAccountContext>())).Callback<object>(o => BatchTestHelpers.WriteValueToPipeline<BatchAccountContext>((BatchAccountContext)o, pipelineOutput));

            cmdlet.ExecuteCmdlet();

            Assert.Equal<int>(1, pipelineOutput.Count);
            BatchTestHelpers.AssertBatchAccountContextsAreEqual(expected, pipelineOutput[0]);
        }
    }
}
