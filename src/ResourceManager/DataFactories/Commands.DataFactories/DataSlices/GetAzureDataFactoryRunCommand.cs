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
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.Permissions;
using Microsoft.Azure.Commands.DataFactories.Models;

namespace Microsoft.Azure.Commands.DataFactories
{
    [Cmdlet(VerbsCommon.Get, Constants.Run, DefaultParameterSetName = ByTableName), OutputType(typeof(List<PSDataSliceRun>))]
    public class GetAzureDataFactoryRunCommand : DataFactoryBaseCmdlet
    {
        private const string ByTableName = "ByTableName";
        private const string ByPipelineName = "ByPipelineName";

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The data factory name.")]
        [ValidateNotNullOrEmpty]
        public string DataFactoryName { get; set; }

        [Parameter(ParameterSetName = ByTableName, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The table name.")]
        [ValidateNotNullOrEmpty]
        public string TableName { get; set; }

        [Parameter(ParameterSetName = ByPipelineName, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The pipeline name.")]
        [ValidateNotNullOrEmpty]
        public string PipelineName { get; set; }

        [Parameter(ParameterSetName = ByPipelineName, Position = 3, Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The activity name.")]
        [ValidateNotNullOrEmpty]
        public string ActivityName { get; set; }

        [Parameter(ParameterSetName = ByTableName, Position = 3, Mandatory = true, HelpMessage = "The start time of the data slice queried.")]
        [Parameter(ParameterSetName = ByPipelineName, Position = 4, HelpMessage = "The start time of the data slice queried.")]
        public DateTime StartDateTime { get; set; }

        [Parameter(ParameterSetName = ByPipelineName, Position = 5, Mandatory = false, HelpMessage = "The end time of the data slice queried.")]
        public DateTime? EndDateTime { get; set; }

        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public override void ExecuteCmdlet()
        {
            switch (ParameterSetName)
            {
                case ByTableName:
                    var dataSliceRuns = DataFactoryClient.ListDataSliceRuns(
                        ResourceGroupName, DataFactoryName, TableName, StartDateTime);
                    WriteObject(dataSliceRuns);
                    break;

                case ByPipelineName:
                    var pipelineRuns = DataFactoryClient.GetPipelineRuns(
                        ResourceGroupName, DataFactoryName, PipelineName, ActivityName,
                        StartDateTime, EndDateTime, null);
                    WriteObject(pipelineRuns, true);
                    break;
            }
        }
    }
}