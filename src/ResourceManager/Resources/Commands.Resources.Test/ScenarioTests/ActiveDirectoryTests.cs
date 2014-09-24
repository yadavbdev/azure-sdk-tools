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

using Microsoft.Azure.Graph.RBAC;
using Microsoft.Azure.Graph.RBAC.Models;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Testing;
using System.Linq;
using Xunit;

namespace Microsoft.Azure.Commands.Resources.Test.ScenarioTests
{
    public class ActiveDirectoryTests : ResourcesTestsBase
    {
        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetAllADGroups()
        {
            const string scriptMethod = "Test-GetAllADGroups";
            Group newGroup = null;

            RunPowerShellTest(() =>
            {
                newGroup = CreateNewAdGroup();
                return new[] { scriptMethod };
            }, () =>
                {
                    DeleteAdGroup(newGroup);
                });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupWithSearchString()
        {
            const string scriptMethod = "Test-GetADGroupWithSearchString '{0}'";
            Group newGroup = null;

            RunPowerShellTest(() =>
            {
                newGroup = CreateNewAdGroup();
                return new[] { string.Format(scriptMethod, newGroup.DisplayName) };
            }, () =>
                {
                    DeleteAdGroup(newGroup);
                });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupWithBadSearchString()
        {
            RunPowerShellTest("Test-GetADGroupWithBadSearchString");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupWithObjectId()
        {
            const string scriptMethod = "Test-GetADGroupWithObjectId '{0}'";
            Group newGroup = null;

            RunPowerShellTest(() =>
            {
                newGroup = CreateNewAdGroup();
                return new[] { string.Format(scriptMethod, newGroup.ObjectId) };
            }, () =>
            {
                DeleteAdGroup(newGroup);
            });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupWithBadObjectId()
        {
            RunPowerShellTest("Test-GetADGroupWithBadObjectId");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupWithUserObjectId()
        {
            const string scriptMethod = "Test-GetADGroupWithUserObjectId '{0}'";
            User newUser = null;
            RunPowerShellTest(() =>
                {
                    newUser = CreateNewAdUser();
                    return new[] { string.Format(scriptMethod, newUser.ObjectId) };
                }, () =>
                {
                    DeleteAdUser(newUser);
                });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupMemberWithGroupObjectId()
        {
            const string scriptMethod = "Test-GetADGroupMemberWithGroupObjectId '{0}' '{1}' '{2}'";
            User newUser = null;
            Group newGroup = null;
            RunPowerShellTest(() =>
                {
                    newUser = CreateNewAdUser();
                    newGroup = CreateNewAdGroup();

                    string memberUrl = string.Format("{0}{1}/directoryObjects/{2}",
                        GraphClient.BaseUri.AbsoluteUri, GraphClient.TenantID, newUser.ObjectId);
                    
                    base.GraphClient.Group.AddMember(newGroup.ObjectId, new GroupAddMemberParameters(memberUrl));

                    return new[] { string.Format(scriptMethod, newGroup.ObjectId, newUser.ObjectId, newUser.DisplayName) };
                }, () =>
                {
                    DeleteAdUser(newUser);
                    DeleteAdGroup(newGroup);
                });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupMemberWithBadGroupObjectId()
        {
            RunPowerShellTest("Test-GetADGroupMemberWithBadGroupObjectId");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupMemberWithUserObjectId()
        {
            const string scriptMethod = "Test-GetADGroupMemberWithUserObjectId '{0}'";
            User newUser = null;
            RunPowerShellTest(() =>
            {
                newUser = CreateNewAdUser();
                return new[] { string.Format(scriptMethod, newUser.ObjectId) };
            }, () =>
            {
                DeleteAdUser(newUser);
            });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupMemberFromEmptyGroup()
        {
            const string scriptMethod = "Test-GetADGroupMemberFromEmptyGroup '{0}'";
            Group newGroup = null;
            RunPowerShellTest(() =>
            {
                newGroup = CreateNewAdGroup();
                return new[] { string.Format(scriptMethod, newGroup.ObjectId) };
            }, () =>
            {
                DeleteAdGroup(newGroup);
            });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADServicePrincipalWithObjectId()
        {
            const string scriptMethod = "Test-GetADServicePrincipalWithObjectId '{0}'";
            ServicePrincipal newServicePrincipal = null;
            Application app = null;
            RunPowerShellTest(() =>
                {
                    app = CreateNewAdApp();
                    newServicePrincipal = CreateNewAdServicePrincipal(app.AppId);
                    return new[] { string.Format(scriptMethod, newServicePrincipal.ObjectId) };
                }, () =>
                {
                    DeleteAdServicePrincipal(newServicePrincipal);
                    DeleteAdApp(app);
                });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADServicePrincipalWithBadObjectId()
        {
            RunPowerShellTest("Test-GetADServicePrincipalWithBadObjectId");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADServicePrincipalWithUserObjectId()
        {
            const string scriptMethod = "Test-GetADServicePrincipalWithUserObjectId '{0}'";
            User newUser = null;
            RunPowerShellTest(() =>
            {
                newUser = CreateNewAdUser();
                return new[] { string.Format(scriptMethod, newUser.ObjectId) };
            }, () =>
            {
                DeleteAdUser(newUser);
            });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADServicePrincipalWithSPN()
        {
            const string scriptMethod = "Test-GetADServicePrincipalWithSPN '{0}'";
            ServicePrincipal newServicePrincipal = null;
            Application app = null;
            RunPowerShellTest(() =>
            {
                app = CreateNewAdApp();
                newServicePrincipal = CreateNewAdServicePrincipal(app.AppId);
                return new[] { string.Format(scriptMethod, newServicePrincipal.ServicePrincipalNames[1]) };
            }, () =>
            {
                DeleteAdServicePrincipal(newServicePrincipal);
                DeleteAdApp(app);
            });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADServicePrincipalWithBadSPN()
        {
            RunPowerShellTest("Test-GetADServicePrincipalWithBadSPN");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADServicePrincipalWithSearchString()
        {
            const string scriptMethod = "Test-GetADServicePrincipalWithSearchString '{0}'";
            ServicePrincipal newServicePrincipal = null;
            Application app = null;
            RunPowerShellTest(() =>
            {
                app = CreateNewAdApp();
                newServicePrincipal = CreateNewAdServicePrincipal(app.AppId);
                return new[] { string.Format(scriptMethod, newServicePrincipal.DisplayName) };
            }, () =>
            {
                DeleteAdServicePrincipal(newServicePrincipal);
                DeleteAdApp(app);
            });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADServicePrincipalWithBadSearchString()
        {
            RunPowerShellTest("Test-GetADServicePrincipalWithBadSearchString");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetAllADUser()
        {
            const string scriptMethod = "Test-GetAllADUser";
            User newUser = null;
            RunPowerShellTest(() =>
            {
                newUser = CreateNewAdUser();
                return new[] { string.Format(scriptMethod) };
            }, () =>
            {
                DeleteAdUser(newUser);
            });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADUserWithObjectId()
        {
            const string scriptMethod = "Test-GetADUserWithObjectId '{0}'";
            User newUser = null;
            RunPowerShellTest(() =>
            {
                newUser = CreateNewAdUser();
                return new[] { string.Format(scriptMethod, newUser.ObjectId) };
            }, () =>
            {
                DeleteAdUser(newUser);
            });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADUserWithBadObjectId()
        {
            RunPowerShellTest("Test-GetADUserWithBadObjectId");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADUserWithGroupObjectId()
        {
            const string scriptMethod = "Test-GetADUserWithGroupObjectId '{0}'";
            Group newGroup = null;
            RunPowerShellTest(() =>
            {
                newGroup = CreateNewAdGroup();
                return new[] { string.Format(scriptMethod, newGroup.ObjectId) };
            }, () =>
            {
                DeleteAdGroup(newGroup);
            });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADUserWithUPN()
        {
            const string scriptMethod = "Test-GetADUserWithUPN '{0}'";
            User newUser = null;
            RunPowerShellTest(() =>
            {
                newUser = CreateNewAdUser();
                return new[] { string.Format(scriptMethod, newUser.UserPrincipalName) };
            }, () =>
            {
                DeleteAdUser(newUser);
            });
        }

        [Fact(Skip = "Currently not working.")]
        public void TestGetADUserWithFPOUPN()
        {
            RunPowerShellTest("Test-GetADUserWithFPOUPN");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADUserWithBadUPN()
        {
            RunPowerShellTest("Test-GetADUserWithBadUPN");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADUserWithSearchString()
        {
            const string scriptMethod = "Test-GetADUserWithSearchString '{0}'";
            User newUser = null;
            RunPowerShellTest(() =>
            {
                newUser = CreateNewAdUser();
                return new[] { string.Format(scriptMethod, newUser.DisplayName) };
            }, () =>
            {
                DeleteAdUser(newUser);
            });
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADUserWithBadSearchString()
        {
            RunPowerShellTest("Test-GetADUserWithBadSearchString");
        }

        private User CreateNewAdUser()
        {
            var name = TestUtilities.GenerateName("aduser");
            var parameter = new UserCreateParameters
            {
                DisplayName = name,
                UserPrincipalName = name + "@" + base.UserDomain,
                AccountEnabled = true,
                MailNickname = name + "test",
                PasswordProfileSettings = new UserCreateParameters.PasswordProfile
                {
                    ForceChangePasswordNextLogin = false,
                    Password = TestUtilities.GenerateName("adpass") + "0#$"
                }
            };

            return base.GraphClient.User.Create(parameter).User;
        }

        private Group CreateNewAdGroup()
        {
            var parameter = new GroupCreateParameters
            {
                DisplayName = TestUtilities.GenerateName("adgroup"),
                MailNickname = TestUtilities.GenerateName("adgroupmail"),
                SecurityEnabled = true
            };
            return base.GraphClient.Group.Create(parameter).Group;
        }

        private Application CreateNewAdApp()
        {
            var appName = TestUtilities.GenerateName("adApplication");
            var url = string.Format("http://{0}/home", appName);
            var appParam = new ApplicationCreateParameters
            {
                AvailableToOtherTenants = false,
                DisplayName = appName,
                Homepage = url,
                IdentifierUris = new[] { url },
                ReplyUrls = new[] { url }
            };

            return base.GraphClient.Application.Create(appParam).Application;
        }

        private ServicePrincipal CreateNewAdServicePrincipal(string appId)
        {
            var spParam = new ServicePrincipalCreateParameters
            {
                AppId = appId,
                AccountEnabled = true
            };

            return base.GraphClient.ServicePrincipal.Create(spParam).ServicePrincipal;
        }

        private void DeleteAdUser(User user)
        {
            if (user != null)
            {
                base.GraphClient.User.Delete(user.ObjectId);
            }
        }
        private void DeleteAdGroup(Group group)
        {
            if (group != null)
            {
                base.GraphClient.Group.Delete(group.ObjectId);
            }
        }
        private void DeleteAdApp(Application app)
        {
            if (app != null)
            {
                base.GraphClient.Application.Delete(app.ObjectId);
            }
        }

        private void DeleteAdServicePrincipal(ServicePrincipal newServicePrincipal)
        {
            if (newServicePrincipal != null)
            {
                base.GraphClient.ServicePrincipal.Delete(newServicePrincipal.ObjectId);
            }
        }
    }
}