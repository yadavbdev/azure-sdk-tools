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

Function to get user name and password for a variety of different account types
#>

function Get-UserCredentials ([string] $userType) 
{
	function get-from-environment ($varName) {
	    if (-not (test-path "Env:\$varName")) {
		    throw "Required environment variable $varName is not set"
		}
		(get-childitem "Env:\$varName").Value
	}

	function credential-from-username-password ($user, $password) 
	{
		$ss = ConvertTo-SecureString -String $password -AsPlainText -Force
		New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $user, $ss
	}

	function credential-from-connection-string ([string] $cs) 
	{
		$fields = [Microsoft.WindowsAzure.Testing.TestEnvironmentFactory]::ParseConnectionString($cs)
		$user = $fields[[Microsoft.WindowsAzure.Testing.ConnectionStringFields]::UserId]
		$password = $fields[[Microsoft.WindowsAzure.Testing.ConnectionStringFields]::Password]
		credential-from-username-password $user $password
	}

	function credential-from-environment-var ([string] $envVarPrefix) 
	{
		credential-from-connection-string (get-from-environment $envVarPrefix)
	}

	$typeHandlers = @{
		OrgIdOneTenantOneSubscription = {
			@{
				Credential = (credential-from-environment-var "AZURE_ORGID_ONE_TENANT_ONE_SUBSCRIPTION");
				Environment = (get-from-environment "AZURE_ORGID_ONE_TENANT_ONE_SUBSCRIPTION_ENVIRONMENT");
				ExpectedSubscription = (get-from-environment "AZURE_ORGID_ONE_TENANT_ONE_SUBSCRIPTION_SUBSCRIPTIONID")
			}
		};
		OrgIdForeignPrincipal = {
			@{
				Credential = (credential-from-environment-var "AZURE_ORGID_FPO");
				Environment = (get-from-environment "AZURE_ORGID_FPO_ENVIRONMENT")
			}
		};
		MicrosoftId = {
			@{
				Credential = (credential-from-environment-var "AZURE_LIVEID");
				Environment = 'AzureCloud'
			}
		}
	}

	$handler = $typeHandlers[$userType]
	if ($handler -ne $Null) {
		& $handler
	} else {
		throw "Unknown credential type $userType"
	}
}
