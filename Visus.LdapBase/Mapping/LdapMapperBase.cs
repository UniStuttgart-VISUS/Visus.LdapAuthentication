// <copyright file="LdapMapperBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Visus.Ldap.Configuration;
using Visus.Ldap.Properties;
using LdapAttributeMap = System.Collections.Generic.Dictionary<
    System.Reflection.PropertyInfo,
    Visus.Ldap.Mapping.LdapAttributeAttribute>;


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
        public bool GroupIsGroupMember => (this._groupGroupMemberships != null);

        /// <inheritdoc />
        public string[] RequiredGroupAttributes { get; private set; }

        /// <inheritdoc />
        public string[] RequiredUserAttributes { get; private set; }

        /// <inheritdoc />
        public bool UserIsGroupMember => (this._userGroupMemberships != null);
        #endregion

        #region Public methods
        /// <inheritdoc />
        public string GetAccountName(TGroup group)
            => GetProperty<string>(group, this._groupAccountName);

        /// <inheritdoc />
        public string GetAccountName(TUser user)
            => GetProperty<string>(user, this._userAccountName);

        /// <inheritdoc />
        public string GetDistinguishedName(TGroup group)
            => GetProperty<string>(group, this._groupDistinguishedName);

        /// <inheritdoc />
        public string GetDistinguishedName(TUser user)
            => GetProperty<string>(user, this._userDistinguishedName);

        /// <inheritdoc />
        public string GetIdentity(TGroup group)
            => GetProperty<string>(group, this._groupIdentity);

        /// <inheritdoc />
        public string GetIdentity(TUser user)
            => GetProperty<string>(user, this._userIdentity);

        /// <inheritdoc />
        public TGroup MapGroup(TEntry entry, TGroup group) {
            foreach (var p in this._groupProperties) {
                p.Key.SetValue(group, this.GetAttribute(entry, p.Value));
            }
            return group;
        }

        /// <inheritdoc />
        public TUser MapUser(TEntry entry, TUser user) {
            foreach (var p in this._userProperties) {
                p.Key.SetValue(user, this.GetAttribute(entry, p.Value));
            }
            return user;
        }

        /// <inheritdoc />
        public TUser SetGroups(TUser user, IEnumerable<TGroup> groups) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            ArgumentNullException.ThrowIfNull(groups, nameof(groups));

            if (this._userGroupMemberships != null) {
                this._userGroupMemberships.SetValue(user, groups);
            }

            return user;
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
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        protected LdapMapperBase(LdapOptionsBase options) {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            this._groupAccountName = AccountNameAttribute.GetProperty<TGroup>();
            this._groupDistinguishedName = DistinguishedNameAttribute
                .GetProperty<TGroup>();
            this._groupGroupMemberships = GroupMembershipsAttribute
                .GetGroupMemberships<TGroup>();
            this._groupIdentity = IdentityAttribute.GetProperty<TGroup>();
            this._groupProperties = LdapAttributeAttribute.GetMap<TGroup>(
                options.Schema);

            this._userAccountName = AccountNameAttribute.GetProperty<TUser>();
            this._userDistinguishedName = DistinguishedNameAttribute
                .GetProperty<TUser>();
            this._userGroupMemberships = GroupMembershipsAttribute
                .GetGroupMemberships<TUser>();
            this._userIdentity = IdentityAttribute.GetProperty<TUser>();
            this._userProperties = LdapAttributeAttribute.GetMap<TUser>(
                options.Schema);

            this.RequiredGroupAttributes = this._groupProperties.Values
                .Select(a => a.Name)
                .Distinct()
                .ToArray();
            this.RequiredUserAttributes = this._userProperties.Values
                .Select(a => a.Name)
                .Distinct()
                .ToArray();
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

        #region Protected fields
        protected readonly PropertyInfo? _groupAccountName;
        protected readonly PropertyInfo? _groupDistinguishedName;
        protected readonly PropertyInfo? _groupGroupMemberships;
        protected readonly PropertyInfo? _groupIdentity;
        protected readonly LdapAttributeMap _groupProperties;
        protected readonly PropertyInfo? _userAccountName;
        protected readonly PropertyInfo? _userDistinguishedName;
        protected readonly PropertyInfo? _userGroupMemberships;
        protected readonly PropertyInfo? _userIdentity;
        protected readonly LdapAttributeMap _userProperties;
        #endregion
    }
}
