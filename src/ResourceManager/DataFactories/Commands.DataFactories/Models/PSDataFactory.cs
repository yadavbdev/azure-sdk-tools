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
    public class PSDataFactory
    {
        private DataFactory _dataFactory;

        public PSDataFactory()
        {
            this._dataFactory = new DataFactory();
        }

        public PSDataFactory(DataFactory dataFactory)
        {
            if (dataFactory == null)
            {
                throw new ArgumentNullException("dataFactory");
            }

            this._dataFactory = dataFactory;
        }

        public string DataFactoryName
        {
            get
            {
                return this._dataFactory.Name;
            }
            set
            {
                this._dataFactory.Name = value;
            }
        }

        public string ResourceGroupName { get; set; }

        public string Location
        {
            get
            {
                return this._dataFactory.Location;
            }
            set
            {
                this._dataFactory.Location = value;
            }
        }

        public IDictionary<string, string> Tags
        {
            get
            {
                return this._dataFactory.Tags;
            }
            set
            {
                this._dataFactory.Tags = value;
            }
        }

/* ToDo: DataFactoryFactories will be introduced in the next Hydra spec update
        public DataFactoryProperties Properties
        {
            get
            {
                return this._dataFactory.Properties;
            }
            internal set
            {
                this._dataFactory.Properties = value;
            }
        }
*/
    }
}
