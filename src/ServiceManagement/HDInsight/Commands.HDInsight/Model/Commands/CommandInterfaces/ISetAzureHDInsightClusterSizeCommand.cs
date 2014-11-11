using System.Management.Automation;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.DataObjects;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters;

namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandInterfaces
{
    /// <summary>
    ///     Command interface referenced by the cmdlet that allows a user to change the size of a cluster.
    /// </summary>
    internal interface ISetAzureHDInsightClusterSizeCommand : IAzureHDInsightCommand<AzureHDInsightCluster>, IAzureHDInsightClusterCommandBase
    {
        SwitchParameter Force { get; set; }
        
        int ClusterSizeInNodes { get; set; }

        string Location { get; set; }

        AzureHDInsightCluster Cluster { get; set; }
    }
}
