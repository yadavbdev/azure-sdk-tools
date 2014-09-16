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

<#
.SYNOPSIS
Tests verifies negative scenarios for RoleAssignments
#>
function Test-RaNegativeScenarios
{
	$subscription = Get-AzureSubscription -Current

	# Bad OID does not throw when getting a non-existing role assignment
	$badOid = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
	$badOidResult = Get-AzureRoleAssignment -ObjectId $badOid
	Assert-Null $badOidResult

	# Bad UPN
	$badUpn = 'nonexistent@provider.com'
	$badUpnException = "The provided information does not map to an AD object id."
	Assert-Throws { Get-AzureRoleAssignment -UserPrincipalName $badUpn } $badUpnException
	
	# Bad SPN
	$badSpn = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'
	$badSpnException = "The provided information does not map to an AD object id."
	Assert-Throws { Get-AzureRoleAssignment -ServicePrincipalName $badSpn } $badSpnException
	
	# Bad Scope
	$badScope = '/subscriptions/'+ $subscription.SubscriptionId +'/providers/nonexistent'
	$badScopeException = "InvalidResourceNamespace: The resource namespace 'nonexistent' is invalid."
	Assert-Throws { Get-AzureRoleAssignment -Scope $badScope } $badScopeException
}

<#
.SYNOPSIS
Tests verifies creation and deletion of a RoleAssignments by Scope
#>
function Test-RaByScope
{
	# Setup
	$definitionName = 'Reader'
	$users = Get-AzureADUser | Select-Object -Last 1 -Wait
	$subscription = Get-AzureSubscription -Current
	$scope = '/subscriptions/'+ $subscription.SubscriptionId +'/resourceGroups/' + 'nonexistentRG'
	Assert-AreEqual 1 $users.Count "There should be at least one user to run the test."
	
	# Test
	$newAssignment = New-AzureRoleAssignment `
						-ObjectId $users[0].Id.Guid `
						-RoleDefinitionName $definitionName `
						-Scope $scope 
	
	# cleanup 
	$newAssignment | Remove-AzureRoleAssignment -Force

	# Assert
	Assert-NotNull $newAssignment
	Assert-AreEqual	$definitionName $newAssignment.RoleDefinitionName 
	Assert-AreEqual	$scope $newAssignment.Scope 
	Assert-AreEqual	$users[0].DisplayName $newAssignment.DisplayName

	$deletedRoleAssignment = Get-AzureRoleAssignment -ObjectId $newAssignment.ObjectId.Guid
	Assert-Null $deletedRoleAssignment
}

<#
.SYNOPSIS
Tests verifies creation and deletion of a RoleAssignments by Resource Group
#>
function Test-RaByResourceGroup
{
	# Setup
	$definitionName = 'Contributor'
	$users = Get-AzureADUser | Select-Object -Last 1 -Wait
	$resourceGroups = Get-AzureResourceGroup | Select-Object -Last 1 -Wait
	Assert-AreEqual 1 $users.Count "There should be at least one user to run the test."
	Assert-AreEqual 1 $resourceGroups.Count "No resource group found. Unable to run the test."

	# Test
	$newAssignment = New-AzureRoleAssignment `
						-ObjectId $users[0].Id.Guid `
						-RoleDefinitionName $definitionName `
						-ResourceGroupName $resourceGroups[0].ResourceGroupName
	
	# cleanup 
	$newAssignment | Remove-AzureRoleAssignment -Force
	
	# Assert
	Assert-NotNull $newAssignment
	Assert-AreEqual	$definitionName $newAssignment.RoleDefinitionName 
	Assert-AreEqual	$users[0].DisplayName $newAssignment.DisplayName

	$deletedRoleAssignment = Get-AzureRoleAssignment -ObjectId $newAssignment.ObjectId.Guid
	Assert-Null $deletedRoleAssignment
}

<#
.SYNOPSIS
Tests verifies creation and deletion of a RoleAssignments by Resource 
#>
function Test-RaByResource
{
	# Setup
	$definitionName = 'Owner'
	$groups = Get-AzureADGroup | Select-Object -Last 1 -Wait
	$resourceGroups = Get-AzureResourceGroup | Select-Object -First 1 -Wait
	Assert-AreEqual 1 $groups.Count "There should be at least one group to run the test."
	Assert-AreEqual 1 $resourceGroups.Count "No resource group found. Unable to run the test."
	$resource = Get-AzureResource -ResourceGroupName $resourceGroups[0].ResourceGroupName `
								  | Select-Object -Last 1 -Wait
	Assert-NotNull $resource "Cannot find any resource to continue test execution."

	# Test
	$newAssignment = New-AzureRoleAssignment `
						-ObjectId $groups[0].Id.Guid `
						-RoleDefinitionName $definitionName `
						-ResourceGroupName $resourceGroups[0].ResourceGroupName `
						-ResourceType $resource.ResourceType `
						-ResourceName $resource.Name
	
	# cleanup 
	$newAssignment | Remove-AzureRoleAssignment -Force
	
	# Assert
	Assert-NotNull $newAssignment
	Assert-AreEqual	$definitionName $newAssignment.RoleDefinitionName 
	Assert-AreEqual	$groups[0].DisplayName $newAssignment.DisplayName
}

<#
.SYNOPSIS
Tests verifies creation and deletion of a RoleAssignments for Service principal name 
#>
function Test-RaByServicePrincipal
{
	# Setup
	$definitionName = 'Reader'
	$servicePrincipals = Get-AzureADServicePrincipal | Select-Object -Last 1 -Wait
	$subscription = Get-AzureSubscription -Current
	$scope = '/subscriptions/'+ $subscription.SubscriptionId +'/resourceGroups/' + 'nonexistentRGForSpn'
	Assert-AreEqual 1 $servicePrincipals.Count "No service principals found. Unable to run the test."

	# Test
	$newAssignment = New-AzureRoleAssignment `
						-ServicePrincipalName $servicePrincipals[0].DisplayName `
						-RoleDefinitionName $definitionName `
						-Scope $scope 
						
	
	# cleanup 
	$newAssignment | Remove-AzureRoleAssignment -Force
	
	# Assert
	Assert-NotNull $newAssignment
	Assert-AreEqual	$definitionName $newAssignment.RoleDefinitionName 
	Assert-AreEqual	$scope $newAssignment.Scope 
	Assert-AreEqual	$servicePrincipals[0].DisplayName $newAssignment.DisplayName

	$deletedRoleAssignment = Get-AzureRoleAssignment -ObjectId $newAssignment.ObjectId.Guid
	Assert-Null $deletedRoleAssignment
}

<#
.SYNOPSIS
Tests verifies creation and deletion of a RoleAssignments for User Principal Name
#>
function Test-RaByUpn
{
	# Setup
	$definitionName = 'Contributor'
	$users = Get-AzureADUser | Select-Object -Last 1 -Wait
	$resourceGroups = Get-AzureResourceGroup | Select-Object -Last 1 -Wait
	Assert-AreEqual 1 $users.Count "There should be at least one user to run the test."
	Assert-AreEqual 1 $resourceGroups.Count "No resource group found. Unable to run the test."

	# Test
	$newAssignment = New-AzureRoleAssignment `
						-UPN $users[0].Mail `
						-RoleDefinitionName $definitionName `
						-ResourceGroupName $resourceGroups[0].ResourceGroupName
	
	# cleanup 
	$newAssignment | Remove-AzureRoleAssignment -Force
	
	# Assert
	Assert-NotNull $newAssignment
	Assert-AreEqual	$definitionName $newAssignment.RoleDefinitionName 
	Assert-AreEqual	$users[0].DisplayName $newAssignment.DisplayName

	$deletedRoleAssignment = Get-AzureRoleAssignment -ObjectId $newAssignment.ObjectId.Guid
	Assert-Null $deletedRoleAssignment
}

<#
.SYNOPSIS
Tests validate correctness of returned permissions when logged in as the assigned user 
#>
function Test-RaUserPermissions
{
	param([string]$username, [string]$password, [string]$rgName, [string]$action)
	
	# Setup
	$securePassword = $password | ConvertTo-SecureString -AsPlainText -Force
	$credential = New-Object System.Management.Automation.PSCredential ($username, $securePassword)
	$currentSubscription = Get-AzureSubscription -Current
	
	# Test
	Add-AzureAccount -Credential $credential
	Select-AzureSubscription -Current -SubscriptionName $currentSubscription.SubscriptionName -Account $username
	$permissions = Get-AzureResourceGroup -Name $rgName
	
	# cleanup 
	# Switch account back
	Select-AzureSubscription -Current -SubscriptionName $currentSubscription.SubscriptionName -Account $currentSubscription.Account
	
	# Assert
	Assert-AreEqual 1 $permissions.Permissions.Count "User should have only one permission."
	Assert-AreEqual 1 $permissions.Permissions[0].Actions.Count "User should have only one action in the permission."
	Assert-AreEqual	$action $permissions.Permissions[0].Actions[0] "Permission action mismatch."
}