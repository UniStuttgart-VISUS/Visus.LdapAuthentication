// <copyright file="LdapUserBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using System.Security.Claims;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Base class for implementing custom users.
    /// </summary>
    /// <typeparam name="TGroup">The type used to represent a group a user can
    /// be member of.</typeparam>
    public abstract class LdapUserBase<TGroup> {

        #region Public properties
        /// <summary>
        /// Gets the unique account name of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "sAMAccountName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sAMAccountName")]
        [LdapAttribute(Schema.Rfc2307, "uid")]
        [Claim(ClaimTypes.Name)]
        [Claim(ClaimTypes.WindowsAccountName)]
        public virtual string AccountName { get; internal set; }

        /// <summary>
        /// Gets the Christian name of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "givenName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "givenName")]
        [LdapAttribute(Schema.Rfc2307, "givenName")]
        [Claim(ClaimTypes.GivenName)]
        public virtual string ChristianName { get; internal set; }

        /// <summary>
        /// Gets the claims (eg group memberships) for the user.
        /// </summary>
        [Claims]
        public virtual IEnumerable<Claim> Claims { get; internal set; }

        /// <summary>
        /// Gets the display name of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "displayName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "displayName")]
        [LdapAttribute(Schema.Rfc2307, "displayName")]
        public virtual string DisplayName { get; internal set; }

        /// <summary>
        /// Gets the e-mail address of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "mail")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "mail")]
        [LdapAttribute(Schema.Rfc2307, "mail")]
        [Claim(ClaimTypes.Email)]
        public virtual string EmailAddress { get; internal set; }

        /// <summary>
        /// Gets the groups the user is member of.
        /// </summary>
        [LdapGroups]
        public virtual IEnumerable<TGroup> Groups { get; internal set; }

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
        [LdapIdentity]
        public virtual string Identity { get; internal set; }

        /// <summary>
        /// Gets the surname of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "sn")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sn")]
        [LdapAttribute(Schema.Rfc2307, "sn")]
        [Claim(ClaimTypes.Surname)]
        public virtual string Surname { get; internal set; }
        #endregion
    }
}
