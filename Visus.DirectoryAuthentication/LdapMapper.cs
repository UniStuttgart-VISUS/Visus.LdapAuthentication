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
using Visus.DirectoryAuthentication.Properties;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Maps LDAP entries to user objects of type <typeparamref name="TUser"/>
    /// to the attributes retrieved from a <see cref="SearchResultEntry"/>.
    /// </summary>
    /// <remarks>
    /// <para>While the user object can be basically any class, this default
    /// implementation is for <see cref="ILdapUser"/>-derived classes where
    /// the implementation can find out about the LDAP attributes by means
    /// of <see cref="LdapAttributeAttribute"/>s.</para>
    /// </remarks>
    /// <typeparam name="TUser">The user object.</typeparam>
    public sealed class LdapMapper<TUser, TGroup> : ILdapMapper<TUser, TGroup> {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The options determining where certain
        /// properties are stored on the LDAP server.</param>
        /// <param name="claimsBuilder">A helper that can create
        /// <see cref="System.Security.Claims.Claim"/>s from a user object. This
        /// parameter may be <c>null</c>, in which case the mapper will not
        /// assign any claims to the user object. If
        /// <typeparamref name="TUser" /> is not annotated with
        /// <see cref="ClaimsAttribute"/>, this parameter is irrelevant.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapMapper(LdapOptions options,
                IClaimsBuilder<TUser, TGroup> claimsBuilder = null) {
            this._claimsBuilder = claimsBuilder;
            this._options = options
                ?? throw new ArgumentNullException(nameof(options));

            this._claimsProperty = ClaimsAttribute.GetClaims<TUser>();
            // Note: Claims are optional, so we do not check this.

            this._identityProperty = LdapIdentityAttribute.GetLdapIdentity<
                TUser>();
            if (this._identityProperty == null) {
                throw new ArgumentException(string.Format(
                    Resources.ErrorNoIdentity,
                    typeof(TUser).FullName));
            }

            this._identityAttribute = LdapAttributeAttribute.GetLdapAttribute(
                this._identityProperty, this._options.Schema);
            if (this._identityAttribute == null) {
                throw new ArgumentException(string.Format(
                    Resources.ErrorNoLdapAttribute,
                    this._identityProperty.Name));
            }

            this.RequiredGroupAttributes = LdapAttributeAttribute
                .GetRequiredAttributes<TGroup>(this._options.Schema);
            this.RequiredUserAttributes = LdapAttributeAttribute
                .GetRequiredAttributes<TUser>(this._options.Schema);
        }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The options determining where certain
        /// properties are stored on the LDAP server.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapMapper(IOptions<LdapOptions> options,
                IClaimsBuilder<TUser, TGroup> claimsBuilder = null)
            : this(options?.Value, claimsBuilder) { }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public IEnumerable<string> RequiredGroupAttributes { get; }

        /// <inheritdoc />
        public IEnumerable<string> RequiredUserAttributes { get; }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public void Assign(TUser user,
                SearchResultEntry entry,
                LdapConnection connection,
                ILogger logger) {
            entry.AssignTo(user, this._options);

            // If the user object expects to have claims set, we do so.
            if ((this._claimsBuilder != null)
                    && (this._claimsProperty != null)) {
                var claims = this._claimsBuilder.UseMapper(this).Build(user);
                this._claimsProperty.SetValue(user, claims);
            }
        }

        /// <inheritdoc />
        public IEnumerable<TGroup> GetGroups(TUser user) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string GetIdentity(TUser user)
            => this._identityProperty.GetValue(user) as string;

        /// <inheritdoc />
        public string GetIdentity(SearchResultEntry entry)
            => this._identityAttribute.GetValue(entry) as string;
        #endregion

        #region Private fields
        private readonly PropertyInfo _claimsProperty;
        private readonly IClaimsBuilder<TUser, TGroup> _claimsBuilder;
        private readonly LdapAttributeAttribute _identityAttribute;
        private readonly PropertyInfo _identityProperty;
        private readonly LdapOptions _options;
        #endregion
    }
}
