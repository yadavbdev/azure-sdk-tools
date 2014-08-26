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
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Profile;

namespace Microsoft.WindowsAzure.Commands.Profile
{


    /// <summary>
    /// Sets an azure subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureSubscription", DefaultParameterSetName = "CommonSettings"), OutputType(typeof(AzureSubscription))]
    public class SetAzureSubscriptionCommand : SubscriptionCmdletBase
    {
        public SetAzureSubscriptionCommand() : base(true)
        {
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the subscription.", ParameterSetName = "CommonSettings")]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the subscription.", ParameterSetName = "ResetCurrentStorageAccount")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Account subscription ID.", ParameterSetName = "CommonSettings")]
        public string SubscriptionId { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Account certificate.", ParameterSetName = "CommonSettings")]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Service endpoint.", ParameterSetName = "CommonSettings")]
        public string ServiceEndpoint { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Cloud service endpoint.", ParameterSetName = "CommonSettings")]
        public string ResourceManagerEndpoint { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Current storage account name.", ParameterSetName = "CommonSettings")]
        [ValidateNotNullOrEmpty]
        public string CurrentStorageAccountName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Environment name.", ParameterSetName = "CommonSettings")]
        [ValidateNotNullOrEmpty]
        public string Environment { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Executes the set subscription cmdlet operation.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            var subscription = new AzureSubscription
            {
                Name = SubscriptionName,
                Id = new Guid(SubscriptionId)
            };

            if (CurrentStorageAccountName != null)
            {
                subscription.Properties[AzureSubscription.Property.CloudStorageAccount] = CurrentStorageAccountName;
            }
            if (Certificate != null)
            {
                ProfileClient.ImportCertificate(Certificate);
                subscription.Properties[AzureSubscription.Property.Thumbprint] = Certificate.Thumbprint;
            }

            AzureEnvironment environment = ProfileClient.GetEnvironmentOrDefault(Environment);

            if (ServiceEndpoint != null || ResourceManagerEndpoint != null)
            {
                if (Environment == null)
                {
                    WriteWarning(
                        "Please use Environment parameter to specify subscription environment. This warning will be converted into an error in the upcoming release.");
                }
                else
                {
                    environment = ProfileClient.Profile.Environments.Values
                        .FirstOrDefault(e => e.GetEndpoint(AzureEnvironment.Endpoint.ServiceEndpoint) == ServiceEndpoint 
                            && e.GetEndpoint(AzureEnvironment.Endpoint.ResourceManagerEndpoint) == ResourceManagerEndpoint);

                    if (environment == null)
                    {
                        throw new Exception("ServiceEndpoint and ResourceManagerEndpoint values do not match existing environment. Please use Environment parameter.");
                    }
                }
            }
           
            subscription.Environment = environment.Name;

            WriteObject(ProfileClient.AddOrSetSubscription(subscription));
        }
    }
}