// <copyright file="TestUser1.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Visus.Ldap;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// A custom user class for testing.
    /// </summary>
    public sealed class TestUser1 : LdapUser {

        [LdapAttribute("Active Directory", "thumbnailPhoto",
            Converter = typeof(BinaryConverter))]
        public string? ProfilePicture { get; set; }
    }
}
