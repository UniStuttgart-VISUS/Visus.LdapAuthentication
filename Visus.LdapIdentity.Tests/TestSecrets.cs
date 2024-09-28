// <copyright file="TestSecrets.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapIdentity.Tests {

    /// <summary>
    /// Container for secret test settings (LDAP credentials).
    /// </summary>
    /// <remarks>
    /// Provide this configuration in the user secrets of Visual Studio in order
    /// to run the tests against an actual LDAP directory.
    /// </remarks>
    internal sealed class TestSecrets {

        /// <summary>
        /// Create a new instance that is bound to the actual secrets.
        /// </summary>
        /// <returns></returns>
        public static TestSecrets Create() {
            var retval = new TestSecrets();
            TestExtensions.CreateConfiguration().Bind(retval);
            return retval;
        }

        /// <summary>
        /// Answer whether tests requiring an actual server can run.
        /// </summary>
        public bool CanRun
            => !string.IsNullOrWhiteSpace(this.ExistingGroupAccount)
            && !string.IsNullOrWhiteSpace(this.ExistingGroupIdentity)
            && !string.IsNullOrWhiteSpace(this.ExistingUserAccount)
            && !string.IsNullOrWhiteSpace(this.ExistingUserIdentity)
            && (this.LdapOptions != null)
            && !string.IsNullOrWhiteSpace(this.LdapOptions.User)
            && !string.IsNullOrWhiteSpace(this.LdapOptions.Password)
            && !string.IsNullOrWhiteSpace(this.NonExistingUserAccount)
            && !string.IsNullOrWhiteSpace(this.NonExistingUserIdentity);

        /// <summary>
        /// Gets or sets the account name of a known group that can be searched
        /// in the directory.
        /// </summary>
        public string? ExistingGroupAccount { get; set; }

        /// <summary>
        /// Gets or sets the identity of a known group that can be searched in the
        /// directory.
        /// </summary>
        public string? ExistingGroupIdentity { get; set; }

        /// <summary>
        /// Gets or sets the account name of a known user that can be searched
        /// in the directory.
        /// </summary>
        public string? ExistingUserAccount { get; set; }

        /// <summary>
        /// Gets or sets the identity of a known user that can be searched in the
        /// directory.
        /// </summary>
        public string? ExistingUserIdentity { get; set; }

        /// <summary>
        /// Gets or sets the LDAP options for the test.
        /// </summary>
        public LdapOptions? LdapOptions { get; set; }

        /// <summary>
        /// Gets or sets the account name of a non-existing user.
        /// </summary>
        public string? NonExistingUserAccount { get; set; }

        /// <summary>
        /// Gets or sets the identity of a non-existing user.
        /// </summary>
        public string? NonExistingUserIdentity { get; set; }

    }
}
