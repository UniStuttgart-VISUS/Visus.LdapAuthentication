// <copyright file="LdapGroup.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Diagnostics;
using System.Security.Claims;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;


namespace Visus.Ldap {

    /// <summary>
    /// The basic representation of a group from LDAP.
    /// </summary>
    /// <remarks>
    /// <para>This class provides mappings for LDAP attributes typically used
    /// for authenticating users. It most importantly provides properties
    /// to map <see cref="ClaimTypes.GroupSid"/>,
    /// <see cref="ClaimTypes.PrimaryGroupSid"/> and
    /// <see cref="ClaimTypes.Role"/>. The group name is considered to be
    /// the role.</para>
    /// <para>If you need additional data from the directory, you can derive
    /// from this class and add these properties with appropriate attributes.
    /// Make sure to use the attribute-based mapper in this case as the
    /// performance-optimised ones derived from
    /// <see cref="DefaultMapperBase{TEntry}"/> only fill the properties
    /// in this class, but not the ones you may have added.</para>
    /// <para>Note to implementors: When changing this class, make sure to
    /// reflect the hard-coded assignments of properties in
    /// <see cref="DefaultMapperBase{TEntry}.MapGroup(TEntry, LdapGroup)"/>.
    /// </para>
    /// </remarks>
    [DebuggerDisplay("{AccountName}")]
    public class LdapGroup : IEquatable<LdapGroup> {

        #region Public properties
        /// <summary>
        /// Gets the unique name of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "sAMAccountName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sAMAccountName")]
        [LdapAttribute(Schema.Rfc2307, "gid")]
        [Claim(ClaimTypes.Role)]
        [AccountName]
        public string AccountName { get; set; } = null!;

        /// <summary>
        /// Gets the display name of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "displayName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "displayName")]
        [LdapAttribute(Schema.Rfc2307, "displayName")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets the distinguished name of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "distinguishedName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "distinguishedName")]
        [LdapAttribute(Schema.Rfc2307, "distinguishedName")]
        [DistinguishedName]
        public string DistinguishedName { get; set; } = null!;

        /// <summary>
        /// Gets the security identifier of the group.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "objectSid",
            Converter = typeof(SidConverter))]
        [LdapAttribute(Schema.IdentityManagementForUnix, "gidNumber")]
        [LdapAttribute(Schema.Rfc2307, "gidNumber")]
        [Claim(ClaimTypes.GroupSid)]
        [Identity]
        public string Identity { get; set; } = null!;

        /// <summary>
        /// Gets whether the group is the primary group of the user.
        /// </summary>
        [PrimaryGroupFlag]
        public bool IsPrimary { get; set; }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public bool Equals(LdapGroup? other) {
            if (other == null) {
                return false;
            }

            if (this.Identity != null) {
                return this.Identity.Equals(other.Identity);
            }

            if (this.DistinguishedName != null) {
                return this.DistinguishedName.Equals(other.DistinguishedName);
            }

            if (this.AccountName != null) {
                return this.AccountName.Equals(other.AccountName);
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => this.Equals(obj as LdapGroup);

        /// <inheritdoc />
        public override int GetHashCode() {
            var typeHash = this.GetType().GetHashCode();

            if (this.Identity != null) {
                return this.Identity.GetHashCode() ^ (typeHash >> 1);
            }

            if (this.DistinguishedName != null) {
                return this.DistinguishedName.GetHashCode() ^ (typeHash >> 3);
            }

            if (this.AccountName != null) {
                return this.AccountName.GetHashCode() ^ (typeHash >> 2);
            }

            return base.GetHashCode();
        }
        #endregion
    }
}
