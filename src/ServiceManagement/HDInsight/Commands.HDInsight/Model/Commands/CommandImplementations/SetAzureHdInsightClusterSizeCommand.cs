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
    internal class SetAzureHdInsightClusterSizeCommand : AzureHDInsightClusterCommand<AzureHDInsightCluster>,
        ISetAzureHDInsightClusterSizeCommand
    {
        private SwitchParameter force;
        private SwitchParameter changeClusterSize;

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

        public string Location { get; set; }

        public int ClusterSizeInNodes { get; set; }

        public override async Task EndProcessing()
        {
            Name.ArgumentNotNull("Name");
            Location.ArgumentNotNull("Location");
            if (ChangeClusterSize.IsPresent)
            {
                var client = GetClient();
                var cluster = await client.ChangeClusterSizeAsync(Name, Location, ClusterSizeInNodes);
                if (cluster != null)
                {
                    Output.Add(new AzureHDInsightCluster(cluster));
                }
            }
        }
    }
}
