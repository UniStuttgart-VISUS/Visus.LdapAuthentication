// <copyright file="ClaimsMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Visus.Ldap.Configuration;
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
            var flags = BindingFlags.Public | BindingFlags.Instance;
            this._claims = (from p in typeof(TObject).GetProperties(flags)
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
        #endregion

        #region Private fields
        private readonly Dictionary<LdapAttribute, IEnumerable<ClaimAttribute>>
            _claims;
        #endregion
    }
}
