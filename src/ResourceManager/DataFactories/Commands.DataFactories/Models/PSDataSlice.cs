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
using Microsoft.Azure.Management.DataFactories.Models;

namespace Microsoft.Azure.Commands.DataFactories.Models
{
    /// <summary>
    /// A PowerShell wrapper class on top of the DataSlice type.
    /// </summary>
    public class PSDataSlice
    {
        private DataSlice _dataSlice;

        public PSDataSlice()
        {
            this._dataSlice = new DataSlice();
        }

        public PSDataSlice(DataSlice dataSlice)
        {
            if (dataSlice == null)
            {
                throw new ArgumentNullException("dataSlice");
            }

            this._dataSlice = dataSlice;
        }

        public string ResourceGroupName { get; set; }

        public string DataFactoryName { get; set; }

        public string TableName { get; set; }

        public DateTime Start
        {
            get
            {
                return this._dataSlice.Start;
            }
            internal set
            {
                this._dataSlice.Start = value;
            }
        }

        public DateTime End
        {
            get
            {
                return this._dataSlice.End;
            }
            internal set
            {
                this._dataSlice.End = value;
            }
        }

        public int RetryCount
        {
            get
            {
                return this._dataSlice.RetryCount;
            }
            internal set
            {
                this._dataSlice.RetryCount = value;
            }
        }

        public string Status
        {
            get
            {
                return this._dataSlice.Status;
            }
            internal set
            {
                this._dataSlice.Status = value;
            }
        }

        public string LatencyStatus
        {
            get
            {
                return this._dataSlice.LatencyStatus;
            }
            internal set
            {
                this._dataSlice.LatencyStatus = value;
            }
        }

        public int LongRetryCount
        {
            get
            {
                return this._dataSlice.LongRetryCount;
            }
            internal set
            {
                this._dataSlice.LongRetryCount = value;
            }
        }
    }
}
