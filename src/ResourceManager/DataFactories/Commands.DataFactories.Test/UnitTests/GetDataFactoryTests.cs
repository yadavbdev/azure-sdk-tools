using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Commands.DataFactories.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Azure.Commands.DataFactories.Test
{
    [TestClass]
    public class GetDataFactoryTests : DataFactoriesUnitTestsBase
    {
        private GetAzureDataFactoryCommand _cmdlet;

        [TestInitialize]
        public override void SetupTest()
        {
            base.SetupTest();

            this._cmdlet = new GetAzureDataFactoryCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                DataFactoryClient = this.dataFactoriesClientMock.Object,
                ResourceGroupName = ResourceGroupName
            };
        }

        [TestMethod]
        public void CanGetDataFactory()
        {
            // Arrange
            PSDataFactory expectedOutput = new PSDataFactory()
            {
                DataFactoryName = DataFactoryName,
                Location = Location
            };

            this.dataFactoriesClientMock.Setup(f => f.GetDataFactory(ResourceGroupName, DataFactoryName))
                                    .Returns(expectedOutput);

            this._cmdlet.Name = DataFactoryName;

            // Action
            this._cmdlet.ExecuteCmdlet();

            // Assert
            this.dataFactoriesClientMock.Verify(f => f.GetDataFactory(ResourceGroupName, DataFactoryName), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(expectedOutput), Times.Once());
        }

        [TestMethod]
        public void CanListDataFactories()
        {
            // Arrange
            var expectedOutputs = new List<PSDataFactory>()
                                      {
                                          new PSDataFactory()
                                              {
                                                  DataFactoryName = DataFactoryName,
                                                  Location = Location
                                              },

                                          new PSDataFactory()
                                              {
                                                  DataFactoryName = "foo2",
                                                  Location = Location
                                              }
                                      };


            this.dataFactoriesClientMock.Setup(f => f.ListDataFactories(ResourceGroupName))
                                    .Returns(expectedOutputs);

            // Action
            this._cmdlet.ExecuteCmdlet();

            // Assert
            this.dataFactoriesClientMock.Verify(f => f.ListDataFactories(ResourceGroupName), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(expectedOutputs, true), Times.Once());
        }
    }
}
