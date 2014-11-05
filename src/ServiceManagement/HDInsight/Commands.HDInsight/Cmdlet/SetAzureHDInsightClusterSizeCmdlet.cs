using System;
using System.Globalization;
using System.Linq;
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
    [Cmdlet(VerbsCommon.Set, "AzureHDInsightClusterSize")]
    [OutputType(typeof(AzureHDInsightCluster))]
    public class SetAzureHDInsightClusterSizeCmdlet : AzureHDInsightCmdlet
    {
        private readonly ISetAzureHDInsightClusterSizeCommand command;

        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Specifies a Change Cluster Size operation..",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterResize)]
        public SwitchParameter ChangeClusterSize
        {
            get { return command.ChangeClusterSize; }
            set { command.ChangeClusterSize = value; }
        }

        [Parameter(Position = 1, Mandatory = true, HelpMessage = "The new cluster size in nodes requested.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterResize)]
        public int ClusterSizeInNodes
        {
            get { return command.ClusterSizeInNodes; }
            set { command.ClusterSizeInNodes = value; }
        }

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
        [Parameter(Mandatory = true, HelpMessage = "The name of the HDInsight cluster to set.", ValueFromPipeline = true,
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterByNameWithSpecificSubscriptionCredentials)]
        [Parameter(Mandatory = true, HelpMessage = "The name of the HDInsight cluster to set.", ValueFromPipeline = true,
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterResize)]
        [Alias(AzureHdInsightPowerShellConstants.AliasClusterName, AzureHdInsightPowerShellConstants.AliasDnsName)]
        public string Name
        {
            get { return command.Name; }
            set { command.Name = value; }
        }

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetClusterResize)]
        public SwitchParameter Force
        {
            get { return command.Force; }
            set { command.Force = value; }
        }

        public SetAzureHDInsightClusterSizeCmdlet()
        {
            command = ServiceLocator.Instance.Locate<IAzureHDInsightCommandFactory>().CreateSetClusterSize();
        }

        protected override void EndProcessing()
        {
            Name.ArgumentNotNull("Name");
            if (ChangeClusterSize.IsPresent)
            {
                if (ClusterSizeInNodes < 1)
                {
                    throw new ArgumentOutOfRangeException("ClusterSizeInNodes", "The requested ClusterSizeInNodes must be at least 1.");
                }
                try
                {
                    command.Logger = Logger;
                    var currentSubscription = GetCurrentSubscription(Subscription, Certificate);
                    command.CurrentSubscription = currentSubscription;
                    Func<Task> action = () => command.EndProcessing();
                    var token = command.CancellationToken;

                    //get cluster
                    var getCommand = ServiceLocator.Instance.Locate<IAzureHDInsightCommandFactory>().CreateGet();
                    getCommand.CurrentSubscription = currentSubscription;
                    getCommand.Name = Name;
                    var getTask = getCommand.EndProcessing();
                    while (!getTask.IsCompleted)
                    {
                        WriteDebugLog();
                        getTask.Wait(1000, token);
                    }
                    if (getTask.IsFaulted)
                    {
                        throw new AggregateException(getTask.Exception);
                    }
                    if (getCommand.Output == null || getCommand.Output.Count == 0)
                    {
                        throw new InvalidOperationException(string.Format("Could not find cluster {0}", Name));
                    }
                    var cluster = getCommand.Output.First();

                    //prep cluster resize operation
                    command.Location = cluster.Location;
                    if (ClusterSizeInNodes < cluster.ClusterSizeInNodes)
                    {
                        var task = ConfirmSetAction(
                            "You are requesting a cluster size that is less than the current cluster size. We recommend not running jobs till the operation is complete as all running jobs will fail at end of resize operation and may impact the health of your cluster. Do you want to continue?",
                            "Continuing with set cluster operation.",
                            ClusterSizeInNodes.ToString(CultureInfo.InvariantCulture),
                            action);
                        if (task == null)
                        {
                            throw new OperationCanceledException("The change cluster size operation was aborted.");
                        }
                        while (!task.IsCompleted)
                        {
                            WriteDebugLog();
                            task.Wait(1000, token);
                        }
                        if (task.IsFaulted)
                        {
                            throw new AggregateException(task.Exception);
                        }
                    }
                    else
                    {
                        var task = action();
                        while (!task.IsCompleted)
                        {
                            WriteDebugLog();
                            task.Wait(1000, token);
                        }
                        if (task.IsFaulted)
                        {
                            throw new AggregateException(task.Exception);
                        }
                    }
                    //print cluster details
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
                    if (type == typeof (AggregateException) || type == typeof (TargetInvocationException) ||
                        type == typeof (TaskCanceledException))
                    {
                        ex.Rethrow();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            WriteDebugLog();
        }

        protected Task ConfirmSetAction(string actionMessage, string processMessage, string target,
            Func<Task> action)
        {
            if (Force.IsPresent || ShouldContinue(actionMessage, ""))
            {
                if (ShouldProcess(target, processMessage))
                {
                    return action();
                }
            }
            return null;
        }

        protected override void StopProcessing()
        {
            command.Cancel();
        }
    }
}
