// <copyright file="LdapUserMapper.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Reflection;
using System.Security.Claims;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Maps LDAP entries to user objects of type <typeparamref name="TUser"/>
    /// that inherit from <see cref="ILdapUser"/>.
    /// </summary>
    /// <remarks>
    /// <para>While the user object can be basically any class, this default
    /// implementation is for <see cref="ILdapUser"/>-derived classes where
    /// the implementation can find out about the LDAP attributes by means
    /// of <see cref="LdapAttributeAttribute"/>s.</para>
    /// </remarks>
    /// <typeparam name="TUser">The user object.</typeparam>
    public sealed class LdapUserMapper<TUser> : ILdapUserMapper<TUser>
            where TUser : ILdapUser {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The options determining where certain
        /// properties are stored on the LDAP server.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapUserMapper(LdapOptions options) {
            this._options = options
                ?? throw new ArgumentNullException(nameof(options));

            {
                var prop = typeof(TUser).GetProperty(nameof(ILdapUser.Identity));
                Debug.Assert(prop != null);
                this._identityAttribute = LdapAttributeAttribute
                    .GetLdapAttribute(prop, this._options.Schema);
            }

            {
                var pcs = from p in typeof(TUser).GetProperties()
                          let a = p.GetCustomAttributes<ClaimAttribute>()
                          where (p.PropertyType == typeof(string)) && (a != null) && a.Any()
                          select new {
                              Key = p,
                              Value = a.Select(aa => aa.Name)
                          };
                foreach (var p in pcs) {
                    this._propertyClaims.Add(p.Key, p.Value);
                }
            }

            this.RequiredAttributes = LdapAttributeAttribute
                .GetRequiredAttributes<TUser>(this._options.Schema);
        }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The options determining where certain
        /// properties are stored on the LDAP server.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapUserMapper(IOptions<LdapOptions> options)
            : this(options?.Value) { }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public IEnumerable<string> RequiredAttributes { get; }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public void Assign(TUser user,
                SearchResultEntry entry,
                LdapConnection connection,
                ILogger logger) {
            entry.AssignTo(user, this._options);

            var claims = user.Claims as IList<Claim>;
            if (claims != null) {
                // Reset all existing claims.
                claims.Clear();

                // Add the group claims.
                foreach (var c in entry.GetGroupClaims(this, connection,
                        this._options)) {
                    claims.Add(c);
                }

                // Add claims derived from properties that have already been
                // assigned before.
                foreach (var p in this._propertyClaims) {
                    var v = p.Key.GetValue(user) as string;

                    foreach (var c in p.Value) {
                        if (v != null) {
                            Debug.WriteLine($"Adding {c} claim as \"{v}\".");
                            claims.Add(new Claim(c, v));
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public string GetIdentity(TUser user) => user?.Identity;

        /// <inheritdoc />
        public string GetIdentity(SearchResultEntry entry) {
            return this._identityAttribute.GetValue(entry) as string;
        }
        #endregion

        #region Private fields
        private readonly LdapAttributeAttribute _identityAttribute;
        private readonly LdapOptions _options;
        private readonly Dictionary<PropertyInfo, IEnumerable<string>>
            _propertyClaims = new();
        #endregion
    }
}
