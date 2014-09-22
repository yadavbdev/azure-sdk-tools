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
using System.Net;
using Microsoft.Azure.Commands.DataFactories.Models;
using Microsoft.WindowsAzure;

namespace Microsoft.Azure.Commands.DataFactories
{
    [Cmdlet(VerbsCommon.New, Constants.Gateway), OutputType(typeof(PSDataFactoryGateway))]
    public class NewAzureDataFactoryGatewayCommand : DataFactoryBaseCmdlet
    {
        private const string GatewayExsited = "A gateway with the name {0} already exsits in the data factory {1}.";

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The data factory gateway name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The data factory name.")]
        [ValidateNotNullOrEmpty]
        public string DataFactoryName { get; set; }

        [Parameter(Position = 3, Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The geographic region to create the data factory.")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(Position = 4, Mandatory = false, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The description to update.")]
        public string Description { get; set; }

        public override void ExecuteCmdlet()
        {
            try
            {
                PSDataFactoryGateway gateway = null;
                try
                {
                    gateway = DataFactoryClient.GetGateway(ResourceGroupName, DataFactoryName, Name);
                }
                catch (CloudException ex)
                {
                    if (ex.Response.StatusCode != HttpStatusCode.NotFound) throw;
                }

                if (gateway != null)
                {
                    throw new PSInvalidOperationException(string.Format(GatewayExsited, Name, DataFactoryName));
                }

                var request = new PSDataFactoryGateway
                {
                    Name = Name,
                    Location = NormalizeLocation(Location),
                    Description = Description
                };

                PSDataFactoryGateway response = DataFactoryClient.CreateOrUpdateGateway(ResourceGroupName, DataFactoryName, request);
                WriteObject(response);
            }
            catch (Exception ex)
            {
                WriteExceptionError(ex);
            }
        }

        // As a nested resource of data factory, CSM will not normalize location when
        // creating gateway, so we have to do this by ourselves.
        private static string NormalizeLocation(string location)
        {
            return String.IsNullOrEmpty(location)
                       ? String.Empty
                       : location.Trim().Replace(" ", String.Empty).ToUpperInvariant();
        }
    }
}
