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
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.ExtensionTests
{
    [TestClass]
    public class AzureVMSqlServerExtensionTests:ServiceManagementTest
    {
        private string serviceName;
        private string vmName;
        private const string referenceNamePrefix = "Reference";
        private string version;
        private string referenceName;
        private const string extensionName = "SqlIaaSAgent";
        private const string DisabledState = "Disable";
        private const string EnableState = "Enable";

        [ClassInitialize]
        public static void Intialize(TestContext context)
        {
            imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);
        }

        [TestInitialize]
        public void TestIntialize()
        {
            pass = false;
            GetExtensionInfo();
            serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
            vmName = Utilities.GetUniqueShortName(vmNamePrefix);
            testStartTime = DateTime.Now;
        }

        

        [TestCleanup]
        public void TestCleanUp()
        {
            CleanupService(serviceName);
        }


        [TestMethod(), TestCategory(Category.Sequential), TestProperty("Feature", "IAAS"), Priority(0), Owner("seths"), Description("Test the cmdlet ((Get,Set,Remove)-AzureVMSqlServerExtension)")]
        public void GetAzureVMSqlServerExtensionTest()
        {
            try
            {
                referenceName = Utilities.GetUniqueShortName(referenceNamePrefix);

                //Deploy a new IaaS VM with Extension using Set-AzureVMSqlServerExtension
                Console.WriteLine("Deploying a new vm with Sql Server extension.");
                var vm = CreateIaaSVMObject(vmName);
                vm = vmPowershellCmdlets.SetAzureVMSqlServerExtension(vm, version, referenceName, false);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("Deployed a vm {0}with SQL Server extension.", vmName);

                
                var extension = GetSqlServer(vmName, serviceName);
                VerifyExtension(extension);

                //Disable the extension
                Console.WriteLine("Disable SQL Server extension and update VM.");
                vm = vmPowershellCmdlets.SetAzureVMSqlServerExtension(vm, version, referenceName, true);
                vmPowershellCmdlets.UpdateAzureVM(vmName, serviceName, vm);
                Console.WriteLine("Sql Server extension disabled");

                extension = GetSqlServer(vmName, serviceName);
                VerifyExtension(extension,true);

                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
                vmPowershellCmdlets.RemoveAzureVMSqlServerExtension(vmRoleContext.VM);

                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        [TestMethod(), TestCategory(Category.Sequential), TestProperty("Feature", "IAAS"), Priority(0), Owner("seths"), Description("Test the cmdlet ((Get,Set)-AzureVMSqlServerExtension)")]
        public void UpdateVMWithSqlServerExtensionTest()
        {
            try
            {
                referenceName = extensionName;
                Console.WriteLine("Deploying a new vm {0}",vmName);
                var vm = CreateIaaSVMObject(vmName);
                vm = vmPowershellCmdlets.SetAzureAvailabilitySet("test", vm);
                vm = vmPowershellCmdlets.RemoveAzureVMExtension(vm, null, null, null, true);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("Deployed vm {0}", vmName);
                GetSqlServer(vmName, serviceName, true);

                Console.WriteLine("Set SqlServer extension and update vm {0}." , vmName);
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);

                vm = vmPowershellCmdlets.SetAzureVMSqlServerExtension(vmRoleContext.VM, version, referenceName, false);
                vmPowershellCmdlets.UpdateAzureVM(vmName, serviceName, vm);
                Console.WriteLine("SqlServer extension set and updated vm {0}.", vmName);

                var extension = GetSqlServer(vmName, serviceName);
                VerifyExtension(extension);
                
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        [TestMethod(), TestCategory(Category.Sequential), TestProperty("Feature", "IAAS"), Priority(0), Owner("seths"), Description("Test the cmdlet ((Get,Set)-AzureVMSqlServerExtension)")]
        public void AddRoleWithSqlServerExtensionTest()
        {
            try
            {
                //Deploy a new IaaS VM with Extension using Add-AzureVMExtension
                Console.WriteLine("Deploying a new vm {0}", vmName);
                var vm1 = CreateIaaSVMObject(vmName);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm1 }, locationName);

                //Add a role with extension config
                referenceName = Utilities.GetUniqueShortName(referenceNamePrefix);
                string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);
                Console.WriteLine("Deploying a new vm {0} with SQL Server extension", vmName2);
                var vm2 = CreateIaaSVMObject(vmName2);
                vm2 = vmPowershellCmdlets.SetAzureVMSqlServerExtension(vm2, version, referenceName, false);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm2 });

                var extension = GetSqlServer(vmName2, serviceName);
                VerifyExtension(extension);

                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }


        [TestMethod(), TestCategory(Category.Sequential), TestProperty("Feature", "IAAS"), Priority(0), Owner("seths"), Description("Test the cmdlet ((Get,Set)-AzureVMSqlServerExtension)")]
        public void UpdateRoleWithSqlServerExtensionTest()
        {
            try
            {
                referenceName = extensionName;

                //Deploy a new IaaS VM 
                Console.WriteLine("Deploying a new vm {0}", vmName);
                var vm1 = CreateIaaSVMObject(vmName);

                //Add a role with extension config
                string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);
                Console.WriteLine("Deploying a new vm {0}", vmName2);
                var vm2 = CreateIaaSVMObject(vmName2);

                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm1,vm2 }, locationName);

                Console.WriteLine("Set SqlServer extension and update vm {0}.", vmName2);
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName2, serviceName);
                vm2 = vmPowershellCmdlets.SetAzureVMSqlServerExtension(vm2, version, extensionName, false);
                vmPowershellCmdlets.UpdateAzureVM(vmName2, serviceName, vm2);
                Console.WriteLine("SqlServer extension set and updated vm {0}.", vmName2);

                var extension = GetSqlServer(vmName2, serviceName);
                VerifyExtension(extension);

                vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName2,serviceName);
                vmPowershellCmdlets.RemoveAzureVMSqlServerExtension(vmRoleContext.VM);

                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        [TestMethod(), TestCategory(Category.Sequential),TestProperty("Feature","IAAS"),Priority(0),Owner("seths"),Description("Verifies that SqlServer extension is applied by default to Azure IaaS VM with GA enabled.")]
        public void SqlServerEnabledForNewAzureVMWithGATest()
        {
            try
            {
                StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
                //Create a new Iaas VM with GA enabled
                var vm = Utilities.CreateIaaSVMObject(vmName, InstanceSize.Small, imageName, true, username, password);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                
                var sqlServerExtension = vmPowershellCmdlets.GetAzureVMSqlServerExtension(Utilities.GetAzureVM(vmName, serviceName));
                VerifyExtension(sqlServerExtension);
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [TestMethod(), TestCategory(Category.Sequential), TestProperty("Feature", "IaaS"), Priority(0), Owner("seths"), Description("Verifies that SqlServer extension is not applied by default to Azure IaaS VM with GA disabled.")]
        public void SqlServerDisabledForNewAzureVMWithoutGATest()
        {
            try
            {
                StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
                var vm = Utilities.CreateIaaSVMObject(vmName, InstanceSize.Small, imageName, true, username, password,false);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Assert.IsNull(vmPowershellCmdlets.GetAzureVMSqlServerExtension(Utilities.GetAzureVM(vmName, serviceName),"SqlServer extension is applied to a VM with GA disabled."));
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private PersistentVM CreateIaaSVMObject(string vmName)
        {
            defaultAzureSubscription = vmPowershellCmdlets.SetAzureSubscription(defaultAzureSubscription.SubscriptionName, defaultAzureSubscription.SubscriptionId, CredentialHelper.DefaultStorageName);
            vmPowershellCmdlets.SelectAzureSubscription(defaultAzureSubscription.SubscriptionName, true);


            //Create an IaaS VM with a static CA.
            var azureVMConfigInfo = new AzureVMConfigInfo(vmName, InstanceSize.Small.ToString(), imageName);
            var azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, username, password);
            var persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
            return vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);
        }

        private VirtualMachineSqlServerExtensionContext GetSqlServer(string vmName, string serviceName, bool shouldNotExist = false)
        {
            Console.WriteLine("Get Sql Server extension info.");
            var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
            try
            {
                var extension = vmPowershellCmdlets.GetAzureVMSqlServerExtension(vmRoleContext.VM);
                Utilities.PrintCompleteContext(extension);
                Console.WriteLine("Fetched SqlServer extension info successfully.");
                return extension;
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("SqlServer Extension is not installed in the VM.");
                Console.WriteLine(e);
                if (!shouldNotExist)
                {
                    throw;
                }
                Console.WriteLine("This is expected.");
                return null;
            }
        }

        private void VerifyExtension(VirtualMachineSqlServerExtensionContext extension,bool disable=false)
        {
            Console.WriteLine("Verifying Sql Server extension info.");
            Assert.AreEqual(version, extension.Version);
            Assert.AreEqual(string.IsNullOrEmpty(referenceName) ? extensionName : referenceName, extension.ReferenceName);
            if (disable)
            {
                Assert.AreEqual(DisabledState, extension.State);
            }
            else
            {
                Assert.AreEqual(EnableState, extension.State);
            }
            Console.WriteLine("Sql Server extension verified successfully.");
        }


        private void GetExtensionInfo()
        {
            var extensionInfo = Utilities.GetAzureVMExtenionInfo(extensionName);
            if (extensionInfo != null)
            {
                version = extensionInfo.Version;
            }
        }
        
    }
}
