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

using Microsoft.Azure.Gallery;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Subscriptions;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Management.Monitoring.Events;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Testing;
using Microsoft.Azure.Management.Authorization;
using Microsoft.Azure.Graph.RBAC;
using Microsoft.Azure.Utilities.HttpRecorder;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.Resources.Test.ScenarioTests
{
    public abstract class ResourcesTestsBase
    {
        private EnvironmentSetupHelper helper;
        protected const string TenantIdKey = "TenantId";
        protected const string DomainKey = "Domain";

        protected GraphRbacManagementClient GraphClient { get; private set; }

        protected string UserDomain { get; private set; }
        
        protected ResourcesTestsBase()
        {
            helper = new EnvironmentSetupHelper();
        }

        protected void SetupManagementClients()
        {
            var resourceManagementClient = GetResourceManagementClient();
            var subscriptionsClient = GetSubscriptionClient();
            var galleryClient = GetGalleryClient();
            var eventsClient = GetEventsClient();
            var authorizationManagementClient = GetAuthorizationManagementClient();
            GraphClient = GetGraphClient();

            helper.SetupManagementClients(resourceManagementClient,
                subscriptionsClient,
                galleryClient,
                eventsClient,
                authorizationManagementClient,
                GraphClient);
        }

        private GraphRbacManagementClient GetGraphClient()
        {
            var factory = new CSMTestEnvironmentFactory();
            var environment = factory.GetTestEnvironment();
            string tenantId = null;

            if (HttpMockServer.Mode == HttpRecorderMode.Record)
            {
                tenantId = environment.AuthorizationContext.TenatId;
                UserDomain = environment.AuthorizationContext.UserId
                                .Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries)
                                .Last();
                
                HttpMockServer.Variables[TenantIdKey] = tenantId;
                HttpMockServer.Variables[DomainKey] = UserDomain;
            }
            else if (HttpMockServer.Mode == HttpRecorderMode.Playback)
            {
                tenantId = HttpMockServer.Variables[TenantIdKey];
                UserDomain = HttpMockServer.Variables[DomainKey];
            }

            return TestBase.GetGraphServiceClient<GraphRbacManagementClient>(factory, tenantId);
        }

        protected AuthorizationManagementClient GetAuthorizationManagementClient()
        {
            return TestBase.GetServiceClient<AuthorizationManagementClient>(new CSMTestEnvironmentFactory());
        }

        protected void RunPowerShellTest(params string[] scripts)
        {
            RunPowerShellTest(() => scripts, null);
        }

        protected void RunPowerShellTest(Func<string []> scriptBuilder, Action cleanup)
        {
            if(scriptBuilder == null)
            {
                throw new ArgumentException("scriptBuilder delegate cannot be null.");
            }

            using (UndoContext context = UndoContext.Current)
            {
                context.Start(TestUtilities.GetCallingClass(2), TestUtilities.GetCurrentMethodName(2));

                SetupManagementClients();

                helper.SetupEnvironment(AzureModule.AzureResourceManager);
                helper.SetupModules(AzureModule.AzureResourceManager, "ScenarioTests\\Common.ps1",
                    "ScenarioTests\\" + this.GetType().Name + ".ps1");

                try
                {
                    helper.RunPowerShellTest(scriptBuilder());
                }
                finally
                {
                    if(cleanup !=null)
                    {
                        cleanup();
                    }
                }
            }
        }

        protected ResourceManagementClient GetResourceManagementClient()
        {
            return TestBase.GetServiceClient<ResourceManagementClient>(new CSMTestEnvironmentFactory());
        }

        protected SubscriptionClient GetSubscriptionClient()
        {
            return TestBase.GetServiceClient<SubscriptionClient>(new CSMTestEnvironmentFactory());
        }

        protected GalleryClient GetGalleryClient()
        {
            return TestBase.GetServiceClient<GalleryClient>(new CSMTestEnvironmentFactory());
        }

        protected EventsClient GetEventsClient()
        {
            return TestBase.GetServiceClient<EventsClient>(new CSMTestEnvironmentFactory());
        }

    }
}
