using System;
using System.Management.Automation;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandInterfaces;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.DataObjects;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.Extensions;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.PSCmdlets;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.ServiceLocation;
using Microsoft.WindowsAzure.Management.HDInsight.Logging;

namespace Microsoft.WindowsAzure.Commands.HDInsight.Cmdlet.PSCmdlets
{
    [Cmdlet(VerbsData.Update, "AzureHDInsightCluster")]
    [OutputType(typeof(AzureHDInsightCluster))]
    public class UpdateAzureHDInsightClusterCmdlet : AzureHDInsightCmdlet
    {
        private readonly IUpdateAzureHDInsightClusterCommand command;

        /// <inheritdoc />
        [Parameter(Mandatory = false, HelpMessage = "The management certificate used to manage the Azure subscription.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasCert)]
        public X509Certificate2 Certificate
        {
            get { return command.Certificate; }
            set { command.Certificate = value; }
        }

        /// <inheritdoc />
        [Parameter(Mandatory = false, HelpMessage = "The subscription id for the Azure subscription.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasSub)]
        public string Subscription
        {
            get { return command.Subscription; }
            set { command.Subscription = value; }
        }

        /// <inheritdoc />
        [Parameter(Mandatory = false, HelpMessage = "The Endpoint to use when connecting to Azure.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        public Uri Endpoint
        {
            get { return command.Endpoint; }
            set { command.Endpoint = value; }
        }

        /// <inheritdoc />
        [Parameter(Mandatory = true, HelpMessage = "The name of the HDInsight cluster to update.", ValueFromPipeline = true,
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Alias(AzureHdInsightPowerShellConstants.AliasClusterName, AzureHdInsightPowerShellConstants.AliasDnsName)]
        public string Name
        {
            get { return command.Name; }
            set { command.Name = value; }
        }

        public UpdateAzureHDInsightClusterCmdlet()
        {
            command = ServiceLocator.Instance.Locate<IAzureHDInsightCommandFactory>().UpdateCluster();
        }

        protected override void EndProcessing()
        {
            try
            {
                command.Logger = Logger;
                command.CurrentSubscription = GetCurrentSubscription(Subscription, Certificate);
                var task = command.EndProcessing();
                var token = command.CancellationToken;
                while (!task.IsCompleted)
                {
                    WriteDebugLog();
                    task.Wait(1000, token);
                }
                if (task.IsFaulted)
                {
                    throw new AggregateException(task.Exception);
                }
                foreach (var output in command.Output)
                {
                    WriteObject(output);
                }
                WriteDebugLog();
            }
            catch (Exception ex)
            {
                var type = ex.GetType();
                Logger.Log(Severity.Error, Verbosity.Normal, FormatException(ex));
                WriteDebugLog();
                if (type == typeof(AggregateException) || type == typeof(TargetInvocationException) || type == typeof(TaskCanceledException))
                {
                    ex.Rethrow();
                }
                else
                {
                    throw;
                }
            }
            WriteDebugLog();
        }

        protected override void StopProcessing()
        {
            this.command.Cancel();
        }
    }
}
