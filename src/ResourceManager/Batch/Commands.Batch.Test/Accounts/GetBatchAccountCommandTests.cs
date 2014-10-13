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
    public class GetBatchAccountCommandTests
    {
        private GetBatchAccountCommand cmdlet;
        private Mock<BatchClient> batchClientMock;
        private Mock<ICommandRuntime> commandRuntimeMock;

        public GetBatchAccountCommandTests()
        {
            batchClientMock = new Mock<BatchClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new GetBatchAccountCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                BatchClient = batchClientMock.Object
            };
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void ListBatchAccountsTest()
        {
            List<BatchAccountContext> pipelineOutput = new List<BatchAccountContext>();

            string accountName01 = "account01";
            string resourceGroup = "resourceGroup";
            AccountResource accountResource01 = BatchTestHelpers.CreateAccountResource(accountName01, resourceGroup);
            string nextLink = "nextLink";
            BatchAccountListResponse listResponse = new BatchAccountListResponse() { Accounts = new List<AccountResource>() { accountResource01 }, NextLink = nextLink };
            batchClientMock.Setup(b => b.ListAccounts(It.IsAny<AccountListParameters>())).Returns(listResponse);

            string accountName02 = "account02";
            AccountResource accountResource02 = BatchTestHelpers.CreateAccountResource(accountName02, resourceGroup);
            BatchAccountListResponse nextListResponse = new BatchAccountListResponse() { Accounts = new List<AccountResource>() { accountResource02 }, NextLink = null };
            batchClientMock.Setup(b => b.ListNextAccounts(nextLink)).Returns(nextListResponse);

            BatchAccountContext expected01 = BatchAccountContext.ConvertAccountResourceToNewAccountContext(accountResource01);
            BatchAccountContext expected02 = BatchAccountContext.ConvertAccountResourceToNewAccountContext(accountResource02);

            cmdlet.AccountName = null;
            cmdlet.ResourceGroupName = resourceGroup;
            commandRuntimeMock.Setup(r => r.WriteObject(It.IsAny<BatchAccountContext>())).Callback<object>(o => BatchTestHelpers.WriteValueToPipeline<BatchAccountContext>((BatchAccountContext)o, pipelineOutput));

            cmdlet.ExecuteCmdlet();

            Assert.Equal<int>(2, pipelineOutput.Count);
            BatchTestHelpers.AssertBatchAccountContextsAreEqual(expected01, pipelineOutput[0]);
            BatchTestHelpers.AssertBatchAccountContextsAreEqual(expected02, pipelineOutput[1]);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void GetBatchAccountWithResourceLookup()
        {
            GetBatchAccountTest(true);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void GetBatchAccountWithoutResourceLookup()
        {
            GetBatchAccountTest(false);
        }

        private void GetBatchAccountTest(bool lookupAccountResource)
        {
            List<BatchAccountContext> pipelineOutput = new List<BatchAccountContext>();

            string accountName = "account01";
            string resourceGroup = "resourceGroup";
            AccountResource accountResource = BatchTestHelpers.CreateAccountResource(accountName, resourceGroup);

            BatchAccountGetResponse getResponse = new BatchAccountGetResponse() { Resource = accountResource };
            batchClientMock.Setup(b => b.GetAccount(resourceGroup, accountName)).Returns(getResponse);

            BatchAccountContext expected = BatchAccountContext.ConvertAccountResourceToNewAccountContext(accountResource);

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
