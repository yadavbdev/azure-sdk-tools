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
using Microsoft.Azure.Commands.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.Azure.Commands.DataFactories
{
    public class DataFactoryClient : IDataFactoryClient
    {
        public IDataPipelineManagementClient DataPipelineManagementClient { get; internal set; }

        public Action<string> Logger { get; set; }

        public DataFactoryClient(WindowsAzureSubscription subscription, Action<string> logger)
        {
            Logger = logger;
            DataPipelineManagementClient =
                subscription.CreateClientFromResourceManagerEndpoint<DataPipelineManagementClient>();
        }

        public PSDataFactory GetDataFactory(string resourceGroupName, string dataFactoryName)
        {
            var response = DataPipelineManagementClient.DataFactories.Get(resourceGroupName, dataFactoryName);

            return new PSDataFactory(response.DataFactory) { ResourceGroupName = resourceGroupName };
        }

        public List<PSDataFactory> ListDataFactories(string resourceGroupName)
        {
            List<PSDataFactory> dataFactories = new List<PSDataFactory>();

            var response = DataPipelineManagementClient.DataFactories.List(resourceGroupName);

            if (response != null && response.DataFactories != null)
            {
                response.DataFactories.ForEach(
                    df => dataFactories.Add(new PSDataFactory(df) { ResourceGroupName = resourceGroupName }));
            }

            return dataFactories;
        }
    }
}
