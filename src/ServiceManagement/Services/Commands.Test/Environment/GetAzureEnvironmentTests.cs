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
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Test.Mocks;
using Microsoft.WindowsAzure.Commands.Profile;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Moq;
using Xunit;

namespace Microsoft.WindowsAzure.Commands.Test.Environment
{
    public class GetAzureEnvironmentTests : TestBase
    {
        private MockDataStore dataStore;

        public GetAzureEnvironmentTests()
        {
            dataStore = new MockDataStore();
            ProfileClient.DataStore = dataStore;
        }

        [Fact]
        public void GetsAzureEnvironments()
        {
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();
            GetAzureEnvironmentCommand cmdlet = new GetAzureEnvironmentCommand()
            {
                CommandRuntime = commandRuntimeMock.Object
            };

            cmdlet.ExecuteCmdlet();

            commandRuntimeMock.Verify(
                f => f.WriteObject(It.IsAny<List<PSObject>>(), true),
                Times.Once());
        }

        [Fact]
        public void GetsAzureEnvironment()
        {
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();
            GetAzureEnvironmentCommand cmdlet = new GetAzureEnvironmentCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                Name = EnvironmentName.AzureChinaCloud
            };

            cmdlet.ExecuteCmdlet();

            commandRuntimeMock.Verify(
                f => f.WriteObject(It.IsAny<AzureEnvironment>()),
                Times.Once());
        }
    }
}