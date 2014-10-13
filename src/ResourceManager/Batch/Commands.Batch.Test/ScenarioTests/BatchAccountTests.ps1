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
	$account = Get-BatchAccountName "new"
    $resourceGroup = Get-DefaultResourceGroup
    $location = Get-DefaultLocation

    try 
    {
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
        Clean-BatchAccount $account $resourceGroup
    }
}

<#
.SYNOPSIS
Tests creating an account that already exists throws
#>
function Test-CreateExistingBatchAccount
{
    # Setup
	$account = Get-BatchAccountName "createexisting"
    $resourceGroup = Get-DefaultResourceGroup
    $location = Get-DefaultLocation

    try 
    {
        # Test
        New-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup -Location $location -Tag @{Name = "testtag"; Value = "testval"} 
        Assert-Throws { New-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup -Location $location }
    }
    finally
    {
        # Cleanup
        Clean-BatchAccount $account $resourceGroup
    }
}

<#
.SYNOPSIS
Tests updating existing Batch account
#>
function Test-UpdatesExistingBatchAccount
{
    # Setup
	$account = Get-BatchAccountName "update"
    $resourceGroup = Get-DefaultResourceGroup
    $location = Get-DefaultLocation

    try 
    {
        $new = New-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup -Location $location 
        
		# Append Tag
        $actual = Set-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup -Tag @{Name = "testtag"; Value = "testval"} 
        $expected = Get-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup

        # Assert
		Assert-AreEqual $expected.AccountName $actual.AccountName
        Assert-AreEqual $expected.ResourceGroupName $actual.ResourceGroupName	
		Assert-AreEqual $expected.Location $actual.Location
		Assert-AreEqual 0 $new.Tags.Count
		Assert-AreEqual 1 $expected.Tags.Count
        Assert-AreEqual $expected.Tags[0]["Name"] $actual.Tags[0]["Name"]
    }
    finally
    {
        # Cleanup
        Clean-BatchAccount $account $resourceGroup
    }
}

<#
.SYNOPSIS
Tests getting/setting Batch account keys
#>
function Test-BatchAccountKeys
{
    # Setup
	$account = Get-BatchAccountName "keys"
    $resourceGroup = Get-DefaultResourceGroup
    $location = Get-DefaultLocation

    try 
    {
        # Test
        $new = New-AzureBatchAccount -Name $account -ResourceGroupName $resourceGroup -Location $location -Tag @{Name = "testtag"; Value = "testval"} 
		$actual = New-AzureBatchAccountKey -Name $account -ResourceGroupName $resourceGroup -KeyType Primary
        $expected = Get-AzureBatchAccountKeys -Name $account -ResourceGroupName $resourceGroup

        # Assert
		Assert-AreEqual $null $new.PrimaryAccountKey
        Assert-AreEqual $null $new.SecondaryAccountKey
		Assert-AreEqual $actual.PrimaryAccountKey $expected.PrimaryAccountKey
		Assert-AreEqual $actual.SecondaryAccountKey $expected.SecondaryAccountKey
    }
    finally
    {
        # Cleanup
        Clean-BatchAccount $account $resourceGroup
    }
}
