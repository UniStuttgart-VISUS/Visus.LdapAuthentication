// <copyright file="TestSecrets.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


using Visus.LdapAuthentication.Configuration;

namespace Visus.LdapAuthentication.Tests
{

    /// <summary>
    /// Container for secret test settings (LDAP credentials).
    /// </summary>
    /// <remarks>
    /// Provide this configuration in the user secrets of Visual Studio in order
    /// to run the tests against an actual LDAP directory.
    /// </remarks>
    internal sealed class TestSecrets {

        /// <summary>
        /// Gets or sets the account name of a known user that can be searched
        /// in the directory.
        /// </summary>
        public string ExistingUserAccount { get; set; }

        /// <summary>
        /// Gets or sets the identity of a known user that can be searched in the
        /// directory.
        /// </summary>
        public string ExistingUserIdentity { get; set; }

        /// <summary>
        /// Gets or sets the LDAP options for the test.
        /// </summary>
        public LdapOptions LdapOptions { get; set; }

        /// <summary>
        /// Gets or sets the name of a non-existing user account.
        /// </summary>
        public string NonExistingUserAccount { get; set; }

        /// <summary>
        /// Gets or sets the identity of a non-existing user.
        /// </summary>
        public string NonExistingUserIdentity { get; set; }

    }
}
