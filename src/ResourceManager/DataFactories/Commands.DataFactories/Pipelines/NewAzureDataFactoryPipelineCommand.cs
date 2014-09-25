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
using System.Globalization;
using System.Management.Automation;
using System.Security.Permissions;
using Microsoft.Azure.Commands.DataFactories.Models;
using Microsoft.WindowsAzure;
using System.Net;

namespace Microsoft.Azure.Commands.DataFactories
{
    [Cmdlet(VerbsCommon.New, Constants.Pipeline), OutputType(typeof(PSPipeline))]

    public class NewAzureDataFactoryPipelineCommand : DataFactoryBaseCmdlet
    {
        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The pipeline name.")]
        public string Name { get; set; }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The data factory name.")]
        [ValidateNotNullOrEmpty]
        public string DataFactoryName { get; set; }

        [Parameter(Position = 3, Mandatory = true, HelpMessage = "The pipeline JSON file path.")]
        [ValidateNotNullOrEmpty]
        public string File { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Don't ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public override void ExecuteCmdlet()
        {
                string rawJsonContent = DataFactoryClient.ReadJsonFileContent(GetUnresolvedProviderPathFromPSPath(File));

                Name = ResolveResourceName(rawJsonContent, Name, "Pipeline");

                CreatePSPipelineParameters parameters = new CreatePSPipelineParameters()
                {
                    ResourceGroupName = ResourceGroupName,
                    DataFactoryName = DataFactoryName,
                    Name = Name,
                    RawJsonContent = rawJsonContent,
                    Force = Force.IsPresent,
                    ConfirmAction = ConfirmAction
                };

                WriteObject(DataFactoryClient.CreatePSPipeline(parameters));
        }
    }
}