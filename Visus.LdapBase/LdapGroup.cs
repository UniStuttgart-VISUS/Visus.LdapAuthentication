// <copyright file="LdapGroup.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Security.Claims;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;


namespace Visus.Ldap {

    /// <summary>
    /// The basic representation of a group from LDAP.
    /// </summary>
    public sealed class LdapGroup {

        /// <summary>
        /// Gets the unique name of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "sAMAccountName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sAMAccountName")]
        [LdapAttribute(Schema.Rfc2307, "gid")]
        [Claim(ClaimTypes.Role)]
        [AccountName]
        public string AccountName { get; internal set; } = null!;

        /// <summary>
        /// Gets the display name of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "displayName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "displayName")]
        [LdapAttribute(Schema.Rfc2307, "displayName")]
        public string? DisplayName { get; internal set; }

        /// <summary>
        /// Gets the distinguished name of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "distinguishedName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "distinguishedName")]
        [LdapAttribute(Schema.Rfc2307, "distinguishedName")]
        [DistinguishedName]
        public string DistinguishedName { get; internal set; } = null!;

        /// <summary>
        /// Gets the security identifier of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "objectSid",
            Converter = typeof(SidConverter))]
        [LdapAttribute(Schema.IdentityManagementForUnix, "gidNumber")]
        [LdapAttribute(Schema.Rfc2307, "gidNumber")]
        [Claim(ClaimTypes.GroupSid)]
        [Identity]
        public string Identity { get; internal set; } = null!;

        /// <summary>
        /// Gets whether the group is the primary group of the user.
        /// </summary>
        [PrimaryGroupFlag]
        public bool IsPrimary { get; internal set; }
    }
}
