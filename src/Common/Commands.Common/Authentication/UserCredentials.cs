// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System.Security;
using Microsoft.WindowsAzure.Commands.Common.Factories;

namespace Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication
{
    public struct UserCredentials
    {
        public string UserName { get; set; }

        public SecureString Password { get; set; }

        public ShowDialog ShowDialog { get; set; }

        public CredentialType Type { get; set; }

        public string Tenant { get; set; }

        public string GetTenant()
        {
            if (!string.IsNullOrEmpty(Tenant))
            {
                return Tenant;
            }

            return AuthenticationFactory.CommonAdTenant;
        }
    }
}
