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
        private int clusterSizeInNodes;

        public SwitchParameter Force
        {
            get { return force; }
            set { force = value; }
        }

        public int ClusterSizeInNodes { 
            get { return clusterSizeInNodes; }
            set { clusterSizeInNodes = value; }
        }

        public override async Task EndProcessing()
        {
            Name.ArgumentNotNull("Name");
            IHDInsightClient client = GetClient();
            var getCommand = ServiceLocator.Instance.Locate<IAzureHDInsightCommandFactory>().CreateGet();
            getCommand.CurrentSubscription = CurrentSubscription;
            getCommand.Name = Name;
            await getCommand.EndProcessing();
            var cluster = getCommand.Output.First();
            if (true)
            {
                var currentSize = cluster.ClusterSizeInNodes;
            }
        }
    }
}
