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

using System;
using System.Globalization;
using Microsoft.Azure.Commands.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Azure.Commands.DataFactories.Test.UnitTests
{
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
        
        public NewLinkedServiceTests()
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

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void CanCreateLinkedService()
        {
            CreateLinkedService(rawJsonContent);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void InvalidJsonLinkedService()
        {
            try
            {
                string malformedJson = rawJsonContent.Replace(":", "-");

                CreateLinkedService(malformedJson);

                throw new Exception(
                    string.Format(CultureInfo.InvariantCulture,
                    "Test case failed: Was able to deploy linked service with malformed json content {0}",
                    malformedJson));
            }
            catch (JsonSerializationException)
            {
            }
        }

        private void CreateLinkedService(string rawLinkedServiceJsonContent)
        {
            // Arrange
            LinkedService expected = new LinkedService()
            {
                Name = linkedServiceName,
                Properties = null
            };

            dataFactoriesClientMock.Setup(c => c.ReadJsonFileContent(It.IsAny<string>()))
                .Returns(rawLinkedServiceJsonContent)
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
                    c.CreateOrUpdateLinkedService(ResourceGroupName, DataFactoryName, linkedServiceName, rawLinkedServiceJsonContent))
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
