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
using System.Globalization;
using System.Net;
using Microsoft.Azure.Commands.DataFactories.Models;
using Microsoft.Azure.Commands.DataFactories.Properties;
using Microsoft.Azure.Management.DataFactories;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.WindowsAzure;

namespace Microsoft.Azure.Commands.DataFactories
{
    public partial class DataFactoryClient
    {
        public List<PSDataSliceRun> ListDataSliceRuns(
            string resourceGroupName,
            string dataFactoryName,
            string tableName,
            DateTime dataSliceRangeStartTime)
        {
            List<PSDataSliceRun> runs = new List<PSDataSliceRun>();
            var response = DataPipelineManagementClient.DataSliceRuns.List(
                resourceGroupName,
                dataFactoryName,
                tableName,
                dataSliceRangeStartTime.ConvertToISO8601DateTimeString());

            if (response != null && response.DataSliceRuns != null)
            {
                foreach (var run in response.DataSliceRuns)
                {
                    runs.Add(
                        new PSDataSliceRun(run)
                        {
                            ResourceGroupName = resourceGroupName,
                            DataFactoryName = dataFactoryName,
                            TableName = tableName
                        });
                }
            }

            return runs;
        }

        public List<PSDataSlice> ListDataSlices(string resourceGroupName, string dataFactoryName, string tableName, DateTime dataSliceRangeStartTime, DateTime dataSliceRangeEndTime)
        {
            List<PSDataSlice> dataSlices = new List<PSDataSlice>();
            var response = DataPipelineManagementClient.DataSlices.List(
                resourceGroupName,
                dataFactoryName,
                tableName,
                dataSliceRangeStartTime.ConvertToISO8601DateTimeString(),
                dataSliceRangeEndTime.ConvertToISO8601DateTimeString());

            if (response != null && response.DataSlices != null)
            {
                foreach (var dataSlice in response.DataSlices)
                {
                    dataSlices.Add(
                        new PSDataSlice(dataSlice)
                        {
                            ResourceGroupName = resourceGroupName,
                            DataFactoryName = dataFactoryName,
                            TableName = tableName
                        });
                }
            }

            return dataSlices;
        }

        public void SetSliceStatus(
            string resourceGroupName,
            string dataFactoryName,
            string tableName,
            string sliceStatus,
            string updateType,
            DateTime dataSliceRangeStartTime,
            DateTime dataSliceRangeEndTime)
        {
            DataPipelineManagementClient.DataSlices.SetStatus(
                resourceGroupName,
                dataFactoryName,
                tableName,
                new DataSliceSetStatusParameters()
                {
                    SliceStatus = sliceStatus,
                    UpdateType = updateType,
                    DataSliceRangeStartTime = dataSliceRangeStartTime.ConvertToISO8601DateTimeString(),
                    DataSliceRangeEndTime = dataSliceRangeEndTime.ConvertToISO8601DateTimeString(),
                });
        }

        public PSRunLogInfo GetDataSliceRunLogsSharedAccessSignature(string resourceGroupName, string dataFactoryName, string dataSliceRunId)
        {
            var response = DataPipelineManagementClient.DataSliceRuns.GetLogs(
                resourceGroupName, dataFactoryName, dataSliceRunId);

            return new PSRunLogInfo(response.DataSliceRunLogsSASUri);
        }
    }
}