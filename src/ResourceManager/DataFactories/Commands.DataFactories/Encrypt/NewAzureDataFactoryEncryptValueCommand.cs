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
using System.Management.Automation;
using System.Security;
using System.Security.Permissions;

namespace Microsoft.Azure.Commands.DataFactories
{
    [Cmdlet(VerbsCommon.New, Constants.EncryptString), OutputType(typeof(string))]
    public class NewAzureDataFactoryEncryptValueCommand : DataFactoryBaseCmdlet
    {
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The data factory name.")]
        [ValidateNotNullOrEmpty]
        public string DataFactoryName { get; set; }

        [Parameter(Position = 2, Mandatory = true, HelpMessage = "The value to encrypt.")]
        [ValidateNotNullOrEmpty]
        public SecureString Value { get; set; }

        [Parameter(Position = 3, Mandatory = false, HelpMessage = "The gateway group name.")]
        public string GatewayName { get; set; }

        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public override void ExecuteCmdlet()
        {
            try
            {
                string encryptedValue = this.DataFactoryClient.EncryptString(this.Value, this.ResourceGroupName, this.DataFactoryName, this.GatewayName);

                this.WriteObject(encryptedValue);
            }
            catch (Exception ex)
            {
                this.WriteExceptionError(ex);
            }
        }
    }
}