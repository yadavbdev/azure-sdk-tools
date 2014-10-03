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
using Microsoft.Azure.Management.DataFactories.Models;

namespace Microsoft.Azure.Commands.DataFactories.Models
{
    /// <summary>
    /// A PowerShell wrapper class on top of the DataSlice type.
    /// </summary>
    public class PSDataSliceRun
    {
        private DataSliceRun _dataSliceRun;

        public PSDataSliceRun()
        {
            this._dataSliceRun = new DataSliceRun();
        }

        public PSDataSliceRun(DataSliceRun dataSliceRun)
        {
            if (dataSliceRun == null)
            {
                throw new ArgumentNullException("dataSliceRun");
            }

            this._dataSliceRun = dataSliceRun;
        }

        public string Id
        {
            get
            {
                return this._dataSliceRun.Id;
            }
            internal set
            {
                this._dataSliceRun.Id = value;
            }
        }

        public string ResourceGroupName { get; set; }

        public string DataFactoryName { get; set; }

        public string TableName { get; set; }

        public string ResumptionToken
        {
            get
            {
                return this._dataSliceRun.ResumptionToken;
            }
            internal set
            {
                this._dataSliceRun.ResumptionToken = value;
            }
        }

        public string ContinuationToken
        {
            get
            {
                return this._dataSliceRun.ContinuationToken;
            }
            internal set
            {
                this._dataSliceRun.ContinuationToken = value;
            }
        }

        public DateTime ProcessingStartTime
        {
            get
            {
                return this._dataSliceRun.ProcessingStartTime;
            }
            internal set
            {
                this._dataSliceRun.ProcessingStartTime = value;
            }
        }

        public DateTime ProcessingEndTime
        {
            get
            {
                return this._dataSliceRun.ProcessingEndTime;
            }
            internal set
            {
                this._dataSliceRun.ProcessingEndTime = value;
            }
        }

        public int PercentComplete
        {
            get
            {
                return this._dataSliceRun.PercentComplete;
            }
            internal set
            {
                this._dataSliceRun.PercentComplete = value;
            }
        }

        public DateTime DataSliceStart
        {
            get
            {
                return this._dataSliceRun.DataSliceStart;
            }
            internal set
            {
                this._dataSliceRun.DataSliceStart = value;
            }
        }

        public DateTime DataSliceEnd
        {
            get
            {
                return this._dataSliceRun.DataSliceEnd;
            }
            internal set
            {
                this._dataSliceRun.DataSliceEnd = value;
            }
        }

        public string Status
        {
            get
            {
                return this._dataSliceRun.Status;
            }
            internal set
            {
                this._dataSliceRun.Status = value;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return this._dataSliceRun.Timestamp;
            }
            internal set
            {
                this._dataSliceRun.Timestamp = value;
            }
        }

        public int RetryAttempt
        {
            get
            {
                return this._dataSliceRun.RetryAttempt;
            }
            internal set
            {
                this._dataSliceRun.RetryAttempt = value;
            }
        }

        public IDictionary<string, string> Properties
        {
            get
            {
                return this._dataSliceRun.Properties;
            }
            internal set
            {
                this._dataSliceRun.Properties = value;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return this._dataSliceRun.ErrorMessage;
            }
            internal set
            {
                this._dataSliceRun.ErrorMessage = value;
            }
        }

        public string ActivityId
        {
            get
            {
                return this._dataSliceRun.ActivityId;
            }
            internal set
            {
                this._dataSliceRun.ActivityId = value;
            }
        }

        public string TableId
        {
            get
            {
                return this._dataSliceRun.TableId;
            }
            internal set
            {
                this._dataSliceRun.TableId = value;
            }
        }

        public string PipelineId
        {
            get
            {
                return this._dataSliceRun.PipelineId;
            }
            internal set
            {
                this._dataSliceRun.PipelineId = value;
            }
        }

    }
}
