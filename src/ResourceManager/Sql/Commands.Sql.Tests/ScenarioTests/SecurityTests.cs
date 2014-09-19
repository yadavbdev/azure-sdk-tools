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

using Xunit;

namespace Microsoft.WindowsAzure.Commands.ScenarioTest.SqlTests
{
    public class SecurityTests : SqlTestsBase
    {
        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestDatabaseUpdatePolicyWithStorage()
        {
            RunPowerShellTest("Test-DatabaseUpdatePolicyWithStorage");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestServerUpdatePolicyWithStorage()
        {
            RunPowerShellTest("Test-ServerUpdatePolicyWithStorage");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestDatabaseUpdatePolicyWithEventTypes()
        {
            RunPowerShellTest("Test-DatabaseUpdatePolicyWithEventTypes");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestServerUpdatePolicyWithEventTypes()
        {
            RunPowerShellTest("Test-ServerUpdatePolicyWithEventTypes");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestDisableDatabaseAuditing()
        {
            RunPowerShellTest("Test-DisableDatabaseAuditing");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestDisableServerAuditing()
        {
            RunPowerShellTest("Test-DisableServerAuditing");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestDatabaseDisableEnableKeepProperties()
        {
            RunPowerShellTest("Test-DatabaseDisableEnableKeepProperties");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestServerDisableEnableKeepProperties()
        {
            RunPowerShellTest("Test-ServerDisableEnableKeepProperties");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestUseServerDefault()
        {
            RunPowerShellTest("Test-UseServerDefault");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestFailedDatabaseUpdatePolicyWithNoStorage()
        {
            RunPowerShellTest("Test-FailedDatabaseUpdatePolicyWithNoStorage");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestFailedServerUpdatePolicyWithNoStorage()
        {
            RunPowerShellTest("Test-FailedServerUpdatePolicyWithNoStorage");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestFailedUseServerDefault()
        {
            RunPowerShellTest("Test-FailedUseServerDefault");
        }
        
        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestDatabaseUpdatePolicyWithEventTypeShortcuts()
        {
            RunPowerShellTest("Test-DatabaseUpdatePolicyWithEventTypeShortcuts");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestServerUpdatePolicyWithEventTypeShortcuts()
        {
            RunPowerShellTest("Test-ServerUpdatePolicyWithEventTypeShortcuts");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestDatabaseUpdatePolicyKeepPreviousStorage()
        {
            RunPowerShellTest("Test-DatabaseUpdatePolicyKeepPreviousStorage");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestServerUpdatePolicyKeepPreviousStorage()
        {
            RunPowerShellTest("Test-ServerUpdatePolicyKeepPreviousStorage");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestFailWithBadDatabaseIndentity()
        {
            RunPowerShellTest("Test-FailWithBadDatabaseIndentity");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void TestFailWithBadServerIndentity()
        {
            RunPowerShellTest("Test-FailWithBadServerIndentity");
        }
    }
}
