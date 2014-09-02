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
Tests that when setting the storage account property's value in a database's auditing policy, that value is later fetched properly
#>
function Test-DatabaseUpdatePolicyWithStorage
{
	# Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters

	try 
	{
		# Test
		Set-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy -StorageAccountName $params.storageAccount
		$policy = Get-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy
	
		# Assert
		Assert-AreEqual $policy.StorageAccountName $params.storageAccount
		Assert-True { $policy.IsEnabled } 
		Assert-False { $policy.UseServerDefault }
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}

<#
.SYNOPSIS
Tests that when setting the storage account property's value in a server's auditing policy, that value is later fetched properly
#>
function Test-ServerUpdatePolicyWithStorage
{
	# Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters

	try
	{
		# Test
		Set-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -StorageAccountName $params.storageAccount
		$policy = Get-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy
	
		# Assert
		Assert-AreEqual $policy.StorageAccountName $params.storageAccount
		Assert-True { $policy.IsEnabled } 
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}

<#
.SYNOPSIS
Tests that when modifying the eventType property of a databases's auditing policy (including the All and None values), these properties are later fetched properly
#>
function Test-DatabaseUpdatePolicyWithEventTypes
{
	# Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters

	try
	{
		# Test
		Set-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy -StorageAccountName $params.storageAccount -EventType "All"
		$policy = Get-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy
	
		# Assert
		Assert-AreEqual $policy.EventType.Length 5

		# Test
		Set-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy -StorageAccountName $params.storageAccount -EventType "DataAccess","DataChanges","RevokePermissions"
		$policy = Get-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy
	
		# Assert
		Assert-AreEqual $policy.EventType.Length 3
		Assert-True {$policy.EventType.Contains("DataAccess")}
		Assert-True {$policy.EventType.Contains("DataChanges")}
		Assert-True {$policy.EventType.Contains("RevokePermissions")}

		# Test
		Set-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy -StorageAccountName $params.storageAccount -EventType "None"
		$policy = Get-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy
	
		# Assert
		Assert-AreEqual $policy.EventType.Length 0 
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}

<#
.SYNOPSIS
Tests that when modifying the eventType property of a server's auditing policy (including the All and None values), these properties are later fetched properly
#>
function Test-ServerUpdatePolicyWithEventTypes
{
	# Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters

	try
	{
		# Test
		Set-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -StorageAccountName $params.storageAccount -EventType "All"
		$policy = Get-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy 
	
		# Assert
		Assert-AreEqual $policy.EventType.Length 5

		# Test
		Set-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -StorageAccountName $params.storageAccount -EventType "DataAccess","DataChanges","RevokePermissions"
		$policy = Get-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy
	
		# Assert
		Assert-AreEqual $policy.EventType.Length 3
		Assert-True {$policy.EventType.Contains("DataAccess")}
		Assert-True {$policy.EventType.Contains("DataChanges")}
		Assert-True {$policy.EventType.Contains("RevokePermissions")}

		# Test
		Set-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -StorageAccountName $params.storageAccount -EventType "None"
		$policy = Get-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy
	
		# Assert
		Assert-AreEqual $policy.EventType.Length 0 
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}

<#
.SYNOPSIS
Tests that when asking to disable auditing of a database, later when fetching the policy, it is marked as disabled
#>
function Test-DisableDatabaseAuditing
{
	# Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters

	try
	{
		# Test
		Set-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy -StorageAccountName $params.storageAccount
		Disable-AzureSqlDatabaseAuditing -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy
		$policy = Get-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy
	
		# Assert
		Assert-False { $policy.IsEnabled }
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}

<#
.SYNOPSIS
Tests that when asking to disable auditing of a server, later when fetching the policy, it is marked as disabled
#>
function Test-DisableServerAuditing
{
	# Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters

	try
	{
		# Test
		Set-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -StorageAccountName $params.storageAccount
		Disable-AzureSqlServerAuditing -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy
		$policy = Get-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy
	
		# Assert
		Assert-False { $policy.IsEnabled }
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}

<#
.SYNOPSIS
Tests that when disabling an already existing auditing policy on a database and then re-enabling it, the properties of the policy are kept
#>
function Test-DatabaseDisableEnableKeepProperties
{
	# Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters

	try
	{
		# Test
		Set-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy -StorageAccountName $params.storageAccount -EventType "SecurityExceptions"
		Disable-AzureSqlDatabaseAuditing -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy
		Set-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy
		$policy = Get-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy
	
		# Assert
		Assert-AreEqual $policy.StorageAccountName $params.storageAccount
		Assert-True { $policy.IsEnabled } 
		Assert-False { $policy.UseServerDefault }
		Assert-AreEqual $policy.EventType.Length 1
		Assert-True {$policy.EventType.Contains("SecurityExceptions")}
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}

<#
.SYNOPSIS
Tests that when disabling an already existing auditing policy on a server and then re-enabling it, the properties of the policy are kept
#>
function Test-ServerDisableEnableKeepProperties
{
	# Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters
	
	try
	{
		# Test
		Set-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -StorageAccountName $params.storageAccount -EventType "RevokePermissions"
		Disable-AzureSqlServerAuditing -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy
		Set-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy
		$policy = Get-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy
	
		# Assert
		Assert-AreEqual $policy.StorageAccountName $params.storageAccount
		Assert-True { $policy.IsEnabled } 
		Assert-False { $policy.UseServerDefault }
		Assert-AreEqual $policy.EventType.Length 1
		Assert-True {$policy.EventType.Contains("RevokePermissions")}
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}

<#
.SYNOPSIS
Tests that after marking a database as using its server's policy, when fetching the database's policy, it is marked as using the server's policy  
#>
function Test-UseServerDefault
{
    # Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters

	try
	{
		# Test
		Set-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -StorageAccountName $params.storageAccount
		Use-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy
		$policy = Get-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithPolicy -DatabaseName $params.databaseWithPolicy

		# Assert
		Assert-True {$policy.UseServerDefault}
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}

<#
.SYNOPSIS
Tests that a failure occurs when trying to set a policy to a database, and that database does not have a polic as well as the policy does not have a storage account  
#>
function Test-FailedDatabaseUpdatePolicyWithNoStorage
{
	# Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters

	try
	{
		# Assert
		Assert-Throws { Set-AzureSqlDatabaseAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithoutPolicy -DatabaseName $params.databaseWithoutPolicy }
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}

<#
.SYNOPSIS
Tests that a failure occurs when trying to set a policy to a server, and that policy does not have a storage account  
#>
function Test-FailedServerUpdatePolicyWithNoStorage
{
	# Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters

	try
	{
		# Assert
		Assert-Throws { Set-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithoutPolicy}
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}

<#
.SYNOPSIS
Tests that a failure occurs when trying to make a database use its server's auditing policy when the server's policy does not have a storage account  
#>
function Test-FailedUseServerDefault
{
	# Setup
	Create-TestEnvironment
	$params = Get-SqlAuditingTestEnvironmentParameters

	try
	{
		# Assert
		Assert-Throws { Use-AzureSqlServerAuditingSetting -ResourceGroupName $params.rgname -ServerName $params.serverWithoutPolicy -DatabaseName $params.databaseWithoutPolicy }
	}
	finally
	{
		# Cleanup
		Remove-TestEnvironment
	}
}