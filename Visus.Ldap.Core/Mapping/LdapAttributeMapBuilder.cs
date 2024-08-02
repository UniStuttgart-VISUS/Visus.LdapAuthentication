// <copyright file="LdapAttributeMapBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Basic  implementation of a fluent
    /// <see cref="ILdapAttributeMapBuilder{TObject}"/>.
    /// </summary>
    /// <typeparam name="TObject">The object to be mapped, which is typically
    /// the representation of a user or a group.</typeparam>
    public sealed class LdapAttributeMapBuilder<TObject>
            : ILdapAttributeMapBuilder<TObject> {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="schema">The schema the mapping is intended for.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="schema"/>
        /// is <c>null.</c></exception>
        public LdapAttributeMapBuilder(string schema) => this._schema = schema
            ?? throw new ArgumentNullException(nameof(schema));
        #endregion

        #region Public methods
        /// <inheritdoc />
        public ILdapAttributeMap<TObject>? Build() {
            return this._map.IsValueCreated ? this._map.Value : null;
        }

        /// <inheritdoc />
        public ILdapPropertyMappingBuilder MapProperty(string propertyName)
            => this._map.Value.MapProperty(propertyName, this._schema);
        #endregion

        #region Private fields
        private readonly Lazy<FluentLdapAttributeMap<TObject>> _map = new(
            () => new FluentLdapAttributeMap<TObject>());
        private readonly string _schema;
        #endregion
    }
}
