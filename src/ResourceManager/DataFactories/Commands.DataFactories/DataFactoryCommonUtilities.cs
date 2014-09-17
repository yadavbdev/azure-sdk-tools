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
using System.Linq;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.Azure.Commands.DataFactories
{
    public static class DataFactoryCommonUtilities
    {
        public static string ExtractNameFromJson(string jsonText)
        {
            Dictionary<string,object> jsonKeyValuePairs = JsonUtilities.DeserializeJson(jsonText);

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

        public static DateTime SpecifyDateTimeKind(this DateTime time)
        {
            if (time.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(time, DateTimeKind.Local);
            }

            return time;
        }

        public static DateTime? SpecifyDateTimeKind(this DateTime? time)
        {
            if (time != null && time.Value.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(time.Value, DateTimeKind.Local);
            }

            return time;
        }
    }
}
