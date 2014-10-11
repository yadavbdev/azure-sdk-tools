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
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Threading;
    using Microsoft.Azure.Commands.RecoveryServices.SiteRecovery;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Management.SiteRecovery.Models;
    #endregion

    /// <summary>
    /// Used to initiate a commit operation.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "AzureSiteRecoveryUnplannedFailoverJob", DefaultParameterSetName = ASRParameterSets.ByRPId)]
    [OutputType(typeof(ASRJob))]
    public class StartAzureSiteRecoveryUnplannedFailoverJob : RecoveryServicesCmdletBase
    {
        #region Parameters
        /// <summary>
        /// ID of the RP object to start failover on.
        /// </summary>
        private string recoveryPlanId;

        /// <summary>
        /// ID of the PE object to start failover on.
        /// </summary>
        private string protectionEntityId;

        /// <summary>
        /// Protection container ID of the object to start failover on.
        /// </summary>
        private string protectionContainerId;
        
        /// <summary>
        /// Recovery Plan object.
        /// </summary>
        private ASRProtectionEntity protectionEntity;

        /// <summary>
        /// Recovery Plan object.
        /// </summary>
        private ASRRecoveryPlan recoveryPlan;

        /// <summary>
        /// Failover direction for the recovery plan.
        /// </summary>
        private string direction;

        /// <summary>
        /// Indicates whether primary site actions are required or not.
        /// </summary>
        private bool primaryAction;

        /// <summary>
        /// Indicates whether primary site actions are required or not.
        /// </summary>
        private bool sourceSiteOperations;

        /// <summary>
        /// This is required to wait for job completion.
        /// </summary>
        private bool waitForCompletion;

        /// <summary>
        /// Job response.
        /// </summary>
        private JobResponse jobResponse = null;

        /// <summary>
        /// Gets or sets ID of the Recovery Plan.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByRPId, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string RPId
        {
            get { return this.recoveryPlanId; }
            set { this.recoveryPlanId = value; }
        }

        /// <summary>
        /// Gets or sets ID of the PE.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByPEId, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string ProtectionEntityId
        {
            get { return this.protectionEntityId; }
            set { this.protectionEntityId = value; }
        }

        /// <summary>
        /// Gets or sets ID of the Recovery Plan.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByPEId, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string ProtectionContainerId
        {
            get { return this.protectionContainerId; }
            set { this.protectionContainerId = value; }
        }

        /// <summary>
        /// Gets or sets Recovery Plan object.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByRPObject, Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRRecoveryPlan RecoveryPlan
        {
            get { return this.recoveryPlan; }
            set { this.recoveryPlan = value; }
        }

        /// <summary>
        /// Gets or sets Protection Entity object.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByPEObject, Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRProtectionEntity ProtectionEntity
        {
            get { return this.protectionEntity; }
            set { this.protectionEntity = value; }
        }

        /// <summary>
        /// Gets or sets Failover direction for the recovery plan.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByRPObject, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.ByRPId, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.ByPEId, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.ByPEObject, Mandatory = true)]
        [ValidateSet(
            PSRecoveryServicesClient.PrimaryToRecovery,
            PSRecoveryServicesClient.RecoveryToPrimary)]
        public string Direction
        {
            get { return this.direction; }
            set { this.direction = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether primary site actions are required or not.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByRPObject, Mandatory = true)]
        [Parameter(ParameterSetName = ASRParameterSets.ByRPId, Mandatory = true)]
        public bool PrimaryAction
        {
            get { return this.primaryAction; }
            set { this.primaryAction = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether can do source site operations.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByPEId, Mandatory = false)]
        [Parameter(ParameterSetName = ASRParameterSets.ByPEObject, Mandatory = false)]
        public bool PerformSourceSiteOperations
        {
            get { return this.sourceSiteOperations; }
            set { this.sourceSiteOperations = value; }
        }

        /// <summary>
        /// Gets or sets switch parameter. This is required to wait for job completion.
        /// </summary>
        [Parameter]
        public SwitchParameter WaitForCompletion
        {
            get { return this.waitForCompletion; }
            set { this.waitForCompletion = value; }
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
                    case ASRParameterSets.ByRPObject:
                        this.recoveryPlanId = this.recoveryPlan.ID;
                        this.StartRpUnPlannedFailover();
                        break;
                    case ASRParameterSets.ByPEObject:
                        this.protectionEntityId = this.ProtectionEntity.ID;
                        this.protectionContainerId = this.ProtectionEntity.ProtectionContainerId;
                        this.StartPEUnplannedFailover();
                        break;
                    case ASRParameterSets.ByPEId:
                        this.StartPEUnplannedFailover();
                        break;
                    case ASRParameterSets.ByRPId:
                        this.StartRpUnPlannedFailover();
                        break;
                }
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
            }
        }

        /// <summary>
        /// Handles interrupts.
        /// </summary>
        protected override void StopProcessing()
        {
            // Ctrl + C and etc
            base.StopProcessing();
            this.StopProcessingFlag = true;
        }

        /// <summary>
        /// Starts PE Unplanned failover.
        /// </summary>
        private void StartPEUnplannedFailover()
        {
            var ufoReqeust = new UnplannedFailoverRequest();
            ufoReqeust.FailoverDirection = this.Direction;
            ufoReqeust.SourceSiteOperations = this.PerformSourceSiteOperations;
            this.jobResponse =
                RecoveryServicesClient.StartAzureSiteRecoveryUnplannedFailover(
                this.protectionContainerId,
                this.ProtectionEntityId,
                ufoReqeust);
            this.WriteJob(this.jobResponse.Job);

            if (this.waitForCompletion)
            {
                this.WaitForJobCompletion(this.jobResponse.Job.ID);
            }
        }

        /// <summary>
        /// Starts RP Planned failover.
        /// </summary>
        private void StartRpUnPlannedFailover()
        {
            RpUnplannedFailoverRequest recoveryPlanUnPlannedFailoverRequest = new RpUnplannedFailoverRequest();
            recoveryPlanUnPlannedFailoverRequest.FailoverDirection = this.Direction;
            recoveryPlanUnPlannedFailoverRequest.PrimaryAction = this.PrimaryAction;
            this.jobResponse = RecoveryServicesClient.StartAzureSiteRecoveryUnplannedFailover(
                this.recoveryPlanId, 
                recoveryPlanUnPlannedFailoverRequest);

            this.WriteJob(this.jobResponse.Job);

            if (this.waitForCompletion)
            {
                this.WaitForJobCompletion(this.jobResponse.Job.ID);
            }
        }

        /// <summary>
        /// Writes Job.
        /// </summary>
        /// <param name="job">JOB object</param>
        private void WriteJob(Microsoft.WindowsAzure.Management.SiteRecovery.Models.Job job)
        {
            this.WriteObject(new ASRJob(job));
        }
    }
}