using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

using System.Security;

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
    public class NewAzureVMSqlServerAutoBackupConfigCommand : PSCmdlet
    {
        protected const string AzureVMSqlServerAutoBackupConfigNoun = "AzureVMSqlServerAutoBackupConfig";

        [Parameter]
        public bool Enable { get; set; }

        [Parameter]
        public bool EnableEncryption { get; set; }

        [Parameter]
        public int RetentionPeriod { get; set; }

        [Parameter]
        public string StorageUrl { get; set; }

        [Parameter]
        public string StorageAccessKey { get; set; }

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

            autoBackupSettings.Enable = Enable;
            autoBackupSettings.EnableEncryption = EnableEncryption;
            autoBackupSettings.RetentionPeriod = RetentionPeriod;
            autoBackupSettings.StorageUrl = StorageUrl;
            autoBackupSettings.StorageAccessKey = StorageAccessKey;
            autoBackupSettings.Password = Password;

            WriteObject(autoBackupSettings);
        }
        
    }
}
