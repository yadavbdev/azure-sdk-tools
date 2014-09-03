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
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.Commands.Common.Interfaces;
using Microsoft.WindowsAzure.Common.Internals;
using System.Diagnostics;

namespace Microsoft.WindowsAzure.Commands.Common.Models
{
    public sealed class AzureProfile
    {
        private IDataStore store;
        private string profilePath;
        private AzureSubscription defaultSubscription;
        private string tokenCacheFile = Path.Combine(AzurePowerShell.ProfileDirectory, AzurePowerShell.TokenCacheFile);

        public AzureProfile()
        {
            Environments = new Dictionary<string, AzureEnvironment>(StringComparer.InvariantCultureIgnoreCase);
            Subscriptions = new Dictionary<Guid, AzureSubscription>();
            Accounts = new Dictionary<string, AzureAccount>(StringComparer.InvariantCultureIgnoreCase);
        }

        public AzureProfile(IDataStore store, string profilePath)
        {
            this.store = store;
            this.profilePath = profilePath;

            Load();
            defaultSubscription = Subscriptions.FirstOrDefault(
                s => s.Value.Properties.ContainsKey(AzureSubscription.Property.Default)).Value;
        }

        private void Load()
        {
            Environments = new Dictionary<string, AzureEnvironment>(StringComparer.InvariantCultureIgnoreCase);
            Subscriptions = new Dictionary<Guid, AzureSubscription>();
            Accounts = new Dictionary<string, AzureAccount>(StringComparer.InvariantCultureIgnoreCase);

            if (!store.DirectoryExists(AzurePowerShell.ProfileDirectory))
            {
                store.CreateDirectory(AzurePowerShell.ProfileDirectory);
            }

            if (store.FileExists(profilePath))
            {
                string contents = store.ReadFileAsText(profilePath);

                IProfileSerializer serializer;
                if (ParserHelper.IsXml(contents))
                {
                    serializer = new XmlProfileSerializer();
                    serializer.Deserialize(contents, this);
                }
                else if (ParserHelper.IsJson(contents))
                {
                    serializer = new JsonProfileSerializer();
                    serializer.Deserialize(contents, this);
                }
            }

            // Adding predefined environments
            foreach (AzureEnvironment env in AzureEnvironment.PublicEnvironments.Values)
            {
                Environments[env.Name] = env;
            }
        }

        public void Save()
        {
            // Removing predefined environments
            foreach (string env in AzureEnvironment.PublicEnvironments.Keys)
            {
                Environments.Remove(env);
            }

            JsonProfileSerializer jsonSerializer = new JsonProfileSerializer();

            string contents = jsonSerializer.Serialize(this);
            string diskContents = string.Empty;
            if (store.FileExists(profilePath))
            {
                diskContents = store.ReadFileAsText(profilePath);
            }

            if (diskContents != contents)
            {
                store.WriteFile(profilePath, contents);
            }
        }

        public IDataStore DataStore { get { return store; } }

        public Dictionary<string, AzureEnvironment> Environments { get; set; }

        public Dictionary<Guid, AzureSubscription> Subscriptions { get; set; }

        public Dictionary<string, AzureAccount> Accounts { get; set; }

        public AzureSubscription DefaultSubscription
        {
            get { return defaultSubscription; }

            set
            {
                if (defaultSubscription != null)
                {
                    defaultSubscription.Properties.Remove(AzureSubscription.Property.Default);
                }

                if (value != null)
                {
                    Debug.Assert(value.Id != Guid.Empty);

                    value.Properties[AzureSubscription.Property.Default] = "True";
                    Subscriptions[value.Id] = value;
                    defaultSubscription = Subscriptions[value.Id];
                }
                else
                {
                    defaultSubscription = null;
                }
            }
        }

        public X509Certificate2 GetCertificate(string thumbprint)
        {
            return store.GetCertificate(thumbprint);
        }

        public void AddCertificate(X509Certificate2 cert)
        {
            store.AddCertificate(cert);
        }

        public void SaveTokenCache(byte[] data)
        {
            store.WriteFile(tokenCacheFile, data);
        }

        public byte[] LoadTokenCache()
        {
            return store.ReadFileAsBytes(tokenCacheFile);
        }
    }
}
