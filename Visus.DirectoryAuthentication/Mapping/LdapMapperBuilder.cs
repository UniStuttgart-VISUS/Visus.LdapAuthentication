// <copyright file="LdapMapperBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.DirectoryServices.Protocols;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Mapping {

    /// <summary>
    /// A fluent builder for <see cref="ILdapMapper{TEntry, TUser, TGroup}"/>s.
    /// </summary>
    /// <typeparam name="TUser">The type used to represent a user.</typeparam>
    /// <typeparam name="TGroup">The type used to represent a group.</typeparam>
    public sealed class LdapMapperBuilder<TUser, TGoup>
            : LdapMapperBuilderBase<SearchResultEntry, TUser, TGoup> {

        /// <inheritdoc />
        public override ILdapMapper<SearchResultEntry, TUser, TGoup> Build()
            => new LdapMapper<TUser, TGoup>(this.UserMap, this.GroupMap);
    }
}
