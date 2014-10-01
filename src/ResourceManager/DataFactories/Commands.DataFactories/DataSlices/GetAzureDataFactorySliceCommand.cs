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
    [Cmdlet(VerbsCommon.Get, Constants.DataSlice), OutputType(typeof(List<PSDataSlice>))]
    public class GetAzureDataFactorySliceCommand : DataSliceContextBaseCmdlet
    {
        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public override void ExecuteCmdlet()
        {
            var dataSlices = DataFactoryClient.ListDataSlices(
                ResourceGroupName, DataFactoryName, TableName, StartDateTime.SpecifyDateTimeKind(),
                EndDateTime.SpecifyDateTimeKind());

            WriteObject(dataSlices, true);
        }
    }
}