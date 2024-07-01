// <copyright file="LdapGroupBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


using System.Security.Claims;

namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Basic implementation of the in-memory representation of an LDAP group.
    /// </summary>
    public abstract class LdapGroupBase {

        /// <summary>
        /// Gets the unique name of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "sAMAccountName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sAMAccountName")]
        [LdapAttribute(Schema.Rfc2307, "gid")]
        public virtual string AccountName { get; internal set; }

        /// <summary>
        /// Gets the display name of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "displayName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "displayName")]
        [LdapAttribute(Schema.Rfc2307, "displayName")]
        public virtual string DisplayName { get; internal set; }

        /// <summary>
        /// Gets the distinguished name of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "distinguishedName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "distinguishedName")]
        [LdapAttribute(Schema.Rfc2307, "distinguishedName")]
        [DistinguishedName]
        public virtual string DistinguishedName { get; internal set; }

        /// <summary>
        /// Gets the security identifier of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "objectSid",
            Converter = typeof(SidConverter))]
        [LdapAttribute(Schema.IdentityManagementForUnix, "gidNumber")]
        [LdapAttribute(Schema.Rfc2307, "gidNumber")]
        [LdapIdentity]
        [Claim(ClaimTypes.GroupSid)]
        public virtual string Identity { get; internal set; }

        /// <summary>
        /// Gets whether the group is the primary group of the user.
        /// </summary>
        public virtual bool IsPrimary { get; internal set; }
    }
}
