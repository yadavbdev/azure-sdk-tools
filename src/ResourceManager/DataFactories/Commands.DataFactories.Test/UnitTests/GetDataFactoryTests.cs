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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Azure.Commands.DataFactories.Test
{
    [TestClass]
    public class GetDataFactoryTests : DataFactoryUnitTestBase
    {
        private GetAzureDataFactoryCommand cmdlet;
        
        [TestInitialize]
        public void SetupTest()
        {
            base.SetupTest();

            cmdlet = new GetAzureDataFactoryCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                DataFactoryClient = dataFactoriesClientMock.Object
            };
        }

        [TestMethod]
        public void CanGetDataFactory()
        {
            PSDataFactory expected = new PSDataFactory() {DataFactoryName = DataFactoryName, ResourceGroupName = ResourceGroupName};

            // Arrange
            dataFactoriesClientMock.Setup(
                c =>
                    c.FilterPSDataFactories(
                        It.Is<DataFactoryFilterOptions>(
                            options => options.Name == DataFactoryName && options.ResourceGroupName == ResourceGroupName)))
                .CallBase()
                .Verifiable();

            dataFactoriesClientMock.Setup(c => c.GetDataFactory(ResourceGroupName, DataFactoryName))
                .Returns(expected)
                .Verifiable();

            cmdlet.Name = DataFactoryName;
            cmdlet.ResourceGroupName = ResourceGroupName;
            
            // Action
            cmdlet.ExecuteCmdlet();

            // Assert
            dataFactoriesClientMock.VerifyAll();

            commandRuntimeMock.Verify(f => f.WriteObject(expected), Times.Once());
        }

        [TestMethod]
        public void CanListDataFactories()
        {
            List<PSDataFactory> expected = new List<PSDataFactory>()
            {
                new PSDataFactory() {DataFactoryName = DataFactoryName, ResourceGroupName = ResourceGroupName},
                new PSDataFactory() {DataFactoryName = "datafactory1", ResourceGroupName = ResourceGroupName}
            };

            // Arrange
            dataFactoriesClientMock.Setup(
                c =>
                    c.FilterPSDataFactories(
                        It.Is<DataFactoryFilterOptions>(
                            options => options.Name == null && options.ResourceGroupName == ResourceGroupName)))
                .CallBase()
                .Verifiable();

            dataFactoriesClientMock.Setup(c => c.ListDataFactories(ResourceGroupName))
                .Returns(expected)
                .Verifiable();

            cmdlet.ResourceGroupName = ResourceGroupName;

            // Action
            cmdlet.ExecuteCmdlet();
            
            // Assert
            dataFactoriesClientMock.VerifyAll();

            commandRuntimeMock.Verify(f => f.WriteObject(expected, true), Times.Once());
        }
    }
}
