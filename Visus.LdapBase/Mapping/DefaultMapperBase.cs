// <copyright file="DefaultMapperBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Visus.Ldap.Configuration;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// The base class for an optimised
    /// <see cref="ILdapMapper{TEntry, TUser, TGroup}"/> for the default
    /// <see cref="LdapUser"/> and <see cref="LdapGroup"/> classes.
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    public abstract class DefaultMapperBase<TEntry>
            : ILdapMapper<TEntry, LdapUser, LdapGroup> {

        #region Public properties
        /// <inheritdoc />
        public bool GroupIsGroupMember => false;

        /// <inheritdoc />
        public string[] RequiredGroupAttributes { get; }

        /// <inheritdoc />
        public string[] RequiredUserAttributes { get; }

        /// <inheritdoc />
        public bool UserIsGroupMember => true;
        #endregion

        #region Public methods
        /// <inheritdoc />
        public string GetAccountName(LdapGroup group) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));
            return group.AccountName;
        }

        /// <inheritdoc />
        public string GetAccountName(LdapUser user) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return user.AccountName;
        }

        /// <inheritdoc />
        public string GetDistinguishedName(LdapGroup group) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));
            return group.DistinguishedName;
        }

        /// <inheritdoc />
        public string GetDistinguishedName(LdapUser user) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return user.DistinguishedName;
        }

        /// <inheritdoc />
        public string GetIdentity(LdapGroup group) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));
            return group.Identity;
        }

        /// <inheritdoc />
        public string GetIdentity(LdapUser user) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return user.Identity;
        }

        /// <inheritdoc />
        public LdapGroup MapGroup(TEntry entry, LdapGroup group) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public LdapUser MapUser(TEntry entry, LdapUser user) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public LdapUser SetGroups(LdapUser user,
                IEnumerable<LdapGroup> groups) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            ArgumentNullException.ThrowIfNull(groups, nameof(groups));
            user.Groups = groups;
            return user;
        }
        #endregion

        #region Protected constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        protected DefaultMapperBase(LdapOptionsBase options) {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            this.RequiredGroupAttributes = LdapAttributeAttribute
                .GetRequiredAttributes<LdapGroup>(options.Schema).ToArray();
            this.RequiredUserAttributes = LdapAttributeAttribute
                .GetRequiredAttributes<LdapUser>(options.Schema).ToArray();
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Gets the value of the specified LDAP <paramref name="attribute"/>
        /// from the given entry.
        /// </summary>
        /// <param name="entry">The entry to retrieve the attribute from.
        /// </param>
        /// <param name="attribute">Describes the attribute to retrieve,
        /// in particula its name and a potenial <see cref="IValueConverter"/>
        /// that should be used.</param>
        /// <returns>The value of the attribute or <c>null</c> if the attribute
        /// does not exit.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="entry"/>
        /// is <c>null</c>, or if <paramref name="attribute"/> is <c>null</c>.
        /// </exception>
        protected abstract object? GetAttribute(TEntry entry,
            LdapAttributeAttribute attribute);
        #endregion
    }
}
