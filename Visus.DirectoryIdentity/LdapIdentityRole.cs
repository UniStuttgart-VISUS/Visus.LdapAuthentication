// <copyright file="LdapIdentityRoleBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Security.Claims;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// The basic representation of a role, which is derived from an LDAP group.
    /// </summary>
    public class LdapIdentityRole {

        #region Public properties
        /// <summary>
        /// Gets or sets the distinguished name of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "distinguishedName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "distinguishedName")]
        [LdapAttribute(Schema.Rfc2307, "distinguishedName")]
        [DistinguishedName]
        public string DistinguishedName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the group that represents the role.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "sAMAccountName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sAMAccountName")]
        [LdapAttribute(Schema.Rfc2307, "gid")]
        [Claim(ClaimTypes.Name)]
        [Claim(ClaimTypes.WindowsAccountName)]
        [AccountName]
        public string? Name { get; internal set; }

        /// <summary>
        /// Gets or sets the unique identitfier of the role, which is equivalent
        /// to the unqiue ID of the group in this implementation.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "objectSid",
            Converter = typeof(SidConverter))]
        [LdapAttribute(Schema.IdentityManagementForUnix, "gidNumber")]
        [LdapAttribute(Schema.Rfc2307, "gidNumber")]
        [Claim(ClaimTypes.GroupSid)]
        [Identity]
        public string ID { get; set; } = null!;
        #endregion

        #region Public methods
        /// <inheritdoc />
        public override string ToString() => this.Name ?? string.Empty;
        #endregion
    }
}
