// <copyright file="TestUser2.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Visus.Ldap.Mapping;
using System.Security.Claims;
using Visus.Ldap.Claims;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// A custom user class for testing.
    /// </summary>
    public sealed class TestUser2 {

        [LdapAttribute(Schema.ActiveDirectory, "sAMAccountName")]
        [Claim(ClaimTypes.Name)]
        [AccountName]
        public string AccountName { get; set; } = null!;

        [LdapAttribute(Schema.ActiveDirectory, "distinguishedName")]
        [DistinguishedName]
        public string DistinguishedName { get; set; } = null!;

        [LdapAttribute(Schema.ActiveDirectory, "objectSid",
            Converter = typeof(SidConverter))]
        [Claim(ClaimTypes.NameIdentifier)]
        [Identity]
        public string Identity { get; set; } = null!;
    }
}
