// <copyright file="LdapAttributeMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Visus.Ldap.Configuration;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// The reflection-based default implementation of
    /// <see cref="IClaimMap{TObject}"/>, which enumerates all properties
    /// of <typeparamref name="TObject"/> that have a
    /// <see cref="LdapAttributeAttribute"/> for the current
    /// <see cref="LdapOptionsBase.Schema"/>.
    /// </summary>
    /// <typeparam name="TObject">The object to be reflected.</typeparam>
    /// <typeparam name="TOptions">The actual type of the LDAP options used by
    /// the library.</typeparam>
    public sealed class LdapAttributeMap<TObject, TOptions>
            : ILdapAttributeMap<TObject>
            where TOptions : LdapOptionsBase {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP configuration, which determines the
        /// LDAP schema to use.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapAttributeMap(IOptions<TOptions> options) {
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));

            var flags = BindingFlags.Instance;
            this._properties = (from p in typeof(TObject).GetProperties(flags)
                                let a = p.GetCustomAttributes<LdapAttributeAttribute>()
                                    .Where(a => a.Schema == this._options.Schema)
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
        #endregion

        #region Public properties
        /// <inheritdoc />
        public PropertyInfo? AccountNameProperty { get; }

        /// <inheritdoc />
        public IEnumerable<string> AttributeNames { get; }

        /// <inheritdoc />
        public IEnumerable<LdapAttributeAttribute> Attributes
            => this._properties.Values;

        /// <inheritdoc />
        public PropertyInfo? DistinguishedNameProperty { get; }

        /// <inheritdoc />
        public PropertyInfo? GroupMembershipsProperty { get; }

        /// <inheritdoc />
        public PropertyInfo? IdentityProperty { get; }

        /// <inheritdoc />
        public PropertyInfo? IsPrimaryGroupProperty { get; }

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

        #region Private fields
        private readonly TOptions _options;
        private readonly Dictionary<PropertyInfo,
            LdapAttributeAttribute> _properties;
        #endregion
    }
}
