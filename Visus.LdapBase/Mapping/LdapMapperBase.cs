// <copyright file="LdapMapperBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Visus.Ldap.Configuration;
using Visus.Ldap.Properties;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Base class for implementations of
    /// <see cref="ILdapMapper{TEntry, TUser, TGroup}"/> that use the attributes
    /// defined in the library to assign LDAP attributes to properties.
    /// </summary>
    /// <typeparam name="TEntry">The type of the LDAP entry where from to obtain
    /// the attribute values.</typeparam>
    /// <typeparam name="TUser">The type used to represent a user.</typeparam>
    /// <typeparam name="TGroup">The type used to represent a group.</typeparam>
    public abstract class LdapMapperBase<TEntry, TUser, TGroup>
            : ILdapMapper<TEntry, TUser, TGroup> {

        #region Public properties
        /// <inheritdoc />
        public virtual bool GroupIsGroupMember
            => (this._groupMap.GroupMembershipsProperty != null);

        /// <inheritdoc />
        public virtual IEnumerable<string> RequiredGroupAttributes
            => this._groupMap.AttributeNames;

        /// <inheritdoc />
        public virtual IEnumerable<string> RequiredUserAttributes
            => this._userMap.AttributeNames;

        /// <inheritdoc />
        public virtual bool UserIsGroupMember
            => (this._userMap.GroupMembershipsProperty != null);
        #endregion

        #region Public methods
        /// <inheritdoc />
        public virtual string GetAccountName(TGroup group)
            => GetProperty<string>(group, this._groupMap.AccountNameProperty);

        /// <inheritdoc />
        public virtual string GetAccountName(TUser user)
            => GetProperty<string>(user, this._userMap.AccountNameProperty);

        /// <inheritdoc />
        public virtual string GetDistinguishedName(TGroup group)
            => GetProperty<string>(group,
                this._groupMap.DistinguishedNameProperty);

        /// <inheritdoc />
        public virtual string GetDistinguishedName(TUser user)
            => GetProperty<string>(user,
                this._userMap.DistinguishedNameProperty);

        /// <inheritdoc />
        public virtual string GetIdentity(TGroup group)
            => GetProperty<string>(group, this._groupMap.IdentityProperty);

        /// <inheritdoc />
        public virtual string GetIdentity(TUser user)
            => GetProperty<string>(user, this._userMap.IdentityProperty);

        /// <inheritdoc />
        public virtual TGroup MapGroup(TEntry entry, TGroup group) {
            foreach (var p in this._groupMap) {
                p.Key.SetValue(group, this.GetAttribute(entry, p.Value));
            }
            return group;
        }

        /// <inheritdoc />
        public virtual TUser MapUser(TEntry entry, TUser user) {
            foreach (var p in this._userMap) {
                p.Key.SetValue(user, this.GetAttribute(entry, p.Value));
            }
            return user;
        }

        /// <inheritdoc />
        public virtual TUser SetGroups(TUser user, IEnumerable<TGroup> groups) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            ArgumentNullException.ThrowIfNull(groups, nameof(groups));
            this._userMap.GroupMembershipsProperty?.SetValue(user, groups);
            return user;
        }

        /// <inheritdoc />
        public virtual TGroup SetGroups(TGroup group,
                IEnumerable<TGroup> groups) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));
            ArgumentNullException.ThrowIfNull(groups, nameof(groups));
            this._groupMap.GroupMembershipsProperty?.SetValue(group, groups);
            return group;
        }

        /// <inheritdoc />
        public virtual TGroup SetPrimary(TGroup group, bool isPrimary) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));
            this._groupMap.IsPrimaryGroupProperty?.SetValue(group, isPrimary);
            return group;
        }
        #endregion

        #region Protected class methods
        /// <summary>
        /// Gets the specified <paramref name="property"/> from
        /// <paramref name="obj"/> or throws an exception if the
        /// <paramref name="property"/> is <c>null</c> or does not have the
        /// requested type <typeparamref name="TValue"/> or is <c>null</c>
        /// by itself.
        /// </summary>
        /// <typeparam name="TValue">The type of the value we expect to
        /// find.</typeparam>
        /// <typeparam name="TObject">The type of the object we intend to
        /// get the property value from.</typeparam>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <param name="objName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        protected static TValue GetProperty<TValue, TObject>(
                TObject obj,
                PropertyInfo? property,
                string objName) {
            ArgumentNullException.ThrowIfNull(obj, objName ?? nameof(obj));

            if (property != null) {
                if (property.GetValue(obj) is TValue retval) {
                    if (retval != null) {
                        return retval;
                    }
                }
            }

            throw new ArgumentException(Resources.ErrorAttributeNotMapped,
                objName ?? nameof(obj));
        }

        /// <summary>
        /// Convenience method for
        /// <see cref="GetProperty{TValue, TObject}(TObject, PropertyInfo?,
        /// string)"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value we expect to
        /// find.</typeparam>
        /// <param name="group"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static TValue GetProperty<TValue>(TGroup group,
                PropertyInfo? property)
            => GetProperty<TValue, TGroup>(group, property, nameof(group));

        /// <summary>
        /// Convenience method for
        /// <see cref="GetProperty{TValue, TObject}(TObject, PropertyInfo?,
        /// string)"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value we expect to
        /// find.</typeparam>
        /// <param name="user"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static TValue GetProperty<TValue>(TUser user,
                PropertyInfo? property)
            => GetProperty<TValue, TUser>(user, property, nameof(user));
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
        protected LdapMapperBase(IOptions<LdapOptionsBase> options,
                ILdapAttributeMap<TUser> userMap,
                ILdapAttributeMap<TGroup> groupMap) {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            this._groupMap = groupMap
                ?? throw new ArgumentNullException(nameof(groupMap));
            this._userMap = userMap
                ?? throw new ArgumentNullException(nameof(userMap));
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
        /// does not exist.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="entry"/>
        /// is <c>null</c>, or if <paramref name="attribute"/> is <c>null</c>.
        /// </exception>
        protected abstract object? GetAttribute(TEntry entry,
            LdapAttributeAttribute attribute);
        #endregion

        #region Protected fields
        protected readonly ILdapAttributeMap<TGroup> _groupMap;
        protected readonly ILdapAttributeMap<TUser> _userMap;
        #endregion
    }
}
