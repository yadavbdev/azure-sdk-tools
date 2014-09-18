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

using System.Collections.Generic;
using Microsoft.Azure.Commands.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Common.Test.Mocks;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Moq;

namespace Microsoft.Azure.Commands.DataFactories.Test.UnitTests
{
    [TestClass]
    public class NewLinkedServiceTests : DataFactoryUnitTestBase
    {
        private const string linkedServiceName = "foo1";

        private const string filePath = "linkedService.json";

        private const string rawJsonContent = @"
{
    name: ""foo2"",
    properties:
    {
        type: ""HDInsightBYOCLinkedService"",
        clusterUri: ""https://MyCluster.azurehdinsight.net/"",
        userName: ""MyUserName"",
        password: ""$EncryptedString$MyEncryptedPassword"",
        linkedServiceName: ""MyStorageAssetName"",
    }
}
";

        private NewAzureDataFactoryLinkedServiceCommand cmdlet;
        
        [TestInitialize]
        public override void SetupTest()
        {
            base.SetupTest();

        cmdlet = new NewAzureDataFactoryLinkedServiceCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                DataFactoryClient = dataFactoriesClientMock.Object,
                Name = linkedServiceName,
                DataFactoryName = DataFactoryName,
                ResourceGroupName = ResourceGroupName
            };
        }

        [TestMethod]
        public void CanCreateLinkedService()
        {
            // Arrange
            LinkedService expected = new LinkedService()
            {
                Name = linkedServiceName,
                Properties = null
            };
            
            dataFactoriesClientMock.Setup(c => c.ReadJsonFileContent(It.IsAny<string>()))
                .Returns(rawJsonContent)
                .Verifiable();

            dataFactoriesClientMock.Setup(
                c =>
                    c.CreatePSLinkedService(
                        It.Is<CreatePSLinkedServiceParameters>(
                            parameters =>
                                parameters.Name == linkedServiceName &&
                                parameters.ResourceGroupName == ResourceGroupName &&
                                parameters.DataFactoryName == DataFactoryName)))
                .CallBase()
                .Verifiable();

            dataFactoriesClientMock.Setup(
                c =>
                    c.CreateOrUpdateLinkedService(ResourceGroupName, DataFactoryName, linkedServiceName, rawJsonContent))
                .Returns(expected)
                .Verifiable();
            
            // Action
            cmdlet.File = filePath;
            cmdlet.Force = true;
            cmdlet.ExecuteCmdlet();

            // Assert
            dataFactoriesClientMock.VerifyAll();

            commandRuntimeMock.Verify(
                f =>
                    f.WriteObject(
                        It.Is<PSLinkedService>(
                            ls =>
                                ResourceGroupName == ls.ResourceGroupName &&
                                DataFactoryName == ls.DataFactoryName &&
                                expected.Name == ls.LinkedServiceName &&
                                expected.Properties == ls.Properties)),
                Times.Once());
        }
    }
}
