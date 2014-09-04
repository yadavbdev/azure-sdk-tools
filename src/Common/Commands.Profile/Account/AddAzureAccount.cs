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

using System.Management.Automation;
using System.Security;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using Microsoft.WindowsAzure.Commands.Utilities.Profile;

namespace Microsoft.WindowsAzure.Commands.Profile
{
    /// <summary>
    /// Cmdlet to log into an environment and download the subscriptions
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureAccount", DefaultParameterSetName = "User")]
    [OutputType(typeof(AzureAccount))]
    public class AddAzureAccount : SubscriptionCmdletBase
    {
        [Parameter(ParameterSetName = "User", Mandatory = false, HelpMessage = "Environment containing the account to log into")]
        [Parameter(ParameterSetName = "ServicePrincipal", Mandatory = false, HelpMessage = "Environment containing the account to log into")]
        public string Environment { get; set; }

        [Parameter(ParameterSetName = "User", Mandatory = false, HelpMessage = "Optional credential")]
        [Parameter(ParameterSetName = "ServicePrincipal", Mandatory = true, HelpMessage = "Optional credential")]
        public PSCredential Credential { get; set; }

        [Parameter(ParameterSetName = "ServicePrincipal", Mandatory = true)]
        public SwitchParameter ServicePrincipal
        {
            get { return isServicePrincipal; }
            set { isServicePrincipal = value; }
        }

        private bool isServicePrincipal;

        [Parameter(ParameterSetName = "ServicePrincipal", Mandatory = true, HelpMessage = "Optional tenant name or ID")]
        public string Tenant { get; set; }
     
        public AddAzureAccount() : base(true)
        {
            Environment = EnvironmentName.AzureCloud;
        }

        public override void ExecuteCmdlet()
        {
            AzureAccount azureAccount = new AzureAccount
            {
                Type = AzureAccount.AccountType.User
            };
            SecureString password = null;
            if (Credential != null)
            {
                azureAccount.Id = Credential.UserName;
                password = Credential.Password;
            }
            azureAccount.Type = isServicePrincipal
                ? AzureAccount.AccountType.ServicePrincipal
                : AzureAccount.AccountType.User;

            if (!string.IsNullOrEmpty(Tenant))
            {
                azureAccount.SetProperty(AzureAccount.Property.Tenants, new[] {Tenant});
            }

            var account = ProfileClient.AddAccountAndLoadSubscriptions(azureAccount, ProfileClient.GetEnvironmentOrDefault(Environment), password);

            if (account != null)
            {
                WriteVerbose(string.Format(Resources.AddAccountAdded, azureAccount.Id));
                if (ProfileClient.Profile.DefaultSubscription != null)
                {
                    WriteVerbose(string.Format(Resources.AddAccountShowDefaultSubscription,
                        ProfileClient.Profile.DefaultSubscription.Name));
                }
                WriteVerbose(Resources.AddAccountViewSubscriptions);
                WriteVerbose(Resources.AddAccountChangeSubscription);
                WriteObject(base.ConstructPSObject(
                    "Microsoft.WindowsAzure.Commands.Profile.Models.CustomAzureAccount",
                    "Id", account.Id,
                    "Type", account.Type,
                    "Subscriptions", account.GetProperty(AzureAccount.Property.Subscriptions).Replace(",", "\r\n"),
                    "Tenants", account.GetProperty(AzureAccount.Property.Tenants)));
            } 
        }
    }
}
