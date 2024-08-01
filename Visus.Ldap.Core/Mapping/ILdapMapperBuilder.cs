// <copyright file="ILdapMapperBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// The interface of a builder class that allows for fluently creating a
    /// custom mapper between LDAP attributes and properties of a user or
    /// group object.
    /// </summary>
    /// <typeparam name="TEntry">The type of the LDAP entries to be mapped to
    /// <typeparamref name="TUser"/> or <typeparamref name="Group"/>.
    /// </typeparam>
    /// <typeparam name="TUser">The type representing the user in memory.
    /// </typeparam>
    /// <typeparam name="TGroup">The type representing the group in memory.
    /// </typeparam>
    public interface ILdapMapperBuilder<TEntry, TUser, TGroup> {

        //ILdapAttributeMap<TUser>

        /// <summary>
        /// Builds the mapper from the current state of the builder.
        /// </summary>
        /// <returns>A new <see cref="ILdapMapper{TEntry, TUser, TGroup}"/>
        /// based on the current state of the builder.</returns>
        /// <exception cref="InvalidOperationException">If the builder is not in
        /// a state to build a valid mapper.</exception>
        ILdapMapper<TEntry, TUser, TGroup> Build();

        /// <summary>
        /// Configures the LDAP schema the mapping is for.
        /// </summary>
        /// <param name="schema">The name of the schema.</param>
        /// <returns><c>this</c></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="schema"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If a different schema has
        /// already been set before.</exception>
        ILdapMapperBuilder<TEntry, TUser, TGroup> ForSchema(string schema);

        /// <summary>
        /// Begins mapping the given property of <typeparamref name="TGroup"/>
        /// to an LDAP attribute.
        /// </summary>
        /// <param name="propertyName">The name of the property to map.</param>
        /// <returns>A builder for configuring the LDAP attribute.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="propertyName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If
        /// <paramref name="propertyName"/> does not exist in
        /// <typeparamref name="TGroup"/>.</exception>
        /// <exception cref="InvalidOperationException">If the builder is not
        /// in a state to map a property.</exception>
        ILdapPropertyMappingBuilder MapGroupProperty(string propertyName);

        /// <summary>
        /// Begins mapping the given property of <typeparamref name="TUser"/>
        /// to an LDAP attribute.
        /// </summary>
        /// <param name="propertyName">The name of the property to map.</param>
        /// <returns>A builder for configuring the LDAP attribute.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="propertyName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If
        /// <paramref name="propertyName"/> does not exist in
        /// <typeparamref name="TUser"/>.</exception>
        /// <exception cref="InvalidOperationException">If the builder is not
        /// in a state to map a property.</exception>
        ILdapPropertyMappingBuilder MapUserProperty(string propertyName);
    }
}
