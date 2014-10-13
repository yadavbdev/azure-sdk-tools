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
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using Xunit;

namespace Microsoft.Azure.Commands.Batch.Test.Accounts
{
    public class SetBatchAccountCommandTests
    {
        private SetBatchAccountCommand cmdlet;
        private Mock<BatchClient> batchClientMock;
        private Mock<ICommandRuntime> commandRuntimeMock;

        public SetBatchAccountCommandTests()
        {
            batchClientMock = new Mock<BatchClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new SetBatchAccountCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                BatchClient = batchClientMock.Object
            };
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void ReplaceTagsTest()
        {
            List<BatchAccountContext> pipelineOutput = new List<BatchAccountContext>();

            string accountName = "account01";
            string resourceGroup = "resourceGroup";
            AccountResource resourceWithoutTags = BatchTestHelpers.CreateAccountResource(accountName, resourceGroup);
            Hashtable[] tags = new[]
            {
                new Hashtable
                {
                    {"Name", "tagName"},
                    {"Value", "tagValue"}
                }
            };
            AccountResource resourceWithTags = BatchTestHelpers.CreateAccountResource(accountName, resourceGroup, tags);

            BatchAccountGetResponse getResponse = new BatchAccountGetResponse() { Resource = resourceWithoutTags };
            batchClientMock.Setup(b => b.GetAccount(resourceGroup, accountName)).Returns(getResponse);
            BatchAccountCreateResponse createResponse = new BatchAccountCreateResponse() { Resource = resourceWithTags };
            batchClientMock.Setup(b => b.CreateAccount(resourceGroup, accountName, It.IsAny<BatchAccountCreateParameters>())).Returns(createResponse);

            BatchAccountContext expected = BatchAccountContext.ConvertAccountResourceToNewAccountContext(resourceWithTags);

            cmdlet.AccountName = accountName;
            cmdlet.ResourceGroupName = resourceGroup;
            cmdlet.ReplaceTags = true;
            cmdlet.Tag = tags;

            commandRuntimeMock.Setup(r => r.WriteObject(It.IsAny<BatchAccountContext>())).Callback<object>(o => BatchTestHelpers.WriteValueToPipeline<BatchAccountContext>((BatchAccountContext)o, pipelineOutput));

            cmdlet.ExecuteCmdlet();

            Assert.Equal<int>(1, pipelineOutput.Count);
            BatchTestHelpers.AssertBatchAccountContextsAreEqual(expected, pipelineOutput[0]);
        }


        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void UpdateBatchAccountWithResourceLookup()
        {
            UpdateAccountTest(true);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void UpdateBatchAccountWithoutResourceLookup()
        {
            UpdateAccountTest(false);
        }

        private void UpdateAccountTest(bool lookupAccountResource)
        {
            List<BatchAccountContext> pipelineOutput = new List<BatchAccountContext>();

            string accountName = "account01";
            string resourceGroup = "resourceGroup";
            AccountResource accountResource = BatchTestHelpers.CreateAccountResource(accountName, resourceGroup);

            BatchAccountUpdateResponse updateResponse = new BatchAccountUpdateResponse() { Resource = accountResource };
            batchClientMock.Setup(b => b.UpdateAccount(resourceGroup, accountName, It.IsAny<BatchAccountUpdateParameters>())).Returns(updateResponse);

            BatchAccountContext expected = BatchAccountContext.ConvertAccountResourceToNewAccountContext(accountResource);

            cmdlet.AccountName = accountName;
            cmdlet.ReplaceTags = false;
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
