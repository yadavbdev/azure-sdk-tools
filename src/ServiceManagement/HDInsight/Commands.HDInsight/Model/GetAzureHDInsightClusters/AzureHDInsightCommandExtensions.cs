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
using Microsoft.Hadoop.Client;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandImplementations;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.BaseInterfaces;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.Extensions;

namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters
{
    internal static class AzureHDInsightCommandExtensions
    {
        public static IHDInsightSubscriptionCredentials GetSubscriptionCredentials(this IAzureHDInsightCommonCommandBase command, AzureSubscription currentSubscription, AzureEnvironment environment)
        {
            var userId = currentSubscription.GetProperty(AzureSubscription.Property.DefaultPrincipalName) ??
                currentSubscription.GetPropertyAsArray(AzureSubscription.Property.AvailablePrincipalNames).FirstOrDefault();

            if (currentSubscription.GetProperty(AzureSubscription.Property.Thumbprint) != null)
            {
                return GetSubscriptionCertificateCredentials(command, currentSubscription, environment);
            }
            else if (userId != null)
            {
                return GetAccessTokenCredentials(command, currentSubscription, environment);
            }

            throw new NotSupportedException();
        }

        public static IHDInsightSubscriptionCredentials GetSubscriptionCertificateCredentials(this IAzureHDInsightCommonCommandBase command, AzureSubscription currentSubscription, AzureEnvironment environment)
        {
            return new HDInsightCertificateCredential
            {
                SubscriptionId = currentSubscription.Id,
                Certificate = ProfileClient.DataStore.GetCertificate(currentSubscription.GetProperty(AzureSubscription.Property.Thumbprint)),
                Endpoint = environment.GetEndpointAsUri(AzureEnvironment.Endpoint.ServiceEndpoint),
            };
        }

        public static IHDInsightSubscriptionCredentials GetAccessTokenCredentials(this IAzureHDInsightCommonCommandBase command, AzureSubscription currentSubscription, AzureEnvironment environment)
        {
            UserCredentials credentials = new UserCredentials
            {
                UserName = currentSubscription.GetProperty(AzureSubscription.Property.DefaultPrincipalName),
                NoPrompt = true
            };
            var accessToken = AzureSession.AuthenticationFactory.Authenticate(environment, ref credentials);
            return new HDInsightAccessTokenCredential()
            {
                SubscriptionId = currentSubscription.Id,
                AccessToken = accessToken.AccessToken
            };
        }

        public static IJobSubmissionClientCredential GetJobSubmissionClientCredentials(this IAzureHDInsightJobCommandCredentialsBase command, AzureSubscription currentSubscription, AzureEnvironment environment, string cluster)
        {
            IJobSubmissionClientCredential clientCredential = null;
            if (command.Credential != null)
            {
                clientCredential = new BasicAuthCredential
                {
                    Server = GatewayUriResolver.GetGatewayUri(cluster),
                    UserName = command.Credential.UserName,
                    Password = command.Credential.GetCleartextPassword()
                };
            }
            else if (currentSubscription.IsNotNull())
            {
                var subscriptionCredentials = GetSubscriptionCredentials(command, currentSubscription, environment);
                var asCertificateCredentials = subscriptionCredentials as HDInsightCertificateCredential;
                var asTokenCredentials = subscriptionCredentials as HDInsightAccessTokenCredential;
                if (asCertificateCredentials.IsNotNull())
                {
                    clientCredential = new JobSubmissionCertificateCredential(asCertificateCredentials, cluster);
                }
                else if (asTokenCredentials.IsNotNull())
                {
                    clientCredential = new JobSubmissionAccessTokenCredential(asTokenCredentials, cluster);
                }
            }

            return clientCredential;
        }
    }
}
