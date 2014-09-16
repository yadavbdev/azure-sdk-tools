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

using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Xunit;

namespace Microsoft.Azure.Commands.Resources.Test.ScenarioTests
{
    public class ActiveDirectoryTests : ResourcesTestsBase
    {
        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetAllADGroups()
        {
            RunPowerShellTest("Test-GetAllADGroups");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupWithSearchString()
        {
            RunPowerShellTest("Test-GetADGroupWithSearchString");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupWithBadSearchString()
        {
            RunPowerShellTest("Test-GetADGroupWithBadSearchString");
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
            RunPowerShellTest("Test-GetADGroupWithUserObjectId");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupMemberWithGroupObjectId()
        {
            RunPowerShellTest("Test-GetADGroupMemberWithGroupObjectId");
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
            RunPowerShellTest("Test-GetADGroupMemberWithUserObjectId");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADGroupMemberFromEmptyGroup()
        {
            RunPowerShellTest("Test-GetADGroupMemberFromEmptyGroup");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADServicePrincipalWithObjectId()
        {
            RunPowerShellTest("Test-GetADServicePrincipalWithObjectId");
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
            RunPowerShellTest("Test-GetADServicePrincipalWithUserObjectId");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADServicePrincipalWithSPN()
        {
            RunPowerShellTest("Test-GetADServicePrincipalWithSPN");
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
            RunPowerShellTest("Test-GetADServicePrincipalWithSearchString");
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
            RunPowerShellTest("Test-GetAllADUser");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADUserWithObjectId()
        {
            RunPowerShellTest("Test-GetADUserWithObjectId");
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
            RunPowerShellTest("Test-GetADUserWithGroupObjectId");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADUserWithUPN()
        {
            RunPowerShellTest("Test-GetADUserWithUPN");
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
            RunPowerShellTest("Test-GetADUserWithSearchString");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestGetADUserWithBadSearchString()
        {
            RunPowerShellTest("Test-GetADUserWithBadSearchString");
        }
    }
}