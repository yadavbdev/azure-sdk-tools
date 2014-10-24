using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Security;
using Microsoft.WindowsAzure.Commands.Common.Storage;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;


namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    /// <summary>
    /// Helper cmdlet to construct instance of AutoBackup settings class
    /// </summary>
   [Cmdlet(
        VerbsCommon.New,
        AzureVMSqlServerAutoBackupConfigNoun),
    OutputType(
        typeof(AutoBackupSettings))]
    public class NewAzureVMSqlServerAutoBackupConfigCommand : ServiceManagementBaseCmdlet
    {
        protected const string AzureVMSqlServerAutoBackupConfigNoun = "AzureVMSqlServerAutoBackupConfig";

        [Parameter(ValueFromPipelineByPropertyName = true,
         Mandatory = true,
         HelpMessage = "The storage connection context")]
        [ValidateNotNullOrEmpty]
        public AzureStorageContext StorageContext
        {
            get;
            set;
        }

        [Parameter]
        public SwitchParameter Enable { get; set; }

        [Parameter]
        public SwitchParameter EnableEncryption { get; set; }

        [Parameter]
        public int RetentionPeriod { get; set; }

        [Parameter]
        public string Password { get; set; }

        /// <summary>
        /// Initialzies a new instance of the <see cref="NewAzureVMSqlServerAutoBackupConfigCommand"/> class.
        /// </summary>
        public NewAzureVMSqlServerAutoBackupConfigCommand()
        {
        }

        /// <summary>
        /// Creates and returns <see cref="AutoBackupSettings"/> object.
        /// </summary>
        protected override void ProcessRecord()
        {
            AutoBackupSettings autoBackupSettings = new AutoBackupSettings();

            autoBackupSettings.Enable = (Enable.IsPresent) ? Enable.ToBool() : false;
            autoBackupSettings.EnableEncryption = (EnableEncryption.IsPresent) ? EnableEncryption.ToBool() : false;
            autoBackupSettings.RetentionPeriod = RetentionPeriod;
            autoBackupSettings.StorageUrl = this.StorageContext.BlobEndPoint;
            autoBackupSettings.StorageAccessKey = this.GetStorageKey();
            autoBackupSettings.Password = Password;

            WriteObject(autoBackupSettings);
        }

        protected string GetStorageKey()
        {
            string storageKey = string.Empty;
            string storageAccountName = this.StorageContext.StorageAccountName;

            if (!string.IsNullOrEmpty(storageAccountName))
            {
                var storageAccount = this.StorageClient.StorageAccounts.Get(storageAccountName);
                if (storageAccount != null)
                {
                    var keys = this.StorageClient.StorageAccounts.GetKeys(storageAccountName);
                    if (keys != null)
                    {
                        storageKey = !string.IsNullOrEmpty(keys.PrimaryKey) ? keys.PrimaryKey : keys.SecondaryKey;
                    }
                }
            }

            return storageKey;
        }
    }
}
