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

using System.Collections.Generic;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Xunit;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Commands.Common.Factories;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using Microsoft.WindowsAzure.Commands.Common.Models;

namespace Microsoft.WindowsAzure.Commands.Common.Test.Common
{
    public class AuthenticationFactoryTests
    {
        //[Fact]
        //public void FooBar()
        //{
        //    AuthenticationFactory authFactory = new AuthenticationFactory();
        //    authFactory.TokenProvider = new MockAccessTokenProvider("testtoken", "testuser");
        //    var subscriptionId = Guid.NewGuid();
        //    var accessToken = authFactory.TokenProvider.GetAccessToken(null, ShowDialog.Auto, "testuser", null);
        //    AzureSession.SubscriptionTokenCache[Tuple.Create(subscriptionId, "testuser")] = accessToken;
        //    var credential = authFactory.GetSubscriptionCloudCredentials(new Models.AzureContext
        //    {
        //        Account = new AzureAccount{Id = "testuser", Type = AzureAccount.AccountType.User},
        //        Environment = AzureEnvironment.PublicEnvironments["AzureCloud"],
        //        Subscription = new AzureSubscription { Id = subscriptionId }
        //    });

        //    Assert.True(credential is AccessTokenCredential);
        //    Assert.Equal(subscriptionId, new Guid(((AccessTokenCredential)credential).SubscriptionId));

        //}
    }
}
