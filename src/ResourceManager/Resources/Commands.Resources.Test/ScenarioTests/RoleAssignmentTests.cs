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


using Microsoft.Azure.Graph.RBAC.Models;
using Microsoft.Azure.Graph.RBAC;
using Microsoft.Azure.Utilities.HttpRecorder;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Testing;
using Xunit;

namespace Microsoft.Azure.Commands.Resources.Test.ScenarioTests
{
    public class RoleAssignmentTests : ResourcesTestsBase
    {
        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RaNegativeScenarios()
        {
            RunPowerShellTest("Test-RaNegativeScenarios");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RaByScope()
        {
            RunPowerShellTest("Test-RaByScope");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RaByResourceGroup()
        {
            RunPowerShellTest("Test-RaByResourceGroup");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RaByResource()
        {
            RunPowerShellTest("Test-RaByResource");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RaByServicePrincipal()
        {
            RunPowerShellTest("Test-RaByServicePrincipal");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RaByUpn()
        {
            RunPowerShellTest("Test-RaByUpn");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void RaUserPermissions()
        {
            const string scriptMethod = "Test-RaUserPermissions '{0}' '{1}' '{2}'";
            User newUser = null;

            RunPowerShellTest(() =>
                {
                    var name = TestUtilities.GenerateName("aduser");
                    var pass = TestUtilities.GenerateName("adpass")+"0#$";
                    var upn = name + "@" + base.UserDomain;
                    var permission = "*/reader";

                    var parameter = new UserCreateParameters
                    {
                        UserPrincipalName = upn,
                        DisplayName = name,
                        AccountEnabled = true,
                        MailNickname = name + "test",
                        PasswordProfileSettings = new UserCreateParameters.PasswordProfile
                                                    {
                                                        ForceChangePasswordNextLogin = false,
                                                        Password = pass
                                                    }
                    };

                    newUser = base.GraphClient.User.Create(parameter).User;

                    return new[] { string.Format(scriptMethod, name, pass, permission) };
                },
                () =>
                {
                    if(newUser != null)
                    {
                        base.GraphClient.User.Delete(newUser.ObjectId);
                    }
                });

            //RunPowerShellTest("Test-RaUserPermissions 'user' 'pass' 'actionToVerify e.g. */reader'");
        }
    }
}
