using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandInterfaces;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.DataObjects;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.Extensions;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.ServiceLocation;

namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandImplementations
{
    internal class UpdateAzureHDInsightClusterCommand : AzureHDInsightClusterCommand<AzureHDInsightCluster>,
        IUpdateAzureHDInsightClusterCommand
    {
        private SwitchParameter force;
        private SwitchParameter changeClusterSize;
        private string location;

        public SwitchParameter Force
        {
            get { return force; }
            set { force = value; }
        }

        public SwitchParameter ChangeClusterSize
        {
            get { return changeClusterSize; }
            set { changeClusterSize = value; }
        }

        public int ClusterSizeInNodes { get; set; }

        public override Task EndProcessing()
        {
            Name.ArgumentNotNull("Name");
            if (ChangeClusterSize.IsPresent)
            {
                var client = GetClient();
                //client.ChangeClusterSize()
            }
        }
    }
}
