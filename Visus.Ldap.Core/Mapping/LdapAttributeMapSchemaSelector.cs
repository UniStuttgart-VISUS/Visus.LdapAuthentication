// <copyright file="LdapAttributeMapSchemaSelector.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using Visus.Ldap.Properties;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// The default implemenation of
    /// <see cref="ILdapAttributeMapSchemaSelector{TObject}"/>.
    /// </summary>
    /// <typeparam name="TObject">The object to be mapped, which is typically
    /// the representation of a user or a group.</typeparam>
    public sealed class LdapAttributeMapSchemaSelector<TObject>
            : ILdapAttributeMapSchemaSelector<TObject> {

        /// <summary>
        /// Gets the builder that has been selected.
        /// </summary>
        public LdapAttributeMapBuilder<TObject>? Builder { get; private set; }

        /// <inheritdoc />
        public ILdapAttributeMapBuilder<TObject> ForSchema(string schema) {
            if (this.Builder != null) {
                if (this.Builder.Schema != schema) {
                    throw new InvalidOperationException(
                        Resources.ErrorSchemaChange);
                }

            } else {
                this.Builder = new LdapAttributeMapBuilder<TObject>(schema);
            }

            return this.Builder;
        }
    }
}
