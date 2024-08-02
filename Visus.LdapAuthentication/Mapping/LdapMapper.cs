// <copyright file="LdapMapper.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using System;
using System.Diagnostics;
using Visus.Ldap.Mapping;
using Visus.LdapAuthentication.Extensions;


namespace Visus.LdapAuthentication.Mapping {

    /// <summary>
    /// The LDAP mapper based on attribute annotations on the
    /// <see cref="TUser"/> and <see cref="TGroup"/> classes.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TGroup"></typeparam>
    public sealed class LdapMapper<TUser, TGroup>
            : LdapMapperBase<LdapEntry, TUser, TGroup> {

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="userMap"></param>
        /// <param name="groupMap"></param>
        public LdapMapper(ILdapAttributeMap<TUser> userMap,
                ILdapAttributeMap<TGroup> groupMap)
                : base(userMap, groupMap) { }

        /// <inheritdoc />
        protected override object? GetAttribute(LdapEntry entry,
                Type targetType,
                LdapAttributeAttribute attribute) {
            Debug.Assert(entry != null);
            Debug.Assert(targetType != null);
            Debug.Assert(attribute != null);
            return attribute.GetValue(entry, targetType);
        }
    }
}
