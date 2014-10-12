// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.Azure.Commands.Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using WindowsAzure;
    using WindowsAzure.Commands.Common;
    using WindowsAzure.Commands.Common.Models;
    using WindowsAzure.Commands.Common.Storage;
    using WindowsAzure.Common;
    using WindowsAzure.Management;
    using WindowsAzure.Management.Network;
    using WindowsAzure.Management.Network.Models;
    using WindowsAzure.Storage.Auth;
    using Gateway.Model;

    public class NetworkClient
    {
        private readonly NetworkManagementClient client;
        private readonly ManagementClient managementClient;
        private readonly ICommandRuntime commandRuntime;

        public NetworkClient(AzureSubscription subscription, ICommandRuntime commandRuntime)
            : this(CreateClient<NetworkManagementClient>(subscription),
                   CreateClient<ManagementClient>(subscription),
                   commandRuntime)
        {   
        }
        public NetworkClient(NetworkManagementClient client, ManagementClient managementClient, ICommandRuntime commandRuntime)
        {
            this.client = client;
            this.managementClient = managementClient;
            this.commandRuntime = commandRuntime;
        }

        public VirtualNetworkGatewayContext GetGateway(string vnetName)
        {
            if (string.IsNullOrWhiteSpace(vnetName))
            {
                throw new ArgumentException("vnetName cannot be null or whitespace.", "vnetName");
            }

            GatewayGetResponse response = client.Gateways.Get(vnetName);
            OperationStatusResponse operationStatus = managementClient.GetOperationStatus(response.RequestId);
            
            VirtualNetworkGatewayContext gatewayContext = new VirtualNetworkGatewayContext()
            {
                OperationId = operationStatus.Id,
                OperationStatus = operationStatus.Status.ToString(),
                OperationDescription = commandRuntime.ToString(),
                LastEventData = (response.LastEvent != null) ? response.LastEvent.Data : null,
                LastEventMessage = (response.LastEvent != null) ? response.LastEvent.Message : null,
                LastEventID = GetEventId(response.LastEvent),
                LastEventTimeStamp = (response.LastEvent != null) ? (DateTime?)response.LastEvent.Timestamp : null,
                State = (ProvisioningState)Enum.Parse(typeof(ProvisioningState), response.State, true),
                VIPAddress = response.VipAddress,
            };

            return gatewayContext;
        }

        public IEnumerable<GatewayConnectionContext> ListConnections(string vnetName)
        {
            GatewayListConnectionsResponse response = client.Gateways.ListConnections(vnetName);
            OperationStatusResponse operationStatus = managementClient.GetOperationStatus(response.RequestId);

            IEnumerable<GatewayConnectionContext> connections = response.Connections.Select(
                (GatewayListConnectionsResponse.GatewayConnection connection) =>
                {
                    return new GatewayConnectionContext()
                    {
                        OperationId               = operationStatus.Id,
                        OperationDescription      = commandRuntime.ToString(),
                        OperationStatus           = operationStatus.Status.ToString(),
                        ConnectivityState         = connection.ConnectivityState.ToString(),
                        EgressBytesTransferred    = (ulong)connection.EgressBytesTransferred,
                        IngressBytesTransferred   = (ulong)connection.IngressBytesTransferred,
                        LastConnectionEstablished = connection.LastConnectionEstablished.ToString(),
                        LastEventID               = connection.LastEvent != null ? connection.LastEvent.Id : null,
                        LastEventMessage          = connection.LastEvent != null ? connection.LastEvent.Message : null,
                        LastEventTimeStamp        = connection.LastEvent != null ? connection.LastEvent.Timestamp.ToString() : null,
                        LocalNetworkSiteName      = connection.LocalNetworkSiteName
                    };
                });

            return connections;
        }

        public VirtualNetworkDiagnosticsContext GetDiagnostics(string vnetName)
        {
            GatewayDiagnosticsStatus diagnosticsStatus = client.Gateways.GetDiagnostics(vnetName);
            OperationStatusResponse operationStatus = managementClient.GetOperationStatus(diagnosticsStatus.RequestId);

            VirtualNetworkDiagnosticsContext diagnosticsContext = new VirtualNetworkDiagnosticsContext()
            {
                OperationId = operationStatus.Id,
                OperationStatus = operationStatus.Status.ToString(),
                OperationDescription = commandRuntime.ToString(),
                DiagnosticsUrl = diagnosticsStatus.DiagnosticsUrl,
                State = diagnosticsStatus.State,
            };

            return diagnosticsContext;
        }

        public SharedKeyContext GetSharedKey(string vnetName, string localNetworkSiteName)
        {
            GatewayGetSharedKeyResponse response = client.Gateways.GetSharedKey(vnetName, localNetworkSiteName);
            OperationStatusResponse operationStatus = managementClient.GetOperationStatus(response.RequestId);

            SharedKeyContext sharedKeyContext = new SharedKeyContext()
            {
                OperationId = operationStatus.Id,
                OperationDescription = commandRuntime.ToString(),
                OperationStatus = operationStatus.Status.ToString(),
                Value = response.SharedKey
            };

            return sharedKeyContext;
        }

        public GatewayGetOperationStatusResponse SetSharedKey(string vnetName, string localNetworkSiteName, string sharedKey)
        {
            GatewaySetSharedKeyParameters sharedKeyParameters = new GatewaySetSharedKeyParameters()
            {
                Value = sharedKey,
            };

            GatewayGetOperationStatusResponse response = client.Gateways.SetSharedKey(vnetName, localNetworkSiteName, sharedKeyParameters);

            return response;
        }

        public GatewayGetOperationStatusResponse CreateGateway(string vnetName, GatewayType gatewayType)
        {
            GatewayCreateParameters parameters = new GatewayCreateParameters()
            {
                GatewayType = gatewayType,
            };

            GatewayGetOperationStatusResponse response = client.Gateways.Create(vnetName, parameters);

            return response;
        }

        public GatewayGetOperationStatusResponse DeleteGateway(string vnetName)
        {
            GatewayGetOperationStatusResponse response = client.Gateways.Delete(vnetName);

            return response;
        }

        public GatewayGetOperationStatusResponse ConnectDisconnectOrTest(string vnetName, string localNetworkSiteName, bool isConnect)
        {
            GatewayConnectDisconnectOrTestParameters connParams = new GatewayConnectDisconnectOrTestParameters()
            {
                Operation = isConnect ? GatewayConnectionUpdateOperation.Connect : GatewayConnectionUpdateOperation.Disconnect
            };

            GatewayGetOperationStatusResponse response = client.Gateways.ConnectDisconnectOrTest(vnetName, localNetworkSiteName, connParams);

            return response;
        }

        public GatewayGetOperationStatusResponse StartDiagnostics(string vnetName, int captureDurationInSeconds, string containerName, AzureStorageContext storageContext)
        {
            StorageCredentials credentials = storageContext.StorageAccount.Credentials;
            string customerStorageKey = credentials.ExportBase64EncodedKey();
            string customerStorageName = credentials.AccountName;
            return StartDiagnostics(vnetName, captureDurationInSeconds, containerName, customerStorageKey, customerStorageName);
        }
        public GatewayGetOperationStatusResponse StartDiagnostics(string vnetName, int captureDurationInSeconds, string containerName, string customerStorageKey, string customerStorageName)
        {
            UpdateGatewayPublicDiagnostics parameters = new UpdateGatewayPublicDiagnostics()
            {
                CaptureDurationInSeconds = captureDurationInSeconds.ToString(),
                ContainerName = containerName,
                CustomerStorageKey = customerStorageKey,
                CustomerStorageName = customerStorageName,
                Operation = UpdateGatewayPublicDiagnosticsOperation.StartDiagnostics,
            };

            GatewayGetOperationStatusResponse response = client.Gateways.UpdateDiagnostics(vnetName, parameters);

            return response;
        }

        public GatewayGetOperationStatusResponse StopDiagnostics(string vnetName)
        {
            UpdateGatewayPublicDiagnostics parameters = new UpdateGatewayPublicDiagnostics()
            {
                Operation = UpdateGatewayPublicDiagnosticsOperation.StopDiagnostics,
            };

            GatewayGetOperationStatusResponse response = client.Gateways.UpdateDiagnostics(vnetName, parameters);

            return response;
        }

        private int GetEventId(GatewayEvent gatewayEvent)
        {
            int val = -1;
            if (gatewayEvent != null)
            {
                int.TryParse(gatewayEvent.Id, out val);
            }

            return val;
        }

        private static ClientType CreateClient<ClientType>(AzureSubscription subscription) where ClientType : ServiceClient<ClientType>
        {
            return AzureSession.ClientFactory.CreateClient<ClientType>(subscription, AzureEnvironment.Endpoint.ServiceManagement);
        }
    }
}
