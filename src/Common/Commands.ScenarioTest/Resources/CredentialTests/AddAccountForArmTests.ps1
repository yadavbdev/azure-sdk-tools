# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

### Add-AzureAccount Scenario Tests for CSM ####

<#
.SYNOPSIS
Tests that single user account can be used to log in and list resource groups
#>
function Test-AddOrgIdWithSingleSubscription
{
	# Verify that account can be added and used to access the
	# expected subscription
	$accountInfo = Get-UserCredentials "OrgIdOneTenantOneSubscription"
	Add-AzureAccount -Credential $accountInfo.Credential -Environment $accountInfo.Environment

	# Is there one subscription added?
	Assert-True { (Get-AzureSubscription).Length -eq 1 }

	# does it have the right subscription id?
	Assert-True { (Get-AzureSubscription)[0].SubscriptionId -eq $accountInfo.ExpectedSubscription }

	# Can we use it to do something? If this passes then we're ok
	Get-AzureResourceGroup
}
