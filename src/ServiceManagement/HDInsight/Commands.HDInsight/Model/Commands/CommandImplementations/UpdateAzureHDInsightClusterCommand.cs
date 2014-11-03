using System.Management.Automation;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandInterfaces;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.DataObjects;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters;

namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandImplementations
{
    internal class UpdateAzureHDInsightClusterCommand : AzureHDInsightClusterCommand<AzureHDInsightCluster>,
        IUpdateAzureHDInsightClusterCommand
    {
        public override Task EndProcessing()
        {
            throw new System.NotImplementedException();
        }
    }
}
