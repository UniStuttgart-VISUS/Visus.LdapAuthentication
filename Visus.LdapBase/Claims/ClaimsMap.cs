// <copyright file="ClaimsMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
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
using Attribute = Visus.Ldap.Mapping.LdapAttributeAttribute;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// Implements a <see cref="IUserClaimsMap"/> or
    /// <see cref="IGroupClaimsMap"/> using the claims derived from annotations
    /// in <typeparamref name="TObject"/>.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TOptions"></typeparam>
    internal sealed class ClaimsMap<TObject, TOptions>
            : IUserClaimsMap, IGroupClaimsMap
            where TOptions : LdapOptionsBase {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP options, which determine the schema to
        /// be used for reflecting LDAP attributes.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public ClaimsMap(IOptions<TOptions> options) {
            ArgumentNullException.ThrowIfNull(options?.Value, nameof(options));
            var flags = BindingFlags.Instance;
            this._claims = (from p in typeof(TObject).GetProperties(flags)
                            let a = p.GetCustomAttributes<Attribute>()
                                    .Where(a => a.Schema == options.Value.Schema)
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
        #endregion

        #region Public properties
        /// <inheritdoc />
        public IEnumerable<string> AttributeNames { get; }

        /// <inheritdoc />
        public IEnumerable<Attribute> Attributes => this._claims.Keys;
        #endregion

        #region Public methods
        /// <inheritdoc />
        public IEnumerator<KeyValuePair<Attribute, IEnumerable<ClaimAttribute>>>
            GetEnumerator() => this._claims.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => this._claims.GetEnumerator();
        #endregion

        #region Public indexers
        /// <inheritdoc />
        public IEnumerable<ClaimAttribute> this[Attribute attribute] {
            get => this._claims[attribute];
        }
        #endregion

        #region Private fields
        private readonly Dictionary<Attribute, IEnumerable<ClaimAttribute>>
            _claims;
        #endregion
    }
}
