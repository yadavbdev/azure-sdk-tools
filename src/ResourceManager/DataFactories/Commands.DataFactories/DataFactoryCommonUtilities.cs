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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Azure.Commands.DataFactories.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Newtonsoft.Json;

namespace Microsoft.Azure.Commands.DataFactories
{
    public static class DataFactoryCommonUtilities
    {
        public static string ExtractNameFromJson(string jsonText, string resourceType)
        {
            Dictionary<string, object> jsonKeyValuePairs;

            try
            {
                jsonKeyValuePairs = JsonUtilities.DeserializeJson(jsonText, true);
            }
            catch (Exception exception)
            {
                throw new JsonSerializationException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidJson, exception.Message, resourceType));
            }

            const string NameTagInJson = "Name";

            foreach (var key in jsonKeyValuePairs.Keys)
            {
                if (string.Compare(NameTagInJson, key, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return (string)jsonKeyValuePairs[key];
                }
            }

            return string.Empty;
        }

        public static Dictionary<string, string> ToDictionary(this Hashtable hashTable)
        {
            return hashTable.Cast<DictionaryEntry>().ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString());
        }
    }
}
