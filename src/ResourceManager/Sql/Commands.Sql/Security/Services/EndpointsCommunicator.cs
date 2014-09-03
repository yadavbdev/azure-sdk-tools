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

using Microsoft.Azure.Commands.Resources.Models;
using Microsoft.Azure.Commands.Sql.Security.Model;
using Microsoft.Azure.Commands.Sql.Services;
using Microsoft.Azure.Management.Sql;
using Microsoft.Azure.Management.Sql.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Storage;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Storage;

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Sql.Security.Services
{
    /// <summary>
    /// This class is responsible for all the REST communication with the management libraries
    /// </summary>
    public class EndpointsCommunicator
    {
        private static SqlManagementClient SqlClient { get; set; }
        
        private static StorageManagementClient StorageClient { get; set; }
        
        private static AzureSubscription Subscription {get ; set; }

        private static ResourcesClient ResourcesClient { get; set; }
 
        public EndpointsCommunicator(AzureSubscription subscription)
        {
            if (subscription != Subscription)
            {
                Subscription = subscription;
                SqlClient = null;
                StorageClient = null;
                ResourcesClient = null;
            }
        }

        /// <summary>
        /// Gets the database security policy for the given database in the given database server in the given resource group
        /// </summary>
        public DatabaseSecurityPolicy GetDatabaseSecurityPolicy(string resourceGroupName, string serverName, string databaseName, string clientRequestId)
        {
            DatabaseSecurityPolicyGetResponse response = GetCurrentSqlClient(clientRequestId).DatabaseSecurity.Get(resourceGroupName, serverName, databaseName);
            return response.DatabaseSecurityPolicy;
        }

        /// <summary>
        /// Gets the database server security policy of the given database server in the given resource group
        /// </summary>
        public DatabaseSecurityPolicy GetServerSecurityPolicy(string resourceGroupName, string serverName, string clientRequestId)
        {
            DatabaseSecurityPolicyGetResponse response = GetCurrentSqlClient(clientRequestId).DatabaseSecurity.Get(resourceGroupName, serverName, Constants.ServerPolicyId);
            return response.DatabaseSecurityPolicy;
        }

        /// <summary>
        /// Sets the database security policy for the given database in the given database server in the given resource group
        /// </summary>
        public void SetDatabaseSecurityPolicy(string resourceGroupName, string serverName, string databaseName, string clientRequestId, DatabaseSecurityPolicyUpdateParameters parameters)
        {
            GetCurrentSqlClient(clientRequestId).DatabaseSecurity.Update(resourceGroupName, serverName, databaseName, parameters);
        }

        /// <summary>
        /// Sets the database server security policy of the given database server in the given resource group
        /// </summary>
        public void SetServerSecurityPolicy(string resourceGroupName, string serverName,  string clientRequestId, DatabaseSecurityPolicyUpdateParameters parameters)
        {
            GetCurrentSqlClient(clientRequestId).DatabaseSecurity.Update(resourceGroupName, serverName, Constants.ServerPolicyId, parameters);
        }
        
        /// <summary>
        /// Gets primary storage keys for the given storage account
        /// </summary>
        public string GetPrimaryStorageKeys(string storageAccountName)
        {
            
            try
            {
                return GetCurrentStorageClient().StorageAccounts.GetKeys(storageAccountName).PrimaryKey;
            }
            catch
            {
                throw new Exception(string.Format(Microsoft.Azure.Commands.Sql.Properties.Resources.StorageAccountNotFound));
            }
        }

        public string GetStorageResourceGroup(string storageAccountName)
        {
            ResourcesClient resourcesClient = GetCurrentResourcesClient();
            List<PSResource> allResources = resourcesClient.FilterPSResources(new BasePSResourceParameters());
            
            if (allResources.Count != 0)
            {
                PSResource account = allResources.Find(r => r.Name == storageAccountName && r.ResourceType == "Microsoft.ClassicStorage/storageAccounts");
                if (account != null)
                {
                    return account.ResourceGroupName;
                }     
                else
                {
                    throw new Exception(string.Format(Microsoft.Azure.Commands.Sql.Properties.Resources.StorageAccountNotFound));
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the storage table endpoint the given storage account
        /// </summary>
        public string GetStorageTableEndpoint(string storageAccountName)
        {
            try
            {
                List<Uri> endpoints = new List<Uri>(GetCurrentStorageClient().StorageAccounts.Get(storageAccountName).StorageAccount.Properties.Endpoints);
                return endpoints.Find(u => u.AbsoluteUri.Contains(".table.")).AbsoluteUri;
            }
            catch
            {
                throw new Exception(string.Format(Microsoft.Azure.Commands.Sql.Properties.Resources.StorageAccountNotFound));
            }
        }

        private StorageManagementClient GetCurrentStorageClient()
        {
            if(StorageClient == null)
                StorageClient = AzureSession.ClientFactory.CreateClient<StorageManagementClient>(Subscription, AzureEnvironment.Endpoint.ServiceManagement);
            return StorageClient;
        }

        private ResourcesClient GetCurrentResourcesClient()
        {
            if (ResourcesClient == null)
                ResourcesClient = new ResourcesClient(AzureSession.CurrentContext);
            return ResourcesClient;
        }

        /// <summary>
        /// Retrieve the SQL Management client for the currently selected subscription, adding the session and request
        /// id tracing headers for the current cmdlet invocation.
        /// </summary>
        /// <returns>The SQL Management client for the currently selected subscription.</returns>
        private SqlManagementClient GetCurrentSqlClient(String clientRequestId)
        {
            // Get the SQL management client for the current subscription
            if (SqlClient == null)
            {
                SqlClient = AzureSession.ClientFactory.CreateClient<SqlManagementClient>(Subscription, AzureEnvironment.Endpoint.ResourceManager);
                SqlClient.HttpClient.DefaultRequestHeaders.Add(Constants.ClientSessionIdHeaderName, Util.GenerateTracingId());
            }
            SqlClient.HttpClient.DefaultRequestHeaders.Remove(Constants.ClientRequestIdHeaderName);
            SqlClient.HttpClient.DefaultRequestHeaders.Add(Constants.ClientRequestIdHeaderName, clientRequestId);
            return SqlClient;
        }
    }
}
