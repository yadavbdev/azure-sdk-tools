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

namespace Microsoft.Azure.Commands.RecoveryServices
{
    #region Using directives
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.Azure.Commands.RecoveryServices.SiteRecovery;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Management.SiteRecovery.Models;
    #endregion

    /// <summary>
    /// Retrieves Azure Site Recovery Protection Entity.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSiteRecoveryProtectionEntity", DefaultParameterSetName = ASRParameterSets.ByObject)]
    [OutputType(typeof(IEnumerable<ASRProtectionEntity>))]
    public class GetAzureSiteRecoveryProtectionEntity : RecoveryServicesCmdletBase
    {
        #region Parameters

        /// <summary>
        /// Protection entity ID.
        /// </summary>
        private string id;

        /// <summary>
        /// Name of the Protection entity.
        /// </summary>
        private string name;

        /// <summary>
        /// Protection container ID.
        /// </summary>
        private string protectionContainerId;

        /// <summary>
        /// Protection container object.
        /// </summary>
        private ASRProtectionContainer protectionContainer;

        /// <summary>
        /// Gets or sets ID of the Virtual Machine.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByObjectWithId, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.ByIDsWithId, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Gets or sets name of the Virtual Machine.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByObjectWithName, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.ByIDsWithName, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets or sets ID of the ProtectionContainer containing the Virtual Machine.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByIDs, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.ByIDsWithId, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.ByIDsWithName, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string ProtectionContainerId
        {
            get { return this.protectionContainerId; }
            set { this.protectionContainerId = value; }
        }

        /// <summary>
        /// Gets or sets Server Object.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByObject, Mandatory = true, ValueFromPipeline = true)]
        [Parameter(ParameterSetName = ASRParameterSets.ByObjectWithId, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.ByObjectWithName, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public ASRProtectionContainer ProtectionContainer
        {
            get { return this.protectionContainer; }
            set { this.protectionContainer = value; }
        }
        #endregion Parameters

        /// <summary>
        /// ProcessRecord of the command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            try
            {
                switch (this.ParameterSetName)
                {
                    case ASRParameterSets.ByObject:
                    case ASRParameterSets.ByObjectWithId:
                    case ASRParameterSets.ByObjectWithName:
                        this.protectionContainerId = this.ProtectionContainer.ID;
                        break;
                    case ASRParameterSets.ByIDs:
                    case ASRParameterSets.ByIDsWithId:
                    case ASRParameterSets.ByIDsWithName:
                        break;
                }

                if (this.id != null)
                {
                    this.GetById();
                }
                else if (this.name != null)
                {
                    this.GetByName();
                }
                else
                {
                    this.GetAll();
                }
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
            }
        }

        /// <summary>
        /// Queries by name.
        /// </summary>
        private void GetByName()
        {
            ProtectionEntityListResponse protectionEntityListResponse =
                RecoveryServicesClient.GetAzureSiteRecoveryProtectionEntity(
                this.protectionContainerId);

            bool found = false;
            foreach (ProtectionEntity pe in protectionEntityListResponse.ProtectionEntities)
            {
                if (0 == string.Compare(this.name, pe.Name, true))
                {
                    this.WriteProtectionEntity(pe);
                    found = true;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException(
                    string.Format(
                    Properties.Resources.ProtectionEntityNotFound,
                    this.name,
                    this.protectionContainerId));
            }
        }

        /// <summary>
        /// Queries by Id.
        /// </summary>
        private void GetById()
        {
            ProtectionEntityResponse protectionEntityResponse =
                RecoveryServicesClient.GetAzureSiteRecoveryProtectionEntity(
                this.protectionContainerId,
                this.id);

            this.WriteProtectionEntity(protectionEntityResponse.ProtectionEntity);
        }

        /// <summary>
        /// Queries all.
        /// </summary>
        private void GetAll()
        {
            ProtectionEntityListResponse protectionEntityListResponse =
                RecoveryServicesClient.GetAzureSiteRecoveryProtectionEntity(
                this.protectionContainerId);

            this.WriteProtectionEntities(protectionEntityListResponse.ProtectionEntities);
        }

        /// <summary>
        /// Writes Protection Entities.
        /// </summary>
        /// <param name="protectionEntities">Protection Entities</param>
        private void WriteProtectionEntities(IList<ProtectionEntity> protectionEntities)
        {
            foreach (ProtectionEntity pe in protectionEntities)
            {
                this.WriteProtectionEntity(pe);
            }
        }

        /// <summary>
        /// Writes Protection Entity.
        /// </summary>
        /// <param name="pe">Protection Entity</param>
        private void WriteProtectionEntity(ProtectionEntity pe)
        {
            this.WriteObject(
                new ASRProtectionEntity(
                    pe.ID,
                    pe.ServerId,
                    pe.ProtectionContainerId,
                    pe.Name,
                    pe.Type,
                    pe.FabricObjectId,
                    pe.Protected,
                    pe.CanCommit,
                    pe.CanFailover,
                    pe.CanReverseReplicate,
                    pe.ActiveLocation,
                    pe.ProtectionStateDescription,
                    pe.TestFailoverStateDescription,
                    pe.ReplicationHealth,
                    pe.ReplicationProvider),
                true);
        }
    }
}