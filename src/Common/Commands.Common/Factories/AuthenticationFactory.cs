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
using System.Linq;
using System.Security;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using System.Diagnostics;

namespace Microsoft.WindowsAzure.Commands.Common.Factories
{
    public class AuthenticationFactory : IAuthenticationFactory
    {
        public const string CommonAdTenant = "Common";

        public AuthenticationFactory()
        {
            TokenProvider = new AdalTokenProvider();
        }

        public ITokenProvider TokenProvider { get; set; }

        public IAccessToken Authenticate(AzureEnvironment environment, ref UserCredentials credentials)
        {
            return Authenticate(environment, CommonAdTenant, ref credentials);
        }

        public IAccessToken Authenticate(AzureEnvironment environment, string tenant, ref UserCredentials credentials)
        {
            var token = TokenProvider.GetAccessToken(GetAdalConfiguration(environment, tenant), credentials.ShowDialog, credentials.UserName, credentials.Password);
            credentials.UserName = token.UserId;
            return token;
        }

        public SubscriptionCloudCredentials GetSubscriptionCloudCredentials(AzureContext context)
        {
            if (context.Subscription == null)
            {
                throw new ApplicationException(Resources.InvalidCurrentSubscription);
            }

            var account = context.Subscription.Account;

            if (!AzureSession.SubscriptionTokenCache.ContainsKey(context.Subscription.Id))
            {
                // Try to re-authenticate
                UserCredentials credentials = new UserCredentials
                    {
                        UserName = account,
                        ShowDialog = ShowDialog.Never
                    };

                var tenants = context.Subscription.GetPropertyAsArray(AzureSubscription.Property.Tenants)
                    .Intersect(context.Account.GetPropertyAsArray(AzureAccount.Property.Tenants));

                foreach (var tenant in tenants)
                {
                    try
                    {
                        AzureSession.SubscriptionTokenCache[context.Subscription.Id] = Authenticate(context.Environment, tenant, ref credentials);
                        break;
                    }
                    catch
                    {
                        // Skip
                    }
                }
            }

            if (AzureSession.SubscriptionTokenCache.ContainsKey(context.Subscription.Id))
            {
                return new AccessTokenCredential(context.Subscription.Id, AzureSession.SubscriptionTokenCache[context.Subscription.Id]);
            }
            else if (account != null)
            {
                switch (context.Account.Type)
                {
                    case AzureAccount.AccountType.User:
                        if (!AzureSession.SubscriptionTokenCache.ContainsKey(context.Subscription.Id))
                        {
                            throw new ArgumentException(Resources.InvalidSubscriptionState);
                        }
                        return new AccessTokenCredential(context.Subscription.Id, AzureSession.SubscriptionTokenCache[context.Subscription.Id]);

                    case AzureAccount.AccountType.Certificate:
                        var certificate = ProfileClient.DataStore.GetCertificate(account);
                        return new CertificateCloudCredentials(context.Subscription.Id.ToString(), certificate);

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                throw new ArgumentException(Resources.InvalidSubscriptionState);
            }
        }

        private AdalConfiguration GetAdalConfiguration(AzureEnvironment environment, string tenantId)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            var adEndpoint = environment.Endpoints[AzureEnvironment.Endpoint.ActiveDirectory];
            var adResourceId = environment.Endpoints[AzureEnvironment.Endpoint.ActiveDirectoryServiceEndpointResourceId];

            return new AdalConfiguration
            {
                AdEndpoint = adEndpoint,
                ResourceClientUri = adResourceId,
                AdDomain = tenantId
            };
        }
    }
}
