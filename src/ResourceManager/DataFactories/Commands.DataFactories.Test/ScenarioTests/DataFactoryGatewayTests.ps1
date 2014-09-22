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
Nagative test. Get gateway from an non-existing empty group.
#>
function Test-GetNonExistingDataFactoryGateway
{	
    $dfname = Get-DataFactoryName
    $rgname = Get-ResourceGroupName
    $rglocation = Get-ProviderLocation ResourceManagement
    
    New-AzureResourceGroup -Name $rgname -Location $rglocation -Force
	New-AzureDataFactory -Name $dfname -Location $rglocation -ResourceGroup $rgname  -Force
    
    # Test
    Assert-ThrowsContains { Get-AzureDataFactoryGateway -ResourceGroupName $rgname -DataFactoryName $dfname -Name "gwname"  } "GatewayNotFound"    
}

<#
.SYNOPSIS
Create a gateway and then do a Get to compare the result are identical.
Delete the created gateway after test finishes.
#>
function Test-DataFactoryGateway
{
    $dfname = Get-DataFactoryName
    $rgname = Get-ResourceGroupName
    $rglocation = Get-ProviderLocation ResourceManagement
    $dflocation = Get-ProviderLocation DataFactoryManagement
        
    New-AzureResourceGroup -Name $rgname -Location $rglocation -Force

    try
    {
        New-AzureDataFactory -ResourceGroupName $rgname -Name $dfname -Location $dflocation -Force
     
        $gwname = "foo"
   
        $actual = New-AzureDataFactoryGateway -ResourceGroupName $rgname -DataFactoryName $dfname -Name $gwname -Location $dflocation
        $expected = Get-AzureDataFactoryGateway -ResourceGroupName $rgname -DataFactoryName $dfname -Name $gwname

        Assert-AreEqual $expected.Name $expected.Name
        Assert-AreEqual $expected.Location $expected.Location

        Remove-AzureDataFactoryGateway -ResourceGroupName $rgname -DataFactoryName $dfname -Name $gwname -Force
    }
    finally
    {
        Clean-DataFactory $rgname $dfname
    }
}