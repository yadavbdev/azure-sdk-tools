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

using System.Linq;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Helpers;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    /// <summary>
    /// Get-AzureVMSqlServerExtension implementation
    /// </summary>
    [Cmdlet(
        VerbsCommon.Get,
        VirtualMachineSqlServerExtensionNoun,
        DefaultParameterSetName = GetSqlServerExtensionParamSetName),
    OutputType(
        typeof(VirtualMachineSqlServerExtensionContext))]
    public class GetAzureVMSqlServerExtensionCommand : VirtualMachineSqlServerExtensionCmdletBase
    {
        protected const string GetSqlServerExtensionParamSetName = "GetSqlServerExtension";
        protected const string AutoPatchingStatusMessageName = "Automatic Patching";
        protected const string AutoBackupStatusMessageName = "Automatic Backup";

        internal void ExecuteCommand()
        {
            var extensionRefs = GetPredicateExtensionList();
            WriteObject(
                extensionRefs == null ? null : extensionRefs.Select(
                r =>
                {
                    GetExtensionValues(r);

                    return this.GetExtensionContext(r);
                }), true);

        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            ExecuteCommand();
        }

        /// <summary>
        /// Get the SQL Extension's context
        /// </summary>
        /// <returns></returns>
        private VirtualMachineSqlServerExtensionContext GetExtensionContext(ResourceExtensionReference r)
        {
            VirtualMachineSqlServerExtensionContext context = new VirtualMachineSqlServerExtensionContext
            {
                ExtensionName = r.Name,
                Publisher = r.Publisher,
                ReferenceName = r.ReferenceName,
                Version = r.Version,
                State = r.State,
                PublicConfiguration = PublicConfiguration,
                PrivateConfiguration = SecureStringHelper.GetSecureString(PrivateConfiguration),
                RoleName = VM.GetInstance().RoleName,
            };
            
            // gather extension status messages
            List<string> statusMessageList = new List<string>();

            PersistentVMRoleContext pvr = (PersistentVMRoleContext)VM;
            if (null != pvr)
            {
                foreach (ResourceExtensionStatus res in pvr.ResourceExtensionStatusList)
                {
                    // skip all non-sql extensions
                    if (res.HandlerName.Equals(r.Publisher, System.StringComparison.InvariantCulture))
                    {
                        continue;
                    }

                    if (null != res.ExtensionSettingStatus)
                    {
                        context.SubStatusList = res.ExtensionSettingStatus.SubStatusList;

                        foreach (ResourceExtensionSubStatus status in res.ExtensionSettingStatus.SubStatusList)
                        {
                            if (null != status.FormattedMessage)
                            {
                                string formattedMessage = status.FormattedMessage.Message;

                                // get current auto patching and auto backup config from extension status message
                                if (status.Name.Equals(AutoPatchingStatusMessageName, System.StringComparison.InvariantCulture))
                                {
                                    context.AutoPatchingSettings = DeSerializeAutoPatchingSettings(status.Name, formattedMessage);
                                }

                                if (status.Name.Equals(AutoBackupStatusMessageName, System.StringComparison.InvariantCulture))
                                {
                                    context.AutoBackupSettings = DeSerializeAutoBackupSettings(status.Name, formattedMessage);
                                }

                                statusMessageList.Add(formattedMessage);
                            }
                        }
                    }
                }
            }

            context.StatusMessages = statusMessageList;
            return context;
        }

        private AutoPatchingSettings DeSerializeAutoPatchingSettings(string category, string input)
        {
            AutoPatchingSettings aps = new AutoPatchingSettings();
            
            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    aps = JsonConvert.DeserializeObject<AutoPatchingSettings>(input);
                    aps.UpdatePatchingCategory(this.ResolvePatchCategoryStringforPowerShell(aps.PatchCategory));
                }
                catch (JsonReaderException jre)
                {
                    WriteVerboseWithTimestamp("Category:" + category);
                    WriteVerboseWithTimestamp("Message:" + input);
                    WriteVerboseWithTimestamp(jre.ToString());
                }
            }

            return aps;
        }

        private AutoBackupSettings DeSerializeAutoBackupSettings(string category, string input)
        {
            AutoBackupSettings abs = new AutoBackupSettings();

            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    abs = JsonConvert.DeserializeObject<AutoBackupSettings>(input);
                }
                catch (JsonReaderException jre)
                {
                    WriteVerboseWithTimestamp("Category:" + category);
                    WriteVerboseWithTimestamp("Message:" + input);
                    WriteVerboseWithTimestamp(jre.ToString());
                }
            }

            return abs;
        }

        /// <summary>
        /// map strings Auto-patching public settings -> Powershell API
        ///      "WindowsMandatoryUpdates" -> "Important"
        ///       "MicrosoftOptionalUpdates" -> "Optional"
        /// 
        /// </summary>
        /// <param name="patchCategory"></param>
        /// <returns></returns>
        private AzureVMSqlServerAutoPatchingPatchCategoryEnum ResolvePatchCategoryStringforPowerShell(string category)
        {
            AzureVMSqlServerAutoPatchingPatchCategoryEnum patchCategory = AzureVMSqlServerAutoPatchingPatchCategoryEnum.Important;

            if (!string.IsNullOrEmpty(category))
            {
                switch (category.ToLower())
                {
                    case "windowsmandatoryupdates":
                        patchCategory = AzureVMSqlServerAutoPatchingPatchCategoryEnum.Important;
                        break;

                    case "microsoftoptionalupdates":
                        patchCategory = AzureVMSqlServerAutoPatchingPatchCategoryEnum.Optional;
                        break;

                    default:
                        patchCategory = AzureVMSqlServerAutoPatchingPatchCategoryEnum.Unknown;
                        break;
                }
            }

            return patchCategory;
        }
    }
}
