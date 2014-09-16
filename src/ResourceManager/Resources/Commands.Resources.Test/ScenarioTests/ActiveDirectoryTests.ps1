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
function Test-GetAllADGroups
{
    # Test
    $groups = Get-AzureADGroup

    # Assert
    Assert-NotNull($groups)
    foreach($group in $groups) {
        Assert-NotNull($group.DisplayName)
        Assert-NotNull($group.Id)
    }
}

<#
.SYNOPSIS
Tests getting Active Directory groups.
#>
function Test-GetADGroupWithSearchString
{
    # Test
    # Select at most 10 groups. Groups are restricted to contain "test" to fasten the test
    $groups = Get-AzureADGroup -SearchString "reader"

    # Assert
    Assert-NotNull($groups)
    foreach($group in $groups) {
        Assert-NotNull($group.DisplayName)
        Assert-NotNull($group.Id)
    }
}

<#
.SYNOPSIS
Tests getting Active Directory groups.
#>
function Test-GetADGroupWithBadSearchString
{
    # Test
    # Select at most 10 groups. Groups are restricted to contain "test" to fasten the test
    $groups = Get-AzureADGroup -SearchString "BadSearchString"

    # Assert
    Assert-Null($groups)
}

<#
.SYNOPSIS
Tests getting Active Directory groups.
#>
function Test-GetADGroupWithObjectId
{
    # Test
    $groups = Get-AzureADGroup -ObjectId "d5fbb343-cf1d-47bb-9aa8-5c3dd57b336f"

    # Assert
    Assert-AreEqual $groups.Count 1
    Assert-AreEqual $groups[0].Id "d5fbb343-cf1d-47bb-9aa8-5c3dd57b336f"
    Assert-NotNull($groups[0].DisplayName)
}

<#
.SYNOPSIS
Tests getting Active Directory groups.
#>
function Test-GetADGroupWithBadObjectId
{
    # Test
    $groups = Get-AzureADGroup -ObjectId "deadbeef-dead-beef-dead-beefdeadbeef"

    # Assert
    Assert-Null $groups
}

<#
.SYNOPSIS
Tests getting Active Directory groups.
#>
function Test-GetADGroupWithUserObjectId
{
    # Setup
    $user = Get-AzureADUser | Select-Object -First 1 -Wait

    # Test
    $groups = Get-AzureADGroup -ObjectId $user[0].Id

    # Assert
    Assert-Null $groups
}

<#
.SYNOPSIS
Tests getting members from an Active Directory group.
#>
function Test-GetADGroupMemberWithGroupObjectId
{
    # Test
    $members = Get-AzureADGroupMember -GroupObjectId "fb7d9586-9377-43c8-95c0-22f1f067915f"
    
    # Assert 
    foreach($member in $members) {
        Assert-NotNull($member.DisplayName)
        Assert-NotNull($member.Id)
        Assert-True {$member.Type -eq "User" -or $member.Type -eq "Group" -or $member.Type -eq "ServicePrincipal"}
    }
}

<#
.SYNOPSIS
Tests getting members from an Active Directory group.
#>
function Test-GetADGroupMemberWithBadGroupObjectId
{
    # Test
    $members = Get-AzureADGroupMember -GroupObjectId "deadbeef-dead-beef-dead-beefdeadbeef"
    
    # Assert 
    Assert-Null($members)
}

<#
.SYNOPSIS
Tests getting members from an Active Directory group.
#>
function Test-GetADGroupMemberWithUserObjectId
{
    # Test
    $members = Get-AzureADGroupMember -GroupObjectId "7b45838f-42c3-4fef-a85a-0a9051dfda41"
    
    # Assert 
    Assert-Null($members)
}

<#
.SYNOPSIS
Tests getting members from an Active Directory group.
#>
function Test-GetADGroupMemberFromEmptyGroup
{
    # Test
    $members = Get-AzureADGroupMember -GroupObjectId "8fd46a09-454e-41f1-b70f-f28331b12a31"
    
    # Assert 
    Assert-Null($members)
}

<#
.SYNOPSIS
Tests getting Active Directory service principals.
#>
function Test-GetADServicePrincipalWithObjectId
{
    # Test
    $servicePrincipals = Get-AzureADServicePrincipal -ObjectId "e68deb93-98e3-476f-8667-1bb60a7f867b"

    # Assert
    Assert-AreEqual $servicePrincipals.Count 1
    Assert-AreEqual $servicePrincipals[0].Id "e68deb93-98e3-476f-8667-1bb60a7f867b"
}

<#
.SYNOPSIS
Tests getting Active Directory service principals.
#>
function Test-GetADServicePrincipalWithBadObjectId
{
    # Test
    $servicePrincipals = Get-AzureADServicePrincipal -ObjectId "deadbeef-dead-beef-dead-beefdeadbeef"

    # Assert
    Assert-Null($servicePrincipals)
}

<#
.SYNOPSIS
Tests getting Active Directory service principals.
#>
function Test-GetADServicePrincipalWithUserObjectId
{
    # Test
    $servicePrincipals = Get-AzureADServicePrincipal -ObjectId "7b45838f-42c3-4fef-a85a-0a9051dfda41"

    # Assert
    Assert-Null($servicePrincipals)
}

<#
.SYNOPSIS
Tests getting Active Directory service principals.
#>
function Test-GetADServicePrincipalWithSPN
{
    # Test
    $servicePrincipals = Get-AzureADServicePrincipal -ServicePrincipalName "https://localhost:8080"

    # Assert
    Assert-AreEqual $servicePrincipals.Count 1
    Assert-AreEqual $servicePrincipals[0].ServicePrincipalName "https://localhost:8080"
}

<#
.SYNOPSIS
Tests getting Active Directory service principals.
#>
function Test-GetADServicePrincipalWithBadSPN
{
    # Test
    $servicePrincipals = Get-AzureADServicePrincipal -ServicePrincipalName "badspn"

    # Assert
    Assert-Null($servicePrincipals)
}

<#
.SYNOPSIS
Tests getting Active Directory service principals.
#>
function Test-GetADServicePrincipalWithSearchString
{
    # Test
    $servicePrincipals = Get-AzureADServicePrincipal -SearchString "Microsoft"

    # Assert
    Assert-NotNull($servicePrincipals)
    foreach($servicePrincipal in $servicePrincipals) {
        Assert-NotNull($servicePrincipal.DisplayName)
        Assert-NotNull($servicePrincipal.Id)
        Assert-NotNull($servicePrincipal.ServicePrincipalName)
    }
}

<#
.SYNOPSIS
Tests getting Active Directory service principals.
#>
function Test-GetADServicePrincipalWithBadSearchString
{
    # Test
    $servicePrincipals = Get-AzureADServicePrincipal -SearchString "badsearchstring"

    # Assert
    Assert-Null($servicePrincipals)
}

<#
.SYNOPSIS
Tests getting Active Directory users.
#>
function Test-GetAllADUser
{
    # Test
    $users = Get-AzureADUser

    # Assert
    Assert-NotNull($users)
    foreach($user in $users) {
        Assert-NotNull($user.DisplayName)
        Assert-NotNull($user.Id)
    }
}

<#
.SYNOPSIS
Tests getting Active Directory users.
#>
function Test-GetADUserWithObjectId
{
    # Test
    $users = Get-AzureADUser -ObjectId "7b45838f-42c3-4fef-a85a-0a9051dfda41"

    # Assert
    Assert-AreEqual $users.Count 1
    Assert-AreEqual $users[0].Id "7b45838f-42c3-4fef-a85a-0a9051dfda41"
    Assert-NotNull($users[0].DisplayName)
    Assert-NotNull($users[0].UserPrincipalName)
}

<#
.SYNOPSIS
Tests getting Active Directory users.
#>
function Test-GetADUserWithBadObjectId
{
    # Test
    $users = Get-AzureADUser -ObjectId "deadbeef-dead-beef-dead-beefdeadbeef"

    # Assert
    Assert-Null($users)
}

<#
.SYNOPSIS
Tests getting Active Directory users.
#>
function Test-GetADUserWithGroupObjectId
{
    # Test
    $users = Get-AzureADUser -ObjectId "d5fbb343-cf1d-47bb-9aa8-5c3dd57b336f"

    # Assert
    Assert-Null($users)
}

<#
.SYNOPSIS
Tests getting Active Directory users.
#>
function Test-GetADUserWithUPN
{
    # Test
    $users = Get-AzureADUser -UserPrincipalName "admin@rbactest.onmicrosoft.com"

    # Assert
    Assert-AreEqual $users.Count 1
    Assert-AreEqual $users[0].UserPrincipalName "admin@rbactest.onmicrosoft.com"
    Assert-NotNull($users[0].DisplayName)
    Assert-NotNull($users[0].Id)
}

<#
.SYNOPSIS
Tests getting Active Directory users.
#>
function Test-GetADUserWithFPOUPN
{
    # Test
    $users = Get-AzureADUser -UserPrincipalName "azsdkposhteam_outlook.com#EXT#@rbactest.onmicrosoft.com"

    # Assert
    Assert-AreEqual $users.Count 1
    Assert-AreEqual $users[0].UserPrincipalName "azsdkposhteam_outlook.com#EXT#@rbactest.onmicrosoft.com"
    Assert-NotNull($users[0].DisplayName)
    Assert-NotNull($users[0].Id)
}

<#
.SYNOPSIS
Tests getting Active Directory users.
#>
function Test-GetADUserWithBadUPN
{
    # Test
    $users = Get-AzureADUser -UserPrincipalName "baduser@rbactest.onmicrosoft.com"

    # Assert
    Assert-Null($users)
}

<#
.SYNOPSIS
Tests getting Active Directory users.
#>
function Test-GetADUserWithSearchString
{
    # Test
    # Select at most 10 users. Users are restricted to contain "test" to fasten the test
    $users = Get-AzureADUser -SearchString "reader"

    # Assert
    Assert-NotNull($users)
    foreach($user in $users) {
        Assert-NotNull($user.DisplayName)
        Assert-NotNull($user.Id)
        Assert-NotNull($user.UserPrincipalName)
    }
}

<#
.SYNOPSIS
Tests getting Active Directory users.
#>
function Test-GetADUserWithBadSearchString
{
    # Test
    # Select at most 10 users. Users are restricted to contain "test" to fasten the test
    $users = Get-AzureADUser -SearchString "badsearchstring"

    # Assert
    Assert-Null($users)
}
