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

using Microsoft.Azure.Commands.DataFactories.Models;
using Microsoft.Azure.Commands.DataFactories.Properties;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using Microsoft.WindowsAzure.Storage.Auth;
namespace Microsoft.Azure.Commands.DataFactories
{
    [Cmdlet(VerbsData.Save, Constants.RunLog), OutputType(typeof(PSRunLogInfo))]
    public class SaveAzureDataFactoryLogCommand : DataFactoryBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The data factory name.")]
        [ValidateNotNullOrEmpty]
        public string DataFactoryName { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The data slice run id.")]
        [ValidateNotNullOrEmpty]
        public string Id { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Download logs using the SAS url.")]
        public SwitchParameter DownloadLogs { get; set; }

        [Parameter(Position = 2, Mandatory = false, HelpMessage = "Directory to download the log. Default is current directory.")]
        public string Output { get; set; }

        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public override void ExecuteCmdlet()
        {
            WebClient webClient = new WebClient();
            CloudBlockBlob blob;

            PSRunLogInfo runLog =
                DataFactoryClient.GetDataSliceRunLogsSharedAccessSignature(
                    ResourceGroupName, DataFactoryName, Id);

            blob = new CloudBlockBlob(new Uri(runLog.SasUri));

            if (DownloadLogs.IsPresent)
            {
                string directory = string.IsNullOrWhiteSpace(Output) 
                    ? Directory.GetCurrentDirectory() 
                    : Output;

                if (!HaveWriteAccess(directory))
                {
                    throw new IOException(string.Format(CultureInfo.InvariantCulture, Resources.NoWriteAccessToDirectory, directory));
                }

                blob.DownloadToFile(directory, FileMode.CreateNew, AccessCondition.GenerateEmptyCondition());

                WriteVerbose(string.Format(CultureInfo.InvariantCulture, Resources.DownloadLogCompleted, directory));
            }

            WriteObject(runLog);
        }

        private bool HaveWriteAccess(string directory)
        {
            bool writeAllow = false;
            bool writeDeny = false;

            DirectorySecurity accessControlList = Directory.GetAccessControl(directory);

            if (accessControlList == null)
            {
                return false;
            }

            var rules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));

            if (rules == null)
            {
                return false;
            }

            foreach (FileSystemAccessRule rule in rules)
            {
                if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                {
                    continue;
                }

                if (rule.AccessControlType == AccessControlType.Allow)
                {
                    writeAllow = true;
                }

                else if (rule.AccessControlType == AccessControlType.Deny)
                {
                    writeDeny = true;
                }
            }

            return writeAllow && !writeDeny;
        }
    }
}