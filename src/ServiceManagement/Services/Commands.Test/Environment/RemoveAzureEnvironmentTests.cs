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
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Test.Mocks;
using Microsoft.WindowsAzure.Commands.Profile;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Moq;
using Xunit;

namespace Microsoft.WindowsAzure.Commands.Test.Environment
{
    public class RemoveAzureEnvironmentTests : TestBase, IDisposable
    {
        private MockDataStore dataStore;

        public RemoveAzureEnvironmentTests()
        {
            dataStore = new MockDataStore();
            ProfileClient.DataStore = dataStore;
        }

        public void Cleanup()
        {
            AzureSession.SetCurrentSubscription(null, null);
        }

        [Fact]
        public void RemovesAzureEnvironment()
        {
            var commandRuntimeMock = new Mock<ICommandRuntime>();
            const string name = "test";
            ProfileClient client = new ProfileClient();
            client.AddEnvironment(new AzureEnvironment
            {
                Name = name
            });
            client.Profile.Save();

            var cmdlet = new RemoveAzureEnvironmentCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                Name = name
            };

            cmdlet.ExecuteCmdlet();

            client = new ProfileClient();
            Assert.False(client.Profile.Environments.ContainsKey(name));
        }

        [Fact]
        public void ThrowsForUnknownEnvironment()
        {
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();
            RemoveAzureEnvironmentCommand cmdlet = new RemoveAzureEnvironmentCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                Name = "test2"
            };

            Assert.Throws<KeyNotFoundException>(() => cmdlet.ExecuteCmdlet());
        }

        [Fact]
        public void ThrowsForPublicEnvironment()
        {
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();

            foreach (string name in AzureEnvironment.PublicEnvironments.Keys)
            {
                RemoveAzureEnvironmentCommand cmdlet = new RemoveAzureEnvironmentCommand()
                {
                    CommandRuntime = commandRuntimeMock.Object,
                    Name = name
                };

                Assert.Throws<InvalidOperationException>(() => cmdlet.ExecuteCmdlet());
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}