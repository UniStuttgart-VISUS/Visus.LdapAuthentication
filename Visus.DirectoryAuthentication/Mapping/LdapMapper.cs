// <copyright file="LdapMapper.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Diagnostics;
using System.DirectoryServices.Protocols;
using Visus.DirectoryAuthentication.Extensions;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Mapping {

    /// <summary>
    /// The LDAP mapper based on attribute annotations on the
    /// <see cref="TUser"/> and <see cref="TGroup"/> classes.
    /// </summary>
    /// <typeparam name="TUser">The type used to represent a user.</typeparam>
    /// <typeparam name="TGroup">The type used to represent a group.</typeparam>
    public sealed class LdapMapper<TUser, TGroup>
            : LdapMapperBase<SearchResultEntry, TUser, TGroup> {

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="userMap"></param>
        /// <param name="groupMap"></param>
        public LdapMapper(ILdapAttributeMap<TUser> userMap,
                ILdapAttributeMap<TGroup> groupMap)
                : base(userMap, groupMap) { }

        /// <inheritdoc />
        protected override object? GetAttribute(SearchResultEntry entry,
                LdapAttributeAttribute attribute) {
            Debug.Assert(entry != null);
            Debug.Assert(attribute != null);
            return attribute.GetValue(entry);
        }
    }
}
