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
using Microsoft.Azure.Commands.DataFactories.Properties;
using Microsoft.Azure.Management.DataFactories;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.Azure.Commands.DataFactories
{
    public class DataFactoryClient
    {
        public IDataPipelineManagementClient DataPipelineManagementClient { get; private set; }

        public DataFactoryClient(WindowsAzureSubscription subscription)
        {
            DataPipelineManagementClient =
                subscription.CreateClientFromResourceManagerEndpoint<DataPipelineManagementClient>();
        }

        /// <summary>
        /// Parameterless constructor for Mocking.
        /// </summary>
        public DataFactoryClient()
        {
        }
        
        public virtual PSDataFactory GetDataFactory(string resourceGroupName, string dataFactoryName)
        {
            var response = DataPipelineManagementClient.DataFactories.Get(resourceGroupName, dataFactoryName);

            return new PSDataFactory(response.DataFactory) { ResourceGroupName = resourceGroupName };
        }

        public virtual List<PSDataFactory> ListDataFactories(string resourceGroupName)
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

        public virtual List<PSDataFactory> FilterPSDataFactories(DataFactoryFilterOptions filterOptions)
        {
            if (filterOptions == null)
            {
                throw new ArgumentNullException("filterOptions");
            }
            
            // ToDo: make ResourceGroupName optional
            if (string.IsNullOrWhiteSpace(filterOptions.ResourceGroupName))
            {
                throw new ArgumentException(Resources.ResourceGroupNameCannotBeEmpty);
            }

            List<PSDataFactory> dataFactories = new List<PSDataFactory>();

            if (!string.IsNullOrWhiteSpace(filterOptions.Name))
            {
                dataFactories.Add(GetDataFactory(filterOptions.ResourceGroupName, filterOptions.Name));
            }
            else
            {
                // ToDo: Filter list results by Tag
                dataFactories.AddRange(ListDataFactories(filterOptions.ResourceGroupName));
            }

            return dataFactories;
        }
    }
}
