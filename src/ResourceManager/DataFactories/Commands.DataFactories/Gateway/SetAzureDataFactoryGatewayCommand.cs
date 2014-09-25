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
using System.Security.Permissions;
using Microsoft.Azure.Commands.DataFactories.Models;

namespace Microsoft.Azure.Commands.DataFactories
{
    [Cmdlet(VerbsCommon.Set, Constants.Gateway), OutputType(typeof(PSDataFactoryGateway))]
    public class SetAzureDataFactoryGatewayCommand : DataFactoryBaseCmdlet
    {
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The data factory gateway name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The data factory name.")]
        [ValidateNotNullOrEmpty]
        public string DataFactoryName { get; set; }

        [Parameter(Position = 3, Mandatory = true, HelpMessage = "The description to update.")]
        [ValidateNotNullOrEmpty]
        public string Description { get; set; }

        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public override void ExecuteCmdlet()
        {
            try
            {
                PSDataFactoryGateway gateway = DataFactoryClient.GetGateway(ResourceGroupName, DataFactoryName, Name);
                gateway.Description = Description;
                PSDataFactoryGateway response = DataFactoryClient.CreateOrUpdateGateway(ResourceGroupName, DataFactoryName, gateway);
                WriteObject(response);
            }
            catch (Exception ex)
            {
                WriteExceptionError(ex);
            }
        }
    }
}
