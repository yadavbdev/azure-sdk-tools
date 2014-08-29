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
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.Management.Resources;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Common;
using System.Diagnostics;
using Microsoft.WindowsAzure.Management;

namespace Microsoft.WindowsAzure.Commands.Common.Factories
{
    public class ClientFactory : IClientFactory
    {
        private static readonly char[] uriPathSeparator = { '/' };

        public TClient CreateClient<TClient>(AzureContext context, AzureEnvironment.Endpoint endpoint) where TClient : ServiceClient<TClient>
        {
            if (context == null)
            {
                throw new ApplicationException(Resources.InvalidCurrentSubscription);
            }

            if (!TestMockSupport.RunningMocked)
            {
                if (endpoint == AzureEnvironment.Endpoint.ServiceManagement)
                {
                    RegisterServiceManagementProviders<TClient>(context);
                } 
                else if (endpoint == AzureEnvironment.Endpoint.ResourceManager)
                {
                    RegisterResourceManagerProviders<TClient>(context);
                }
            }

            SubscriptionCloudCredentials creds = AzureSession.AuthenticationFactory.GetSubscriptionCloudCredentials(context);
            return CreateCustomClient<TClient>(creds, context.Environment.GetEndpointAsUri(endpoint));
        }

        /// <summary>
        /// TODO: Migrate all code that references this method to use AzureContext
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="subscription"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public TClient CreateClient<TClient>(AzureSubscription subscription, AzureEnvironment.Endpoint endpointName) where TClient : ServiceClient<TClient>
        {
            if (subscription == null)
            {
                throw new ApplicationException(Resources.InvalidCurrentSubscription);
            }

            ProfileClient profileClient = new ProfileClient();
            AzureContext context = new AzureContext
            {
                Subscription = subscription,
                Environment = profileClient.GetEnvironmentOrDefault(subscription.Environment),
                Account = profileClient.ListAccounts(subscription.Account).First()
            };
            return CreateClient<TClient>(context, endpointName);
        }

        public TClient CreateCustomClient<TClient>(params object[] parameters) where TClient : ServiceClient<TClient>
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
           
            return client;
        }

        /// <summary>
        /// Registers resource providers for Sparta.
        /// </summary>
        /// <typeparam name="T">The client type</typeparam>
        private void RegisterResourceManagerProviders<T>(AzureContext context) where T : ServiceClient<T>
        {
            var credentials = AzureSession.AuthenticationFactory.GetSubscriptionCloudCredentials(context);
            var providersToRegister = RequiredResourceLookup.RequiredProvidersForResourceManager<T>();
            var registeredProviders = context.Subscription.GetPropertyAsArray(AzureSubscription.Property.RegisteredResourceProviders);
            var unregisteredProviders = providersToRegister.Where(p => !registeredProviders.Contains(p)).ToList();
            var successfullyRegisteredProvider = new List<string>();

            if (unregisteredProviders.Count > 0)
            {
                using (IResourceManagementClient client = new ResourceManagementClient(credentials, 
                    context.Environment.GetEndpointAsUri(AzureEnvironment.Endpoint.ResourceManager)))
                {
                    foreach (string provider in unregisteredProviders)
                    {
                        try
                        {
                            client.Providers.Register(provider);
                            successfullyRegisteredProvider.Add(provider);
                        }
                        catch
                        {
                            // Ignore this as the user may not have access to Sparta endpoint or the provider is already registered
                        }
                    }
                }

                UpdateSubscriptionRegisteredProviders(context.Subscription, successfullyRegisteredProvider);
            }
        }

        /// <summary>
        /// Registers resource providers for RDFE.
        /// </summary>
        /// <typeparam name="T">The client type</typeparam>
        private void RegisterServiceManagementProviders<T>(AzureContext context) where T : ServiceClient<T>
        {
            var credentials = AzureSession.AuthenticationFactory.GetSubscriptionCloudCredentials(context);
            var providersToRegister = RequiredResourceLookup.RequiredProvidersForServiceManagement<T>();
            var registeredProviders = context.Subscription.GetPropertyAsArray(AzureSubscription.Property.RegisteredResourceProviders);
            var unregisteredProviders = providersToRegister.Where(p => !registeredProviders.Contains(p)).ToList();
            var successfullyRegisteredProvider = new List<string>();

            if (unregisteredProviders.Count > 0)
            {
                using (var client = new ManagementClient(credentials, context.Environment.GetEndpointAsUri(AzureEnvironment.Endpoint.ServiceManagement)))
                {
                    foreach (var provider in unregisteredProviders)
                    {
                        try
                        {
                            client.Subscriptions.RegisterResource(provider);
                        }
                        catch (CloudException ex)
                        {
                            if (ex.Response.StatusCode != HttpStatusCode.Conflict && ex.Response.StatusCode != HttpStatusCode.NotFound)
                            {
                                // Conflict means already registered, that's OK.
                                // NotFound means there is no registration support, like Windows Azure Pack.
                                // Otherwise it's a failure.
                                throw;
                            }
                        }
                        successfullyRegisteredProvider.Add(provider);
                    }
                }

                UpdateSubscriptionRegisteredProviders(context.Subscription, successfullyRegisteredProvider);
            }
        }

        private void UpdateSubscriptionRegisteredProviders(AzureSubscription subscription, List<string> providers)
        {
            var registeredProviders = subscription.GetPropertyAsArray(AzureSubscription.Property.RegisteredResourceProviders);
            subscription.SetProperty(AzureSubscription.Property.RegisteredResourceProviders, registeredProviders.Union(providers).ToArray());
            ProfileClient profileClient = new ProfileClient();
            profileClient.AddOrSetSubscription(subscription);
            profileClient.Profile.Save();
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
