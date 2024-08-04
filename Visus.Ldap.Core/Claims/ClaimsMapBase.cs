// <copyright file="ClaimsMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Visus.Ldap.Configuration;
using Visus.Ldap.Properties;
using LdapAttribute = Visus.Ldap.Mapping.LdapAttributeAttribute;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// Implements a <see cref="IUserClaimsMap"/> or
    /// <see cref="IGroupClaimsMap"/> using the claims derived from annotations
    /// in <typeparamref name="TObject"/>.
    /// </summary>
    /// <typeparam name="TObject">The object used to reflect the claims from,
    /// which must be annotated with <see cref="LdapAttribute"/> and
    /// <see cref="ClaimAttribute"/> in order to automatically derive the claim
    /// mapping.</typeparam>
    public abstract class ClaimsMapBase<TObject>
            : IUserClaimsMap, IGroupClaimsMap {

        #region Public properties
        /// <inheritdoc />
        public IEnumerable<string> AttributeNames { get; }

        /// <inheritdoc />
        public IEnumerable<LdapAttribute> Attributes => this._claims.Keys;
        #endregion

        #region Public methods
        /// <inheritdoc />
        public IEnumerator<KeyValuePair<LdapAttribute, IEnumerable<ClaimAttribute>>>
            GetEnumerator() => this._claims.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => this._claims.GetEnumerator();
        #endregion

        #region Public indexers
        /// <inheritdoc />
        public IEnumerable<ClaimAttribute> this[LdapAttribute attribute] {
            get => this._claims.TryGetValue(attribute, out var retval)
                ? retval
                : Enumerable.Empty<ClaimAttribute>();
        }
        #endregion

        #region Protected constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP options, which determine the schema to
        /// be used for reflecting LDAP attributes.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        protected ClaimsMapBase(LdapOptionsBase options) {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            this._claims = (from p in typeof(TObject).GetProperties(PropertyFlags)
                            let a = p.GetCustomAttributes<LdapAttribute>()
                                    .Where(a => a.Schema == options.Schema)
                                    .FirstOrDefault()
                            let c = p.GetCustomAttributes<ClaimAttribute>()
                            where (a != null) && (c != null) && c.Any()
                            select new {
                                Key = a,
                                Value = c
                            }).ToDictionary(v => v.Key, v => v.Value);

            this.AttributeNames = this.Attributes.Select(a => a.Name)
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="mapper">A callback that dynamically creates the mapping
        /// using a <see cref="IClaimsMapBuilder"/>.</param>
        /// <param name="options">The LDAP configuration, which determines the
        /// LDAP schema to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="mapper"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        protected ClaimsMapBase(
                Action<IClaimsMapBuilder, LdapOptionsBase> mapper,
                LdapOptionsBase options) {
            ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            this._claims = new();
            mapper(new MapBuilder(this, options.Schema), options);

            this.AttributeNames = this.Attributes.Select(a => a.Name)
                .Distinct()
                .ToArray();
        }
        #endregion

        #region Nested class EntryBuilder
        /// <summary>
        /// Builder to adding the claims to an attribute.
        /// </summary>
        private class EntryBuilder(ClaimsMapBase<TObject> map,
                LdapAttribute attribute)
                : INewAttributeClaimsMapBuilder {

            #region Public methods
            /// <inheritdoc />
            public IAttributeClaimsMapBuilder ToClaim(string claim) {
                ArgumentException.ThrowIfNullOrEmpty(claim, nameof(claim));
                Debug.Assert(this._map != null);
                this._map._claims[this._attribute] = this.Existing.Append(
                    new(claim));
                return this;
            }

            /// <inheritdoc />
            public IAttributeClaimsMapBuilder ToClaims(
                    IEnumerable<string> claims) {
                ArgumentNullException.ThrowIfNull(claims, nameof(claims));
                Debug.Assert(this._map != null);
                this._map._claims[this._attribute] = this.Existing.Concat(
                    claims.Select(c => new ClaimAttribute(c)));
                return this;
            }

            /// <inheritdoc />
            public IAttributeClaimsMapBuilder WithConverter(Type converter) {
                Debug.Assert(this._attribute != null);
                ArgumentNullException.ThrowIfNull(converter, nameof(converter));
                this._attribute.Converter = converter;
                return this;
            }
            #endregion

            #region Private properties
            /// <summary>
            /// Gets the existing claims for <see cref="_attribute"/>
            /// </summary>
            private IEnumerable<ClaimAttribute> Existing
                => this._map._claims.TryGetValue(this._attribute, out var retval)
                ? retval
                : Enumerable.Empty<ClaimAttribute>();
            #endregion

            #region Private fields
            private readonly LdapAttribute _attribute = attribute
                ?? throw new ArgumentNullException(nameof(attribute));
            private readonly ClaimsMapBase<TObject> _map = map;
            #endregion
        }
        #endregion

        #region Nested class MapBuilder
        /// <summary>
        /// Provides the entry point to the fluent initialisation of the
        /// map.
        /// </summary>
        private class MapBuilder : IClaimsMapBuilder {

            #region Public constructors
            public MapBuilder(ClaimsMapBase<TObject> map, string schema) {
                Debug.Assert(map != null);
                Debug.Assert(schema != null);
                this._map = map;
                this._schema = schema;
            }
            #endregion

            #region Public methods
            /// <inheritdoc />
            public IAttributeClaimsMapBuilder MapAttribute(
                    LdapAttribute attribute) {
                ArgumentNullException.ThrowIfNull(attribute, nameof(attribute));

                if (attribute.Schema != this._schema) {
                    throw new ArgumentException(Resources.ErrorSchemaMismatch);
                }

                return new EntryBuilder(this._map, attribute);
            }

            /// <inheritdoc />
            public INewAttributeClaimsMapBuilder MapAttribute(
                    string attributeName)
                => new EntryBuilder(this._map,
                    new LdapAttribute(this._schema, attributeName));

            /// <inheritdoc />
            public IAttributeClaimsMapBuilder MapProperty(string propertyName) {
                (_, var a) = this.GetAnnotatedProperty(propertyName);
                return this.MapAttribute(a);
            }

            /// <inheritdoc />
            public void MapPropertyToAnnotatedClaims(string propertyName) {
                (var p, var a) = this.GetAnnotatedProperty(propertyName);
                var claims = p.GetCustomAttributes<ClaimAttribute>();

                if (!this._map._claims.TryGetValue(a, out var existing)) {
                    existing = Enumerable.Empty<ClaimAttribute>();
                }

                this._map._claims[a] = existing.Concat(claims);
            }
            #endregion

            #region Private methods
            private (PropertyInfo, LdapAttribute) GetAnnotatedProperty(
                    string propertyName) {
                var p = typeof(TObject).GetProperty(propertyName, PropertyFlags);
                if (p == null) {
                    var msg = Resources.ErrorPropertyMissing;
                    msg = string.Format(msg, propertyName, typeof(TObject).Name);
                    throw new ArgumentException(msg);
                }

                var a = p.GetCustomAttributes<LdapAttribute>()
                    .FirstOrDefault(a => a.Schema == this._schema);
                if (a == null) {
                    var msg = Resources.ErrorPropertyWithoutAttribute;
                    msg = string.Format(msg, propertyName, typeof(TObject).Name,
                        this._schema);
                    throw new ArgumentException(msg);
                }

                return (p, a);
            }
            #endregion

            #region Private fields
            private readonly ClaimsMapBase<TObject> _map;
            private readonly string _schema;
            #endregion
        }
        #endregion

        #region Private fields
        private const BindingFlags PropertyFlags = BindingFlags.Public
            | BindingFlags.Instance;
        private readonly Dictionary<LdapAttribute, IEnumerable<ClaimAttribute>>
            _claims;
        #endregion
    }
}
