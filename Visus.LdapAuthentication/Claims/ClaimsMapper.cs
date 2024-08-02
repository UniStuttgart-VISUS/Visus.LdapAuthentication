// <copyright file="ClaimsMapper.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Visus.LdapAuthentication.Extensions;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;
using Novell.Directory.Ldap;


namespace Visus.LdapAuthentication.Claims {

    /// <summary>
    /// Implementation of the entry to claim mapper.
    /// </summary>
    public sealed class ClaimsMapper : ClaimsMapperBase<LdapEntry> {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="userMap">The mapping from LDAP attributes to claims
        /// for user entries.</param>
        /// <param name="groupMap">The mapping from LDAP attributes to claims
        /// for group entries.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="userMap"/> is <c>null</c>, or if
        /// <paramref name="groupMap"/> is <c>null</c>.</exception>
        public ClaimsMapper(IUserClaimsMap userMap, IGroupClaimsMap groupMap)
            : base(userMap, groupMap) { }
        #endregion

        #region Protected methods
        /// <inheritdoc />
        protected override object? GetAttribute(
                LdapEntry entry,
                LdapAttributeAttribute attribute) {
            return attribute?.GetValue(entry, typeof(string));
        }
        #endregion
    }
}
