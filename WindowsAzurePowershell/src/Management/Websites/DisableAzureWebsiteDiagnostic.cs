﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Management.Websites
{
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Utilities.Websites;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.DeploymentEntities;

    //[Cmdlet(VerbsLifecycle.Disable, "AzureWebsiteApplicationDiagnostic"), OutputType(typeof(bool))]
    public class DisableAzureWebsiteApplicationDiagnosticCommand : WebsiteContextBaseCmdlet
    {
        private const string SiteParameterSetName = "SiteParameterSet";

        private const string ApplicationParameterSetName = "ApplicationParameterSet";

        public IWebsitesClient WebsitesClient { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Mandatory = true)]
        public WebsiteDiagnosticType Type { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = SiteParameterSetName)]
        public SwitchParameter WebServerLogging { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = SiteParameterSetName)]
        public SwitchParameter DetailedErrorMessages { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = SiteParameterSetName)]
        public SwitchParameter FailedRequestTracing { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = ApplicationParameterSetName)]
        public WebsiteDiagnosticOutput Output { get; set; }

        /// <summary>
        /// Initializes a new instance of the DisableAzureWebsiteApplicationDiagnosticCommand class.
        /// </summary>
        public DisableAzureWebsiteApplicationDiagnosticCommand()
            : this(null)
        {
        }

        public DisableAzureWebsiteApplicationDiagnosticCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        public override void ExecuteCmdlet()
        {
            WebsitesClient = WebsitesClient ?? new WebsitesClient(CurrentSubscription, WriteDebug);

            switch (Type)
            {
                case WebsiteDiagnosticType.Site:
                    WebsitesClient.DisableSiteDiagnostic(
                        Name,
                        WebServerLogging,
                        DetailedErrorMessages,
                        FailedRequestTracing);
                    break;
                case WebsiteDiagnosticType.Application:
                    WebsitesClient.DisableApplicationDiagnostic(Name, Output);
                    break;
                default:
                    throw new PSArgumentException();
            }

            if (PassThru.IsPresent)
            {
                WriteObject(true);
            }
        }
    }
}
