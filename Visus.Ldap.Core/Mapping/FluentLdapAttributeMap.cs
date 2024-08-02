// <copyright file="FluentLdapAttributeMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Visus.Ldap.Properties;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// An implementation of <see cref="ILdapAttributeMap{TObject}"/> that is
    /// filled by a <see cref="ILdapAttributeMapBuilder{TEntry, TUser, TGroup}"/>.
    /// </summary>
    /// <typeparam name="TObject">The object to be mapped to.</typeparam>
    internal class FluentLdapAttributeMap<TObject>
            : ILdapAttributeMap<TObject> {

        #region Public properties
        /// <inheritdoc />
        public PropertyInfo? AccountNameProperty { get; internal set; }

        /// <inheritdoc />
        public IEnumerable<string> AttributeNames
            => this.Attributes.Select(a => a.Name).Distinct();

        /// <inheritdoc />
        public IEnumerable<LdapAttributeAttribute> Attributes
            => this._properties.Values;

        /// <inheritdoc />
        public PropertyInfo? DistinguishedNameProperty { get; internal set; }

        /// <inheritdoc />
        public PropertyInfo? GroupMembershipsProperty { get; internal set; }

        /// <inheritdoc />
        public PropertyInfo? IdentityProperty { get; internal set; }

        /// <inheritdoc />
        public PropertyInfo? IsPrimaryGroupProperty { get; internal set; }

        /// <inheritdoc />
        public IEnumerable<PropertyInfo> Properties
            => this._properties.Keys;
        #endregion

        #region Public methods
        /// <inheritdoc />
        public IEnumerator<KeyValuePair<PropertyInfo,
                LdapAttributeAttribute>> GetEnumerator()
            => this._properties.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
            => this._properties.Keys.GetEnumerator();
        #endregion

        #region Public indexers
        /// <inheritdoc />
        public LdapAttributeAttribute? this[PropertyInfo property]
            => this._properties.TryGetValue(property, out var retval)
            ? retval
            : null;
        #endregion

        #region Internal methods
        /// <summary>
        /// Tries adding the given <paramref name="propertyName"/> to the map
        /// and return a builder to configure the mapping target.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        internal ILdapPropertyMappingBuilder MapProperty(string propertyName,
                string schema) {
            ArgumentException.ThrowIfNullOrEmpty(propertyName,
                nameof(propertyName));
            ArgumentException.ThrowIfNullOrEmpty(schema,
                nameof(schema));
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var property = typeof(TObject).GetProperty(propertyName, flags);
            if (property == null) {
                var msg = Resources.ErrorPropertyMissing;
                msg = string.Format(msg, propertyName, typeof(TObject).Name);
                throw new ArgumentException(msg);
            }

            return new EntryBuilder(this, property, schema);
        }
        #endregion

        #region Nested class EntryBuilder
        /// <summary>
        /// The proxy creating entries in the map.
        /// </summary>
        private class EntryBuilder : ILdapPropertyMappingBuilder,
                ILdapAttributeMappingBuilder {

            #region Public constructors
            public EntryBuilder(
                    FluentLdapAttributeMap<TObject> map,
                    PropertyInfo property,
                    string schema) {
                Debug.Assert(map != null);
                Debug.Assert(property != null);
                Debug.Assert(schema != null);
                this._map = map;
                this._property = property;
                this._schema = schema;
            }
            #endregion

            #region Public methods
            /// <inheritdoc />
            public ILdapPropertyMappingBuilder StoringAccountName() {
                if (this._map.AccountNameProperty != null) {
                    throw new InvalidOperationException(
                        Resources.ErrorAccountNamePropertyAlreadySet);
                }

                this._map.AccountNameProperty = this._property;
                return this;
            }

            /// <inheritdoc />
            public ILdapPropertyMappingBuilder StoringDistinguishedName() {
                if (this._map.DistinguishedNameProperty != null) {
                    throw new InvalidOperationException(
                        Resources.ErrorDistinguishedNamePropertyAlreadySet);
                }

                this._map.DistinguishedNameProperty = this._property;
                return this;
            }

            /// <inheritdoc />
            public ILdapPropertyMappingBuilder StoringGroupMemberships() {
                if (this._map.GroupMembershipsProperty != null) {
                    throw new InvalidOperationException(
                        Resources.ErrorGroupMembershipsPropertyAlreadySet);
                }

                this._map.GroupMembershipsProperty = this._property;
                return this;
            }

            /// <inheritdoc />
            public ILdapPropertyMappingBuilder StoringIdentity() {
                if (this._map.IdentityProperty != null) {
                    throw new InvalidOperationException(
                        Resources.ErrorIdentityPropertyAlreadySet);
                }

                this._map.IdentityProperty = this._property;
                return this;
            }

            /// <inheritdoc />
            public ILdapPropertyMappingBuilder StoringPrimaryGroupFlag() {
                if (this._map.IsPrimaryGroupProperty!= null) {
                    throw new InvalidOperationException(
                        Resources.ErrorIsPrimaryGroupPropertyAlreadySet);
                }

                this._map.IsPrimaryGroupProperty = this._property;
                return this;
            }

            /// <inheritdoc />
            public ILdapAttributeMappingBuilder ToAttribute(
                    string attributeName) {
                ArgumentException.ThrowIfNullOrEmpty(attributeName,
                    nameof(attributeName));
                var attribute = new LdapAttributeAttribute(this._schema,
                    attributeName);
                this.ToAttribute(attribute);
                return this;
            }

            /// <inheritdoc />
            public void ToAttribute(LdapAttributeAttribute attribute) {
                ArgumentNullException.ThrowIfNull(attribute, nameof(attribute));

                if (attribute.Schema != this._schema) {
                    throw new ArgumentException(Resources.ErrorSchemaMismatch);
                }

                if (this._attribute != null) {
                    throw new InvalidOperationException(
                        Resources.ErrorPropertyAlreadyMapped);
                }

                this._map._properties[this._property]
                    = this._attribute
                    = attribute;
            }

            /// <inheritdoc />
            public void WithConverter(Type converter) {
                Debug.Assert(this._attribute != null);
                ArgumentNullException.ThrowIfNull(converter, nameof(converter));
                this._attribute.Converter = converter;
            }
            #endregion

            #region Private fields
            private LdapAttributeAttribute? _attribute;
            private readonly FluentLdapAttributeMap<TObject> _map;
            private readonly PropertyInfo _property;
            private readonly string _schema;
            #endregion
        }
        #endregion

        #region Private fields
        private Dictionary<PropertyInfo, LdapAttributeAttribute> _properties = new();
        #endregion
    }
}
