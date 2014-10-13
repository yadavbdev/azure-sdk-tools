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

using Microsoft.Azure.Management.Batch.Models;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Moq;
using System.Collections.Generic;
using System.Management.Automation;
using Xunit;

namespace Microsoft.Azure.Commands.Batch.Test.Accounts
{
    public class GetBatchAccountKeysCommandTests
    {
        private GetBatchAccountKeysCommand cmdlet;
        private Mock<BatchClient> batchClientMock;
        private Mock<ICommandRuntime> commandRuntimeMock;

        public GetBatchAccountKeysCommandTests()
        {
            batchClientMock = new Mock<BatchClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new GetBatchAccountKeysCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                BatchClient = batchClientMock.Object
            };
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void GetBatchAccountKeysWithResourceLookup()
        {
            GetBatchAccountKeysTest(true);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void GetBatchAccountKeysWithoutResourceLookup()
        {
            GetBatchAccountKeysTest(false);
        }

        private void GetBatchAccountKeysTest(bool lookupAccountResource)
        {
            List<BatchAccountContext> pipelineOutput = new List<BatchAccountContext>();
            string primaryKey = "pKey";
            string secondaryKey = "sKey";

            string accountName = "account01";
            string resourceGroup = "resourceGroup";
            AccountResource accountResource = BatchTestHelpers.CreateAccountResource(accountName, resourceGroup);

            BatchAccountGetResponse getResponse = new BatchAccountGetResponse() { Resource = accountResource };
            batchClientMock.Setup(b => b.GetAccount(resourceGroup, accountName)).Returns(getResponse);

            BatchAccountListKeyResponse keyResponse = new BatchAccountListKeyResponse() { PrimaryKey = primaryKey, SecondaryKey = secondaryKey };
            batchClientMock.Setup(b => b.ListKeys(resourceGroup, accountName)).Returns(keyResponse);

            BatchAccountContext expected = BatchAccountContext.ConvertAccountResourceToNewAccountContext(accountResource);
            expected.PrimaryAccountKey = primaryKey;
            expected.SecondaryAccountKey = secondaryKey;

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
