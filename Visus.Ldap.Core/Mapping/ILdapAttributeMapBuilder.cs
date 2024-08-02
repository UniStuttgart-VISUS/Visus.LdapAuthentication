// <copyright file="ILdapAttributeMapBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// The interface of a builder class that allows for fluently creating a
    /// custom mapping between LDAP attributes and properties of a user or
    /// group object.
    /// </summary>
    /// <typeparam name="TObject">The object to be mapped, which is typically
    /// the representation of a user or a group.</typeparam>
    public interface ILdapAttributeMapBuilder<TObject> {

        /// <summary>
        /// Builds the map from the current state of the builder.
        /// </summary>
        /// <returns>A new <see cref="ILdapAttributeMap{TObject}"/> based on the
        /// current state of the builder, or possibly <c>null</c> if nothing has
        /// been mapped.</returns>
        /// <exception cref="InvalidOperationException">If the builder is not in
        /// a state to build a valid mapper.</exception>
        ILdapAttributeMap<TObject>? Build();

        /// <summary>
        /// Begins mapping the given property of <typeparamref name="TObject"/>
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
        ILdapPropertyMappingBuilder MapProperty(string propertyName);
    }
}
