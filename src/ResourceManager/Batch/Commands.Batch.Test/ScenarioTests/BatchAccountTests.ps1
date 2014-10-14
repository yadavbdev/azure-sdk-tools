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
Tests querying for a Batch account that does not exist throws
#>
function Test-GetNonExistingBatchAccount
{
    Assert-Throws { Get-AzureBatchAccount -Name "accountthatdoesnotexist" }
}

<#
.SYNOPSIS
Tests creating new Batch account.
#>
function Test-CreatesNewBatchAccount
{
    # Setup
	$account = Get-BatchAccountName
    $resourceGroup = Get-ResourceGroupName
    $location = Get-ProviderLocation ResourceManagement

    try 
    {
        New-AzureResourceGroup -Name $resourceGroup -Location $location

        # Test
        $actual = New-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup -Location $location -Tag @{Name = "testtag"; Value = "testval"} 
        $expected = Get-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup

        # Assert
		Assert-AreEqual $expected.AccountName $actual.AccountName
        Assert-AreEqual $expected.ResourceGroupName $actual.ResourceGroupName	
		Assert-AreEqual $expected.Location $actual.Location
        Assert-AreEqual $expected.Tags[0]["Name"] $actual.Tags[0]["Name"]
    }
    finally
    {
        # Cleanup
        Clean-BatchAccountAndResourceGroup $account $resourceGroup
    }
}

<#
.SYNOPSIS
Tests creating an account that already exists throws
#>
function Test-CreateExistingBatchAccount
{
    # Setup
	$account = Get-BatchAccountName
    $resourceGroup = Get-ResourceGroupName
    $location = Get-ProviderLocation ResourceManagement

    try 
    {
        New-AzureResourceGroup -Name $resourceGroup -Location $location

        # Test
        New-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup -Location $location -Tag @{Name = "testtag"; Value = "testval"} 
        Assert-Throws { New-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup -Location $location }
    }
    finally
    {
        # Cleanup
        Clean-BatchAccountAndResourceGroup $account $resourceGroup
    }
}

<#
.SYNOPSIS
Tests updating existing Batch account
#>
function Test-UpdatesExistingBatchAccount
{
    # Setup
	$account = Get-BatchAccountName
    $resourceGroup = Get-ResourceGroupName
    $location = Get-ProviderLocation ResourceManagement

    try 
    {
        New-AzureResourceGroup -Name $resourceGroup -Location $location

		#Test
        $new = New-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup -Location $location 
		Assert-AreEqual 0 $new.Tags.Count
		
		# TODO: Investigate need for delay. Sometimes get Resource Not Found error if Set cmdlet is called too soon, usually within a few seconds (30 is overkill to play it safe)
		if ([Microsoft.Azure.Utilities.HttpRecorder.HttpMockServer]::Mode -ne [Microsoft.Azure.Utilities.HttpRecorder.HttpRecorderMode]::Playback) 
		{
			Start-Sleep -s 30
		}

		# Append Tag
        $actual = Set-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup -Tag @{Name = "testtag"; Value = "testval"} 
        $expected = Get-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup

        # Assert
		Assert-AreEqual $expected.AccountName $actual.AccountName
        Assert-AreEqual $expected.ResourceGroupName $actual.ResourceGroupName	
		Assert-AreEqual $expected.Location $actual.Location
		Assert-AreEqual 1 $expected.Tags.Count
        Assert-AreEqual $expected.Tags[0]["Name"] $actual.Tags[0]["Name"]
    }
    finally
    {
        # Cleanup
        Clean-BatchAccountAndResourceGroup $account $resourceGroup
    }
}

<#
.SYNOPSIS
Tests creating a new Batch account and deleting it via piping.
#>
function Test-CreateAndRemoveBatchAccountViaPiping
{
    # Setup
    $account1 = Get-BatchAccountName
	$account2 = Get-BatchAccountName
    $resourceGroup = Get-ResourceGroupName
    $location = Get-ProviderLocation ResourceManagement

	try
	{
		New-AzureResourceGroup -Name $resourceGroup -Location $location

		# Test
		New-AzureBatchAccount -Name $account1 -ResourceGroupName $resourceGroup -Location $location
		New-AzureBatchAccount -Name $account2 -ResourceGroupName $resourceGroup -Location $location
		Get-AzureBatchAccount | where {$_.AccountName -eq $account1 -or $_.AccountName -eq $account2} | Remove-AzureBatchAccount -Force

		# Assert
		Assert-Throws { Get-AzureBatchAccount -Name $account1 } 
		Assert-Throws { Get-AzureBatchAccount -Name $account2 } 
	}
	finally
	{
		Clean-ResourceGroup $resourceGroup
	}
}

<#
.SYNOPSIS
Tests getting/setting Batch account keys
#>
function Test-BatchAccountKeys
{
    # Setup
	$account = Get-BatchAccountName
    $resourceGroup = Get-ResourceGroupName
    $location = Get-ProviderLocation ResourceManagement

    try 
    {
        New-AzureResourceGroup -Name $resourceGroup -Location $location

        # Test
        $new = New-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup -Location $location -Tag @{Name = "testtag"; Value = "testval"} 
		$actual = New-AzureBatchAccountKey -Name $account -ResourceGroupName $resourceGroup -KeyType Primary
        $expected = Get-AzureBatchAccountKeys -Name $account -ResourceGroupName $resourceGroup
		$getAccountResult = Get-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup

        # Assert
		Assert-AreEqual $null $new.PrimaryAccountKey
        Assert-AreEqual $null $new.SecondaryAccountKey
		Assert-AreEqual $actual.PrimaryAccountKey $expected.PrimaryAccountKey
		Assert-AreEqual $actual.SecondaryAccountKey $expected.SecondaryAccountKey
		Assert-AreEqual $null $getAccountResult.PrimaryAccountKey
		Assert-AreEqual $null $getAccountResult.SecondaryAccountKey
    }
    finally
    {
        # Cleanup
        Clean-BatchAccountAndResourceGroup $account $resourceGroup
    }
}
