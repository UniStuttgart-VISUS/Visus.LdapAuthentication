// <copyright file="ILdapAttributeMapSchemaSelector.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// A builder that creates a new
    /// <see cref="ILdapAttributeMapBuilder{TObject}"/> for the given schema.
    /// </summary>
    /// <typeparam name="TObject">The object to be mapped, which is typically
    /// the representation of a user or a group.</typeparam>
    public interface ILdapAttributeMapSchemaSelector<TObject> {

        /// <summary>
        /// Begins mapping <typeparamref name="TObject"/> for the given
        /// <paramref name="schema"/>.
        /// </summary>
        /// <param name="schema">The name of the schema the map is for.</param>
        /// <returns>A builder for an LDAP attribute map.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="schema"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If the method is called
        /// for different values of <paramref name="schema"/>.</exception>
        ILdapAttributeMapBuilder<TObject> ForSchema(string schema);
    }
}
