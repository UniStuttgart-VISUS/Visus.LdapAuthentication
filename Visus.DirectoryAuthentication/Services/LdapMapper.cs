// <copyright file="LdapMapper.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Extensions;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Services {

    /// <summary>
    /// The LDAP mapper based on attribute annotations on the
    /// <see cref="TUser"/> and <see cref="TGroup"/> classes.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TGroup"></typeparam>
    internal class LdapMapper<TUser, TGroup>
            : LdapMapperBase<SearchResultEntry, TUser, TGroup> {

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP options defining the mapping to
        /// be performed.</param>
        /// <param name="userMap"></param>
        /// <param name="groupMap"></param>
        public LdapMapper(IOptions<LdapOptions> options,
                ILdapAttributeMap<TUser> userMap,
                ILdapAttributeMap<TGroup> groupMap)
            : base(options, userMap, groupMap) { }

        /// <inheritdoc />
        protected override object? GetAttribute(SearchResultEntry entry,
                LdapAttributeAttribute attribute) {
            Debug.Assert(entry != null);
            Debug.Assert(attribute != null);
            return attribute.GetValue(entry);
        }
    }

}
