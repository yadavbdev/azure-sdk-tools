using System.Management.Automation;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.DataObjects;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters;

namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandInterfaces
{
    internal interface IUpdateAzureHDInsightClusterCommand : IAzureHDInsightCommand<AzureHDInsightCluster>, IAzureHDInsightClusterCommandBase
    {
    }
}
