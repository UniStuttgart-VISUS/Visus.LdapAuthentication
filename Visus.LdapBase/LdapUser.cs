// <copyright file="LdapUser.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Security.Claims;
using Visus.Ldap.Mapping;


namespace Visus.Ldap {

    /// <summary>
    /// The basic representation of a user from LDAP.
    /// </summary>
    public sealed class LdapUser {

        #region Public properties
        /// <summary>
        /// Gets the unique account name of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "sAMAccountName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sAMAccountName")]
        [LdapAttribute(Schema.Rfc2307, "uid")]
        [Claim(ClaimTypes.Name)]
        [Claim(ClaimTypes.WindowsAccountName)]
        [AccountName]
        public string AccountName { get; internal set; } = null!;

        /// <summary>
        /// Gets the Christian name of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "givenName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "givenName")]
        [LdapAttribute(Schema.Rfc2307, "givenName")]
        [Claim(ClaimTypes.GivenName)]
        public string? ChristianName { get; internal set; }

#if false
        /// <summary>
        /// Gets the claims (eg group memberships) for the user.
        /// </summary>
        [Claims]
        public IEnumerable<Claim> Claims { get; internal set; }
#endif

        /// <summary>
        /// Gets the display name of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "displayName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "displayName")]
        [LdapAttribute(Schema.Rfc2307, "displayName")]
        public string? DisplayName { get; internal set; }

        /// <summary>
        /// Gets the distinguished name of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "distinguishedName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "distinguishedName")]
        [LdapAttribute(Schema.Rfc2307, "distinguishedName")]
        [DistinguishedName]
        public string DistinguishedName { get; internal set; } = null!;

        /// <summary>
        /// Gets the e-mail address of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "mail")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "mail")]
        [LdapAttribute(Schema.Rfc2307, "mail")]
        [Claim(ClaimTypes.Email)]
        public string? EmailAddress { get; internal set; }

#if false
        /// <summary>
        /// Gets the groups the user is member of.
        /// </summary>
        [LdapGroups]
        public IEnumerable<TGroup> Groups { get; internal set; }
#endif

        /// <summary>
        /// Gets the security identifier of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "objectSid",
            Converter = typeof(SidConverter))]
        [LdapAttribute(Schema.IdentityManagementForUnix, "uidNumber")]
        [LdapAttribute(Schema.Rfc2307, "uidNumber")]
        [Claim(ClaimTypes.PrimarySid)]
        [Claim(ClaimTypes.Sid)]
        [Claim(ClaimTypes.NameIdentifier)]
        [Identity]
        public string Identity { get; internal set; } = null!;

        /// <summary>
        /// Gets the surname of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "sn")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sn")]
        [LdapAttribute(Schema.Rfc2307, "sn")]
        [Claim(ClaimTypes.Surname)]
        public string? Surname { get; internal set; }
        #endregion
    }
}
