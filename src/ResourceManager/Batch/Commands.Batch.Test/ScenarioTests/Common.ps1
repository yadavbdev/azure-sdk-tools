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
Gets a Batch account name for testing. Add the test name to the end to make the account name unique and support parallelization.
#>
function Get-BatchAccountName($test)
{
    return "accnttest" + $test
}

<#
.SYNOPSIS
Gets default resource group name for testing
#>
function Get-DefaultResourceGroup
{
    return "default-azurebatch-westus"
}

<#
.SYNOPSIS
Gets default location for testing
#>
function Get-DefaultLocation
{
    return "westus"
}

<#
.SYNOPSIS
Cleans the created Batch account
#>
function Clean-BatchAccount($accountName,$resourceGroup)
{
    if ([Microsoft.Azure.Utilities.HttpRecorder.HttpMockServer]::Mode -ne [Microsoft.Azure.Utilities.HttpRecorder.HttpRecorderMode]::Playback) 
	{
        Remove-AzureBatchAccount -Name $accountName -ResourceGroupName $resourceGroup -Force
    }
}