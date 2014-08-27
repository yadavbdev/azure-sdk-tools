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
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.Subscriptions;
using Microsoft.Azure.Subscriptions.Models;
using Microsoft.WindowsAzure.Commands.Common.Interfaces;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Convenience client for azure profile and subscriptions.
    /// </summary>
    public class ProfileClient
    {
        private readonly string AzureModeBoth = AzureModule.AzureServiceManagement + "," + AzureModule.AzureResourceManager;

        public static IDataStore DataStore { get; set; }

        public AzureProfile Profile { get; private set; }

        public Action<string> WarningLog;

        private static void UpgradeProfile()
        {
            string oldProfileFilePath = System.IO.Path.Combine(AzurePowerShell.ProfileDirectory, AzurePowerShell.OldProfileFile);
            string newProfileFilePath = System.IO.Path.Combine(AzurePowerShell.ProfileDirectory, AzurePowerShell.ProfileFile);
            if (DataStore.FileExists(oldProfileFilePath))
            {
                string oldProfilePath = System.IO.Path.Combine(AzurePowerShell.ProfileDirectory,
                    AzurePowerShell.OldProfileFile);
                AzureProfile oldProfile = new AzureProfile(DataStore, oldProfilePath);

                if (DataStore.FileExists(newProfileFilePath))
                {
                    // Merge profile files
                    AzureProfile newProfile = new AzureProfile(DataStore, newProfileFilePath);
                    foreach (var environment in newProfile.Environments.Values)
                    {
                        oldProfile.Environments[environment.Name] = environment;
                    }
                    foreach (var subscription in newProfile.Subscriptions.Values)
                    {
                        oldProfile.Subscriptions[subscription.Id] = subscription;
                    }
                    DataStore.DeleteFile(newProfileFilePath);
                }

                // Save the profile to the disk
                oldProfile.Save();
                
                // Rename WindowsAzureProfile.xml to WindowsAzureProfile.json
                DataStore.RenameFile(oldProfilePath, newProfileFilePath);
            }
        }

        static ProfileClient()
        {
            DataStore = new DiskDataStore();
        }

        public ProfileClient()
            : this(System.IO.Path.Combine(AzurePowerShell.ProfileDirectory, AzurePowerShell.ProfileFile))
        {

        }

        public ProfileClient(string profilePath)
        {
            ProfileClient.UpgradeProfile();

            Profile = new AzureProfile(DataStore, profilePath);

            WarningLog = (s) => Debug.WriteLine(s);
        }

        #region Account management

        public AzureAccount AddAccount(UserCredentials credentials, string environment)
        {
            if (string.IsNullOrEmpty(environment))
            {
                environment = AzureSession.CurrentEnvironment.Name;
            }

            if (!Profile.Environments.ContainsKey(environment))
            {
                throw new Exception(string.Format(Resources.EnvironmentNotFound, environment));
            }

            var subscriptions = RefreshSubscriptions(credentials, environment, null)
                .Where(s => s.GetPropertyAsArray(AzureSubscription.Property.AvailablePrincipalNames).Contains(credentials.UserName))
                .Select(s => s);

            if (Profile.DefaultSubscription == null)
            {
                Profile.DefaultSubscription = Profile.Subscriptions.Values.FirstOrDefault();
            }
            return new AzureAccount {UserName = credentials.UserName, Subscriptions = subscriptions.ToList() };
        }

        public IEnumerable<AzureAccount> ListAccounts(string userName, string environment)
        {
            List<AzureSubscription> subscriptions = Profile.Subscriptions.Values.ToList();
            if (environment != null)
            {
                subscriptions = subscriptions.Where(s => s.Environment == environment).ToList();
            }

            List<string> names = new List<string>();
            if (!string.IsNullOrEmpty(userName))
            {
                names.Add(userName);
            }
            else
            {
                names = subscriptions
                    .SelectMany(s => s.GetPropertyAsArray(AzureSubscription.Property.AvailablePrincipalNames))
                    .Distinct().ToList();
            }

            foreach (var name in names)
            {
                AzureAccount account = new AzureAccount();
                account.UserName = name;
                account.Subscriptions = subscriptions
                    .Where(s => s.GetPropertyAsArray(AzureSubscription.Property.AvailablePrincipalNames).Contains(name)).ToList();

                if (account.Subscriptions.Count == 0)
                {
                    continue;
                }

                yield return account;
            }
        }

        public AzureAccount RemoveAccount(string userName)
        {
            var userAccounts = ListAccounts(userName, null);

            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException("User name needs to be specified.", "userName");
            }

            if (!userAccounts.Any())
            {
                throw new ArgumentException("User name is not valid.", "userName");
            }

            var userAccount = userAccounts.First();

            foreach (var subscriptionFromAccount in userAccount.Subscriptions)
            {
                var subscription = Profile.Subscriptions[subscriptionFromAccount.Id];

                // Warn the user if the removed subscription is the default one.
                if (subscription.GetProperty(AzureSubscription.Property.Default) != null)
                {
                    if (WarningLog != null)
                    {
                        WarningLog(Resources.RemoveDefaultSubscription);
                    }
                }

                // Warn the user if the removed subscription is the current one.
                if (subscription.Equals(AzureSession.CurrentSubscription))
                {
                    if (WarningLog != null)
                    {
                        WarningLog(Resources.RemoveCurrentSubscription);
                    }
                }

                Profile.Subscriptions.Remove(subscription.Id);
            }

            return userAccount;
        }

        #endregion

        #region Subscripton management

        public void AddOrSetSubscriptions(IEnumerable<AzureSubscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                AddOrSetSubscription(subscription);
            }
        }

        public AzureSubscription AddOrSetSubscription(AzureSubscription subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("Subscription needs to be specified.", "subscription");
            }
            if (subscription.Environment == null)
            {
                throw new ArgumentNullException("Environment needs to be specified.", "subscription.Environment");
            }
            if (!Profile.Environments.ContainsKey(subscription.Environment))
            {
                throw new ArgumentException("Environment name is invalid.", "subscription.Environment");
            }

            Profile.Subscriptions[subscription.Id] = subscription;
            return subscription;
        }

        public AzureSubscription RemoveSubscription(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Subscription name needs to be specified.", "name");
            }

            var subscription = Profile.Subscriptions.Values.FirstOrDefault(s => s.Name == name);

            if (subscription == null)
            {
                throw new ArgumentException(string.Format(Resources.SubscriptionNameNotFoundMessage, name), "name");
            }
            else
            {
                return RemoveSubscription(subscription.Id);
            }
        }

        public AzureSubscription RemoveSubscription(Guid id)
        {
            if (!Profile.Subscriptions.ContainsKey(id))
            {
                throw new ArgumentException(Resources.SubscriptionIdNotFoundMessage, "name");
            }

            var subscription = Profile.Subscriptions[id];
            if (subscription.Properties.ContainsKey(AzureSubscription.Property.Default))
            {
                WarningLog(Resources.RemoveDefaultSubscription);
            }

            // Warn the user if the removed subscription is the current one.
            if (AzureSession.CurrentSubscription != null && subscription.Id == AzureSession.CurrentSubscription.Id)
            {
                WarningLog(Resources.RemoveCurrentSubscription);
                AzureSession.SetCurrentSubscription(null, null);
            }

            Profile.Subscriptions.Remove(id);

            return subscription;
        }

        public List<AzureSubscription> RefreshSubscriptions(UserCredentials credentials, string environment, string name)
        {
            IEnumerable<AzureSubscription> subscriptions = MergeSubscriptions(Profile.Subscriptions.Values.ToList(),
                LoadSubscriptionsFromServer(ref credentials).ToList());

            // Update back Profile.Subscriptions
            foreach (var subscription in subscriptions)
            {
                // Update Environment
                subscription.Environment = environment;

                // Update DefaultPrincipalName
                if (credentials.UserName != null)
                {
                    if (subscription.GetPropertyAsArray(AzureSubscription.Property.AvailablePrincipalNames)
                            .Contains(credentials.UserName))
                    {
                        subscription.Properties[AzureSubscription.Property.DefaultPrincipalName] = credentials.UserName;
                    }
                }
                else
                {
                    if (Profile.Subscriptions.ContainsKey(subscription.Id) && 
                        Profile.Subscriptions[subscription.Id].GetProperty(AzureSubscription.Property.DefaultPrincipalName) != null)
                    {
                        subscription.Properties[AzureSubscription.Property.DefaultPrincipalName] = 
                            Profile.Subscriptions[subscription.Id].GetProperty(AzureSubscription.Property.DefaultPrincipalName);
                    }
                }
                Profile.Subscriptions[subscription.Id] = subscription;
            }

            if (!string.IsNullOrEmpty(name))
            {
                subscriptions = subscriptions.Where(s => s.Name == name);
            }

            return subscriptions.ToList();
        }

        public AzureSubscription GetSubscriptionById(Guid id)
        {
            if (Profile.Subscriptions.ContainsKey(id))
            {
                return Profile.Subscriptions[id];
            }
            else
            {
                throw new ArgumentException(Resources.SubscriptionIdNotFoundMessage, "id");
            }
        }

        public AzureSubscription SetSubscriptionAsCurrent(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", string.Format(Resources.InvalidSubscription, name));
            }

            var subscription = Profile.Subscriptions.Values.FirstOrDefault(s => s.Name == name);

            if (subscription == null)
            {
                throw new ArgumentException(string.Format(Resources.InvalidSubscription, name), "name");
            }
            else
            {
                var environment = GetEnvironmentOrDefault(subscription.Environment);
                AzureSession.SetCurrentSubscription(subscription, environment);
            }

            return subscription;
        }

        public AzureSubscription SetSubscriptionAsDefault(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", string.Format(Resources.InvalidSubscription, name));
            }

            var subscription = Profile.Subscriptions.Values.FirstOrDefault(s => s.Name == name);

            if (subscription == null)
            {
                throw new ArgumentException(string.Format(Resources.InvalidSubscription, name), "name");
            }
            else
            {
                Profile.DefaultSubscription = subscription;
            }

            return subscription;
        }

        public void ClearDefaultSubscription()
        {
            Profile.DefaultSubscription = null;
        }

        public void ImportCertificate(X509Certificate2 certificate)
        {
            DataStore.AddCertificate(certificate);
        }

        public List<AzureSubscription> ImportPublishSettings(string filePath)
        {
            var subscriptions = LoadSubscriptionsFromPublishSettingsFile(filePath);
            foreach (var subscription in subscriptions)
            {
                subscription.Properties[AzureSubscription.Property.SupportedModes] = AzureModule.AzureServiceManagement.ToString();
            }
            AddOrSetSubscriptions(subscriptions);
            return subscriptions;
        }

        private List<AzureSubscription> LoadSubscriptionsFromPublishSettingsFile(string filePath)
        {
            var currentEnvironment = AzureSession.CurrentEnvironment;

            if (string.IsNullOrEmpty(filePath) || !DataStore.FileExists(filePath))
            {
                throw new ArgumentException("File path is not valid.", "filePath");
            }
            return PublishSettingsImporter.ImportAzureSubscription(DataStore.ReadFileAsStream(filePath), currentEnvironment.Name).ToList();
        }

        private IEnumerable<AzureSubscription> LoadSubscriptionsFromServer(ref UserCredentials credentials)
        {
            var currentMode = PowerShellUtilities.GetCurrentMode();
            var currentSubscription = AzureSession.CurrentSubscription;
            var currentEnvironment = AzureSession.CurrentEnvironment;
            if (currentSubscription == null)
            {
                return LoadSubscriptionsFromServer(currentEnvironment, currentMode, ref credentials);
            }
            else
            {
                // Get all AD accounts and iterate
                var userIds = Profile.Subscriptions.Values
                    .SelectMany(s => s.GetPropertyAsArray(AzureSubscription.Property.AvailablePrincipalNames))
                    .Distinct();

                List<AzureSubscription> subscriptions = new List<AzureSubscription>();
                foreach (var userId in userIds)
                {
                    credentials.UserName = userId;
                    subscriptions = subscriptions
                        .Union(LoadSubscriptionsFromServer(currentEnvironment, currentMode, ref credentials)).ToList();
                }

                if (subscriptions.Any())
                {
                    return subscriptions;
                }
                else
                {
                    return new AzureSubscription[0];
                }
            }
        }

        private IEnumerable<AzureSubscription> LoadSubscriptionsFromServer(AzureEnvironment environment, AzureModule currentMode,
            ref UserCredentials credentials)
        {
            IEnumerable<AzureSubscription> result;
            if (currentMode == AzureModule.AzureResourceManager)
            {
                result = MergeSubscriptions(GetServiceManagementSubscriptions(environment, ref credentials).ToList(),
                    GetResourceManagerSubscriptions(environment, ref credentials).ToList());
            }
            else
            {
                result = MergeSubscriptions(GetServiceManagementSubscriptions(environment, ref credentials).ToList(),
                    null);
            }

            // Set user ID
            foreach (var subscription in result)
            {
                subscription.Properties[AzureSubscription.Property.DefaultPrincipalName] = credentials.UserName;
                subscription.Properties[AzureSubscription.Property.AvailablePrincipalNames] = credentials.UserName;
            }

            return result;
        }

        private IEnumerable<AzureSubscription> MergeSubscriptions(IList<AzureSubscription> subscriptionsList1,
            IList<AzureSubscription> subscriptionsList2)
        {
            if (subscriptionsList1 == null)
            {
                subscriptionsList1 = new List<AzureSubscription>();
            }
            if (subscriptionsList2 == null)
            {
                subscriptionsList2 = new List<AzureSubscription>();
            }

            Dictionary<Guid, AzureSubscription> mergedSubscriptions = new Dictionary<Guid, AzureSubscription>();
            foreach (var subscription1 in subscriptionsList1)
            {
                mergedSubscriptions[subscription1.Id] = subscription1;
            }
            foreach (var subscription2 in subscriptionsList2)
            {
                if (mergedSubscriptions.ContainsKey(subscription2.Id))
                {
                    mergedSubscriptions[subscription2.Id] = MergeSubscriptionProperties(mergedSubscriptions[subscription2.Id], 
                        subscription2);
                }
                else
                {
                    mergedSubscriptions[subscription2.Id] = subscription2;
                }
            }
            return mergedSubscriptions.Values.ToList();
        }

        private AzureSubscription MergeSubscriptionProperties(AzureSubscription subscription1, AzureSubscription subscription2)
        {
            if (subscription1 == null || subscription2 == null)
            {
                throw new ArgumentNullException("subscription1");
            }
            if (subscription1.Id != subscription2.Id)
            {
                throw new ArgumentException("Subscription Ids do not match.");
            }
            AzureSubscription mergedSubscription = new AzureSubscription
            {
                Id = subscription1.Id,
                Name = subscription1.Name,
                Environment = subscription1.Environment
            };

            // Merge all properties
            foreach (AzureSubscription.Property property in Enum.GetValues(typeof(AzureSubscription.Property)))
            {
                string propertyValue = subscription1.GetProperty(property) ?? subscription2.GetProperty(property);
                if (propertyValue != null)
                {
                    mergedSubscription.Properties[property] = propertyValue;
                }
            }

            // Merge SupportedMode
            var supportedModes = subscription1.GetPropertyAsArray(AzureSubscription.Property.SupportedModes)
                    .Union(subscription2.GetPropertyAsArray(AzureSubscription.Property.SupportedModes));

            mergedSubscription.SetProperty(AzureSubscription.Property.SupportedModes, supportedModes.ToArray());

            // Merge AvailablePrincipalNames
            var principalNames = subscription1.GetPropertyAsArray(AzureSubscription.Property.AvailablePrincipalNames)
                    .Union(subscription2.GetPropertyAsArray(AzureSubscription.Property.AvailablePrincipalNames));

            mergedSubscription.SetProperty(AzureSubscription.Property.AvailablePrincipalNames, principalNames.ToArray());

            return mergedSubscription;
        }

        private IEnumerable<AzureSubscription> GetResourceManagerSubscriptions(AzureEnvironment environment, ref UserCredentials credentials)
        {
            IAccessToken commonTenantToken = AzureSession.AuthenticationFactory.Authenticate(environment, ref credentials);

            List<AzureSubscription> result = new List<AzureSubscription>();
            TenantListResult tenants;
            using (var subscriptionClient = AzureSession.ClientFactory.CreateClient<Azure.Subscriptions.SubscriptionClient>(
                new TokenCloudCredentials(commonTenantToken.AccessToken),
                environment.GetEndpointAsUri(AzureEnvironment.Endpoint.ResourceManagerEndpoint)))
            {
                tenants = subscriptionClient.Tenants.List();
            }

            // Go over each tenant and get all subscriptions for tenant
            foreach (var tenant in tenants.TenantIds)
            {
                // Generate tenant specific token to query list of subscriptions
                IAccessToken tenantToken = AzureSession.AuthenticationFactory.Authenticate(environment, tenant.TenantId, ref credentials);

                using (var subscriptionClient = AzureSession.ClientFactory.CreateClient<Azure.Subscriptions.SubscriptionClient>(
                        new TokenCloudCredentials(tenantToken.AccessToken),
                        environment.GetEndpointAsUri(AzureEnvironment.Endpoint.ResourceManagerEndpoint)))
                {
                    var subscriptionListResult = subscriptionClient.Subscriptions.List();
                    foreach (var subscription in subscriptionListResult.Subscriptions)
                    {
                        AzureSubscription psSubscription = new AzureSubscription
                        {
                            Id = new Guid(subscription.SubscriptionId),
                            Name = subscription.DisplayName,
                            Environment = environment.Name
                        };
                        psSubscription.Properties[AzureSubscription.Property.SupportedModes] = AzureModule.AzureResourceManager.ToString();
                        if (commonTenantToken.LoginType == LoginType.LiveId)
                        {
                            AzureSession.SubscriptionTokenCache[psSubscription.Id] = tenantToken;
                        }
                        else
                        {
                            AzureSession.SubscriptionTokenCache[psSubscription.Id] = commonTenantToken;
                        }

                        result.Add(psSubscription);
                    }
                }
            }

            return result;
        }

        private IEnumerable<AzureSubscription> GetServiceManagementSubscriptions(AzureEnvironment environment, ref UserCredentials credentials)
        {
            IAccessToken commonTenantToken = AzureSession.AuthenticationFactory.Authenticate(environment, ref credentials);

            List<AzureSubscription> result = new List<AzureSubscription>();
            using (var subscriptionClient = AzureSession.ClientFactory.CreateClient<WindowsAzure.Subscriptions.SubscriptionClient>(
                        new TokenCloudCredentials(commonTenantToken.AccessToken),
                        environment.GetEndpointAsUri(AzureEnvironment.Endpoint.ServiceEndpoint)))
            {
                var subscriptionListResult = subscriptionClient.Subscriptions.List();
                foreach (var subscription in subscriptionListResult.Subscriptions)
                {
                    AzureSubscription psSubscription = new AzureSubscription
                    {
                        Id = new Guid(subscription.SubscriptionId),
                        Name = subscription.SubscriptionName,
                        Environment = environment.Name
                    };
                    psSubscription.Properties[AzureSubscription.Property.SupportedModes] = AzureModule.AzureServiceManagement.ToString();
                    if (commonTenantToken.LoginType == LoginType.LiveId)
                    {
                        AzureSession.SubscriptionTokenCache[psSubscription.Id] = 
                            AzureSession.AuthenticationFactory.Authenticate(environment, 
                            subscription.ActiveDirectoryTenantId, ref credentials);
                    }
                    else
                    {
                        AzureSession.SubscriptionTokenCache[psSubscription.Id] = commonTenantToken;
                    }

                    result.Add(psSubscription);
                }
            }

            return result;
        }

        #endregion

        #region Environment management

        public AzureEnvironment AddEnvironment(AzureEnvironment environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("Environment needs to be specified.", "environment");
            }

            if (!Profile.Environments.ContainsKey(environment.Name))
            {
                Profile.Environments[environment.Name] = environment;
                return environment;
            }
            else
            {
                throw new ArgumentException(string.Format(Resources.EnvironmentExists, environment.Name), "environment");
            }
        }

        public AzureEnvironment GetEnvironmentOrDefault(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return AzureSession.CurrentEnvironment;
            }
            else if (AzureSession.CurrentEnvironment != null && AzureSession.CurrentEnvironment.Name == name)
            {
                return AzureSession.CurrentEnvironment;
            }
            else if (Profile.Environments.ContainsKey(name))
            {
                return Profile.Environments[name];
            }
            else
            {
                throw new ArgumentException(string.Format(Resources.EnvironmentNotFound, name));
            }
        }

        public List<AzureEnvironment> ListEnvironments(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Profile.Environments.Values.ToList();
            }
            else if (Profile.Environments.ContainsKey(name))
            {
                return new[] { Profile.Environments[name] }.ToList();
            }
            else
            {
                return new AzureEnvironment[0].ToList();
            }
        }

        public AzureEnvironment RemoveEnvironment(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Environment name needs to be specified.", "name");
            }
            
            if (Profile.Environments.ContainsKey(name))
            {
                var environment = Profile.Environments[name];
                Profile.Environments.Remove(name);
                return environment;
            }
            else
            {
                throw new ArgumentException(string.Format(Resources.EnvironmentNotFound, name), "name");
            }
        }

        public AzureEnvironment SetEnvironment(AzureEnvironment environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("Environment needs to be specified.", "environment");
            }

            if (Profile.Environments.ContainsKey(environment.Name))
            {
                Profile.Environments[environment.Name] = environment;
                return environment;
            }
            else
            {
                throw new ArgumentException(string.Format(Resources.EnvironmentNotFound, environment.Name), "environment");
            }
        }
        #endregion
    }
}