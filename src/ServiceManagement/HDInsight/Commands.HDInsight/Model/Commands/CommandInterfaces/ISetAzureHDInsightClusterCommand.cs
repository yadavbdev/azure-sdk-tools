using System.Management.Automation;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.DataObjects;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters;

namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandInterfaces
{
    internal interface ISetAzureHDInsightClusterCommand : IAzureHDInsightCommand<AzureHDInsightCluster>, IAzureHDInsightClusterCommandBase
    {
        SwitchParameter Force { get; set; }

        SwitchParameter ChangeClusterSize { get; set; }

        int ClusterSizeInNodes { get; set; }

        string Location { get; set; }
    }
}
