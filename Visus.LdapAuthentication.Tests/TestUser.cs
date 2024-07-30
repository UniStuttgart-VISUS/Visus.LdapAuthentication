// <copyright file="TestUser.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Visus.Ldap.Mapping;
using Visus.Ldap;


namespace Visus.LdapAuthentication.Tests {
    /// <summary>
    /// A custom user class for testing.
    /// </summary>
    public sealed class TestUser : LdapUser {

        [LdapAttribute("Active Directory", "thumbnailPhoto")]
        public string? ProfilePicture { get; set; }
    }
}
