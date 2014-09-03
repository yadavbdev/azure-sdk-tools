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
Gets the values of the parameters used at the auditing tests
#>
function Get-SqlAuditingTestEnvironmentParameters 
{
	return @{ rgname = "sql-audit-cmdlet-test-rg";
			  serverWithPolicy = "audit-server-with-policy";
			  serverWithoutPolicy = "audit-server-without-policy";
			  databaseWithPolicy = "audit-database-with-policy";
			  databaseWithoutPolicy = "audit-database-without-policy";
			  storageAccount = "audittestscmdletsstorage"}
}

<#
.SYNOPSIS
Creates the test environment needed to perform the Sql auditing tests
#>
function Create-TestEnvironment 
{
	Switch-AzureMode AzureResourceManager
	$params = Get-SqlAuditingTestEnvironmentParameters
	$storageResource = Get-AzureResource -ErrorAction SilentlyContinue | where {$_.Name -eq $params.storageAccount}
	if($storageResource -ne $null)
	{
		Remove-AzureStorageAccount -StorageAccountName $params.storageAccount
	}
	Switch-AzureMode AzureServiceManagement
	New-AzureStorageAccount -StorageAccountName $params.storageAccount -Location "West US" 
	Switch-AzureMode AzureResourceManager
	New-AzureResourceGroup -Name $params.rgname -Location "West US" -TemplateFile ".\Templates\cmdlet-test-env-setup.json" -server1Name $params.serverWithPolicy -server2Name $params.serverWithoutPolicy -database1Name $params.databaseWithPolicy -database2Name $params.databaseWithoutPolicy -EnvLocation "West US" -StorageAccountName $params.storageAccount -Force
}

<#
.SYNOPSIS
Removes the test environment that was needed to perform the Sql auditing tests
#>
function Remove-TestEnvironment 
{
	Switch-AzureMode AzureResourceManager
	$params = Get-SqlAuditingTestEnvironmentParameters
	Switch-AzureMode AzureServiceManagement
	Remove-AzureStorageAccount -StorageAccountName $params.storageAccount
	Switch-AzureMode AzureResourceManager
}