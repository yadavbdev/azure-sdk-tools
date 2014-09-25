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
Tests getting Active Directory groups.
#>
function Test-GetADGroup
{
    # Test
    # Select at most 10 groups. Groups are restricted to contain "test" to fasten the test
    $groups = Get-AzureADGroup -SearchString "test" | Select-Object -Last 10 -Wait

    if ($groups.Count -eq 0) {
        $groups =  Get-AzureADGroup | Select-Object -Last 10 -Wait
    }

    # Assert
    if ($groups.Count -gt 0) {
        foreach($group in $groups) {
            Assert-NotNull($group.DisplayName)
            Assert-NotNull($group.Id)
        }
    }
}

<#
.SYNOPSIS
Tests getting members from an Active Directory group.
#>
function Test-GetADGroupMember
{
    # Setup
    # Select at most 10 groups. Groups are restricted to contain "test" to fasten the test
    $groups = Get-AzureADGroup -SearchString "test" | Select-Object Id -Last 10 -Wait

    if ($groups.Count -eq 0) {
        $groups =  Get-AzureADGroup | Select-Object Id -Last 10 -Wait
    }

    # Test
    $members = $groups | ForEach-Object {Get-AzureADGroupMember -GroupObjectId $_.Id} | Select-Object -Last 10 -Wait
    

    # Verify 
    if ($members.Count -gt 0) {
        foreach($member in $members) {
            Assert-True {$member.Type -eq "User" -or $member.Type -eq "Group" -or $member.Type -eq "ServicePrincipal"}
        }
    }
}

<#
.SYNOPSIS
Tests getting Active Directory service principals.
#>
function Test-GetADServicePrincipal
{
    # Test
    # Select at most 10 service principals. Service principals are restricted to contain "test" to fasten the test
    $servicePrincipals = Get-AzureADServicePrincipal -SearchString "test" | Select-Object -Last 10 -Wait

    if ($servicePrincipals.Count -eq 0) {
        $servicePrincipals =  Get-AzureADServicePrincipal | Select-Object -Last 10 -Wait
    }

    # Assert
    if ($servicePrincipals.Count -gt 0) {
        foreach($servicePrincipal in $servicePrincipals) {
            Assert-NotNull($servicePrincipal.DisplayName)
            Assert-NotNull($servicePrincipal.Id)
            Assert-NotNull($servicePrincipal.ServicePrincipalName)
        }
    }
}

<#
.SYNOPSIS
Tests getting Active Directory users.
#>
function Test-GetADUser
{
    # Test
    # Select at most 10 users. Users are restricted to contain "test" to fasten the test
    $users = Get-AzureADUser -SearchString "test" | Select-Object -Last 10 -Wait

    if ($users.Count -eq 0) {
        $users =  Get-AzureADGroup | Select-Object -Last 10 -Wait
    }

    # Assert
    if ($users.Count -gt 0) {
        foreach($user in $users) {
            Assert-NotNull($user.DisplayName)
            Assert-NotNull($user.Id)
            Assert-NotNull($user.UserPrincipalName)
        }
    }
}
