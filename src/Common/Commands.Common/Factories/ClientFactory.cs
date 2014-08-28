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
using System.Net;
using System.Net.Http;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Common;
using System.Diagnostics;

namespace Microsoft.WindowsAzure.Commands.Common.Factories
{
    public class ClientFactory : IClientFactory
    {
        private static readonly char[] uriPathSeparator = { '/' };

        public event EventHandler<ClientCreatedArgs> OnClientCreated;

        public TClient CreateClient<TClient>(AzureSubscription subscription, Uri endpoint, AzureProfile profile) where TClient : ServiceClient<TClient>
        {
            if (subscription == null)
            {
                throw new ApplicationException(Resources.InvalidCurrentSubscription);
            }

            Debug.Assert(endpoint != null);

            SubscriptionCloudCredentials creds = AzureSession.AuthenticationFactory.GetSubscriptionCloudCredentials(subscription, profile);
            return CreateClient<TClient>(creds, endpoint);
        }

        public TClient CreateClient<TClient>(AzureSubscription subscription, AzureEnvironment.Endpoint endpointName, AzureProfile profile) where TClient : ServiceClient<TClient>
        {
            if (subscription == null)
            {
                throw new ApplicationException(Resources.InvalidCurrentSubscription);
            }

            Debug.Assert(profile != null);
            Debug.Assert(profile.Environments != null);
            Debug.Assert(profile.Environments.ContainsKey(subscription.Environment));

            return CreateClient<TClient>(subscription, endpointName, profile.Environments[subscription.Environment]);
        }

        public TClient CreateClient<TClient>(AzureSubscription subscription, AzureEnvironment.Endpoint endpointName, AzureEnvironment environment) where TClient : ServiceClient<TClient>
        {
            if (subscription == null)
            {
                throw new ApplicationException(Resources.InvalidCurrentSubscription);
            }

            Debug.Assert(environment != null);
            Debug.Assert(environment.Endpoints.ContainsKey(endpointName));

            Uri endpoint = environment.GetEndpointAsUri(endpointName);
            return CreateClient<TClient>(subscription, endpoint);
        }

        public TClient CreateClient<TClient>(AzureSubscription subscription, AzureEnvironment.Endpoint endpointName) where TClient : ServiceClient<TClient>
        {
            if (subscription == null)
            {
                throw new ApplicationException(Resources.InvalidCurrentSubscription);
            }

            ProfileClient profileClient = new ProfileClient();
            return CreateClient<TClient>(subscription, endpointName, profileClient.Profile);
        }

        public TClient CreateClient<TClient>(params object[] parameters) where TClient : ServiceClient<TClient>
        {
            List<Type> types = new List<Type>();
            foreach (object obj in parameters)
            {
                types.Add(obj.GetType());
            }

            var constructor = typeof(TClient).GetConstructor(types.ToArray());

            if (constructor == null)
            {
                throw new InvalidOperationException(string.Format(Resources.InvalidManagementClientType, typeof(TClient).Name));
            }

            TClient client = (TClient)constructor.Invoke(parameters);
            client.UserAgent.Add(ApiConstants.UserAgentValue);

            if (OnClientCreated != null)
            {
                OnClientCreated(this, new ClientCreatedArgs { CreatedClient = client, ClientType = client.GetType() });
            }
            
            return client;
        }

        HttpClient IClientFactory.CreateHttpClient(string endpoint, ICredentials credentials)
        {
            return CreateHttpClient(endpoint, credentials);
        }

        HttpClient IClientFactory.CreateHttpClient(string endpoint, HttpMessageHandler effectiveHandler)
        {
            return CreateHttpClient(endpoint, effectiveHandler);
        }

        public static HttpClient CreateHttpClient(string endpoint, ICredentials credentials)
        {
            return CreateHttpClient(endpoint, CreateHttpClientHandler(endpoint, credentials));
        }

        public static HttpClient CreateHttpClient(string endpoint, HttpMessageHandler effectiveHandler)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }

            Uri serviceAddr = new Uri(endpoint);
            HttpClient client = new HttpClient(effectiveHandler)
            {
                BaseAddress = serviceAddr,
                MaxResponseContentBufferSize = 30 * 1024 * 1024
            };

            client.DefaultRequestHeaders.Accept.Clear();

            return client;
        }

        public static HttpClientHandler CreateHttpClientHandler(string endpoint, ICredentials credentials)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }

            // Set up our own HttpClientHandler and configure it
            HttpClientHandler clientHandler = new HttpClientHandler();

            if (credentials != null)
            {
                // Set up credentials cache which will handle basic authentication
                CredentialCache credentialCache = new CredentialCache();

                // Get base address without terminating slash
                string credentialAddress = new Uri(endpoint).GetLeftPart(UriPartial.Authority).TrimEnd(uriPathSeparator);

                // Add credentials to cache and associate with handler
                NetworkCredential networkCredentials = credentials.GetCredential(new Uri(credentialAddress), "Basic");
                credentialCache.Add(new Uri(credentialAddress), "Basic", networkCredentials);
                clientHandler.Credentials = credentialCache;
                clientHandler.PreAuthenticate = true;
            }

            // Our handler is ready
            return clientHandler;
        }
    }
}
