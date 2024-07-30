// <copyright file="DefaultMapperBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Visus.Ldap.Configuration;
using Visus.Ldap.Properties;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// The base class for an optimised
    /// <see cref="ILdapMapper{TEntry, TUser, TGroup}"/> for the default
    /// <see cref="LdapUser"/> and <see cref="LdapGroup"/> classes, which
    /// accesses well-known properties of the user and group object directly
    /// rather than relying on reflection like
    /// <see cref="LdapMapperBase{TEntry, TUser, TGroup}"/>.
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    public abstract class DefaultMapperBase<TEntry>
            : LdapMapperBase<TEntry, LdapUser, LdapGroup> {

        #region Public properties
        /// <inheritdoc />
        public override bool GroupIsGroupMember => false;

        /// <inheritdoc />
        public override bool UserIsGroupMember => true;
        #endregion

        #region Public methods
        /// <inheritdoc />
        public override string GetAccountName(LdapGroup group) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));
            return group.AccountName;
        }

        /// <inheritdoc />
        public override string GetAccountName(LdapUser user) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return user.AccountName;
        }

        /// <inheritdoc />
        public override string GetDistinguishedName(LdapGroup group) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));
            return group.DistinguishedName;
        }

        /// <inheritdoc />
        public override string GetDistinguishedName(LdapUser user) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return user.DistinguishedName;
        }

        /// <inheritdoc />
        public override string GetIdentity(LdapGroup group) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));
            return group.Identity;
        }

        /// <inheritdoc />
        public override string GetIdentity(LdapUser user) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return user.Identity;
        }

        /// <inheritdoc />
        public override LdapGroup MapGroup(TEntry entry, LdapGroup group) {
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));
            ArgumentNullException.ThrowIfNull(group, nameof(group));

            group.AccountName = this.GetRequiredAttribute<string>(
                entry, this._groupMap.AccountNameAttribute!);
            group.DisplayName = this.GetAttribute<string>(
                entry, this._groupDisplayName);
            group.DistinguishedName = this.GetRequiredAttribute<string>(
                entry, this._groupMap.DistinguishedNameAttribute!);
            group.Identity = this.GetRequiredAttribute<string>(
                entry, this._groupMap.IdentityAttribute!);

            return group;
        }

        /// <inheritdoc />
        public override LdapUser MapUser(TEntry entry, LdapUser user) {
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            user.AccountName = this.GetRequiredAttribute<string>(
                entry, this._userMap.AccountNameAttribute!);
            user.ChristianName = this.GetAttribute<string>(
                entry, this._userChristianName);
            user.DisplayName = this.GetAttribute<string>(
                entry, this._userDisplayName);
            user.DistinguishedName = this.GetRequiredAttribute<string>(
                entry, this._userMap.DistinguishedNameAttribute!);
            user.EmailAddress = this.GetAttribute<string>(
                entry, this._userEmailAddress);
            user.Identity = this.GetRequiredAttribute<string>(
                entry, this._userMap.IdentityAttribute!);
            user.Surname = this.GetAttribute<string>(
                entry, this._userSurname);

            return user;
        }

        /// <inheritdoc />
        public override LdapUser SetGroups(LdapUser user,
                IEnumerable<LdapGroup> groups) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            ArgumentNullException.ThrowIfNull(groups, nameof(groups));
            user.Groups = groups.Distinct().ToList();
            return user;
        }

        /// <inheritdoc />
        public override LdapGroup SetPrimary(LdapGroup group, bool isPrimary) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));
            group.IsPrimary = isPrimary;
            return group;
        }
        #endregion

        #region Protected constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="userMap"></param>
        /// <param name="groupMap"></param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        protected DefaultMapperBase(IOptions<LdapOptionsBase> options,
                ILdapAttributeMap<LdapUser> userMap,
                ILdapAttributeMap<LdapGroup> groupMap)
                : base(options, userMap, groupMap) {
            Debug.Assert(options != null);
            Debug.Assert(options.Value != null);

            this._groupDisplayName = GetLdapAttribute<LdapGroup>(
                nameof(LdapGroup.DisplayName), options.Value);

            this._userChristianName = GetLdapAttribute<LdapUser>(
                nameof(LdapUser.ChristianName), options.Value);
            this._userDisplayName = GetLdapAttribute<LdapUser>(
                nameof(LdapUser.DisplayName), options.Value);
            this._userEmailAddress = GetLdapAttribute<LdapUser>(
                nameof(LdapUser.EmailAddress), options.Value);
            this._userSurname= GetLdapAttribute<LdapUser>(
                nameof(LdapUser.Surname), options.Value);
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Gets the value of the specified LDAP <paramref name="attribute"/> and
        /// tries to cast it to the specified type or returns <c>null</c> or the
        /// <c>default</c> value.
        /// </summary>
        /// <typeparam name="TValue">The value the attribute should be converted
        /// to.</typeparam>
        /// <param name="entry">The entry to retrieve the attribute from.
        /// </param>
        /// <param name="attribute">Describes the attribute to retrieve,
        /// in particula its name and a potenial <see cref="IValueConverter"/>
        /// that should be used.</param>
        /// <returns>The value of the attribute or <c>null</c> if the attribute
        /// does not exist or does not have the requested
        /// <typeparamref name="TValue"/> type.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="entry"/>
        /// is <c>null</c>, or if <paramref name="attribute"/> is <c>null</c>.
        /// </exception>
        protected TValue? GetAttribute<TValue>(TEntry entry,
                LdapAttributeAttribute attribute) {
            var value = this.GetAttribute(entry, attribute);
            return (value is TValue retval) ? retval : default;
        }

        /// <summary>
        /// Gets the value of the specified LDAP <paramref name="attribute"/> and
        /// converts it to the specified type or throws an exception.
        /// </summary>
        /// <typeparam name="TValue">The value the attribute should be converted
        /// to.</typeparam>
        /// <param name="entry">The entry to retrieve the attribute from.
        /// </param>
        /// <param name="attribute">Describes the attribute to retrieve,
        /// in particula its name and a potenial <see cref="IValueConverter"/>
        /// that should be used.</param>
        /// <returns>The requested value.</returns>
        /// <exception cref="InvalidOperationException">If
        /// <paramref name="entry"/> does not have the requested
        /// <paramref name="attribute"/>.</exception>
        /// <exception cref="InvalidCastException">If the
        /// <paramref name="entry"/> has the requested
        /// <paramref name="attribute"/>, but it does not have the requested
        /// type.</exception>
        protected TValue GetRequiredAttribute<TValue>(TEntry entry,
                LdapAttributeAttribute attribute) {
            var value = this.GetAttribute(entry, attribute);
            Debug.Assert(entry != null);
            Debug.Assert(attribute != null);

            if (value == null) {
                var msg = Resources.ErrorMissingRequiredAttribute;
                msg = string.Format(msg, entry, attribute.Name);
                throw new InvalidOperationException(msg);
            }

            return (TValue) value;
        }
        #endregion

        #region Private class methods
        /// <summary>
        /// Gets the <see cref="LdapAttributeAttribute"/> for the given
        /// <paramref name="property"/> of the specified
        /// <typeparamref name="TType"/>.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="property"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static LdapAttributeAttribute GetLdapAttribute<TType>(
                string property, LdapOptionsBase options) {
            Debug.Assert(property != null);
            Debug.Assert(options != null);

            var propInfo = typeof(TType).GetProperty(property)!;
            Debug.Assert(propInfo != null);

            var retval = (from a in propInfo.GetCustomAttributes<LdapAttributeAttribute>()
                          where a.Schema == options.Schema
                          select a).FirstOrDefault();

            if (retval == null) {
                var msg = Resources.ErrorMissingLdapAnnotation;
                msg = string.Format(msg, property, typeof(TType).Name);
                throw new InvalidOperationException(msg);
            }

            return retval;
        }
        #endregion

        #region Private fields
        private readonly LdapAttributeAttribute _groupDisplayName;
        private readonly LdapAttributeAttribute _userChristianName;
        private readonly LdapAttributeAttribute _userDisplayName;
        private readonly LdapAttributeAttribute _userEmailAddress;
        private readonly LdapAttributeAttribute _userSurname;
        #endregion
    }
}
