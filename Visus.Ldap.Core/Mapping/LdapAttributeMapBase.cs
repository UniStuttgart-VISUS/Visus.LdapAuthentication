// <copyright file="LdapAttributeMapBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
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
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using Visus.Ldap.Configuration;
using Visus.Ldap.Properties;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// The reflection-based default implementation of
    /// <see cref="ILdapAttributeMap{TObject}"/>, which enumerates all
    /// properties of <typeparamref name="TObject"/> that have a
    /// <see cref="LdapAttributeAttribute"/> for the current
    /// <see cref="LdapOptionsBase.Schema"/>.
    /// </summary>
    /// <typeparam name="TObject">The object to be reflected.</typeparam>
    public abstract class LdapAttributeMapBase<TObject>
            : ILdapAttributeMap<TObject> {

        #region Public properties
        /// <inheritdoc />
        public PropertyInfo? AccountNameProperty { get; private set; }

        /// <inheritdoc />
        public IEnumerable<string> AttributeNames { get; }

        /// <inheritdoc />
        public IEnumerable<LdapAttributeAttribute> Attributes
            => this._properties.Values;

        /// <inheritdoc />
        public PropertyInfo? DistinguishedNameProperty { get; private set; }

        /// <inheritdoc />
        public PropertyInfo? GroupMembershipsProperty { get; private set; }

        /// <inheritdoc />
        public PropertyInfo? IdentityProperty { get; private set; }

        /// <inheritdoc />
        public PropertyInfo? IsPrimaryGroupProperty { get; private set; }

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

        #region Protected constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP configuration, which determines the
        /// LDAP schema to use.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        protected LdapAttributeMapBase(LdapOptionsBase options) {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            this._properties = (from p in typeof(TObject).GetProperties(PropertyFlags)
                                let a = p.GetCustomAttributes<LdapAttributeAttribute>()
                                    .Where(a => a.Schema == options.Schema)
                                    .FirstOrDefault()
                                where a != null
                                select new {
                                    Key = p,
                                    Value = a
                                }).ToDictionary(v => v.Key, v => v.Value);

            this.AttributeNames = this.Attributes.Select(a => a.Name)
                .Distinct()
                .ToArray();

            this.AccountNameProperty = AccountNameAttribute
                .GetProperty<TObject>();
            this.DistinguishedNameProperty = DistinguishedNameAttribute
                .GetProperty<TObject>();
            this.GroupMembershipsProperty = GroupMembershipsAttribute
                .GetGroupMemberships<TObject>();
            this.IdentityProperty = IdentityAttribute
                .GetProperty<TObject>();
            this.IsPrimaryGroupProperty = PrimaryGroupFlagAttribute
                .GetProperty<TObject>();
        }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="mapper">A callback that dynamically creates the mapping
        /// using a <see cref="ILdapAttributeMapBuilder{TObject}"/>.</param>
        /// <param name="options">The LDAP configuration, which determines the
        /// LDAP schema to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="mapper"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        protected LdapAttributeMapBase(Action<ILdapAttributeMapBuilder<TObject>,
                LdapOptionsBase> mapper, LdapOptionsBase options) {
            ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            this._properties = new();
            mapper(new MapBuilder(this, options.Schema), options);

            this.AttributeNames = this.Attributes.Select(a => a.Name)
                .Distinct()
                .ToArray();

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
                    LdapAttributeMapBase<TObject> map,
                    string propertyName,
                    string schema) {
                Debug.Assert(map != null);
                Debug.Assert(schema != null);
                ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));
                ArgumentNullException.ThrowIfNull(schema, nameof(schema));

                this._property = typeof(TObject).GetProperty(propertyName, PropertyFlags)!;
                if (this._property == null) {
                    var msg = Resources.ErrorPropertyMissing;
                    msg = string.Format(msg, propertyName, typeof(TObject).Name);
                    throw new ArgumentException(msg);
                }

                this._map = map;
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
                if (this._map.IsPrimaryGroupProperty != null) {
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
            private readonly LdapAttributeMapBase<TObject> _map;
            private readonly PropertyInfo _property;
            private readonly string _schema;
            #endregion
        }
        #endregion

        #region Nested class MapBuilder
        /// <summary>
        /// Provides the entry point to the fluent initialisation of the
        /// map.
        /// </summary>
        private class MapBuilder : ILdapAttributeMapBuilder<TObject> {

            #region Public constructors
            public MapBuilder(LdapAttributeMapBase<TObject> map,
                    string schema) {
                Debug.Assert(map != null);
                Debug.Assert(schema != null);
                this._map = map;
                this._schema = schema;
            }
            #endregion

            #region Public methods
            public ILdapPropertyMappingBuilder MapProperty(string propertyName)
                => new EntryBuilder(this._map, propertyName, this._schema);
            #endregion

            #region Private fields
            private readonly LdapAttributeMapBase<TObject> _map;
            private readonly string _schema;
            #endregion
        }
        #endregion

        #region Private fields
        private const BindingFlags PropertyFlags = BindingFlags.Public
            | BindingFlags.Instance;
        private readonly Dictionary<PropertyInfo, LdapAttributeAttribute> _properties;
        #endregion
    }
}
