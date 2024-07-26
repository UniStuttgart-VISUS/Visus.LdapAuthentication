// <copyright file="ClaimsBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Visus.Ldap.Mapping;
using ClaimMap = System.Collections.Generic.Dictionary<
    System.Reflection.PropertyInfo,
    System.Collections.Generic.IEnumerable<Visus.Ldap.Claims.ClaimAttribute>>;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// The default implementation of
    /// <see cref="IClaimsBuilder{TEntry, TUser, TGroup}"/> which is based on
    /// attribute annotations of the user and group objects.
    /// </summary>
    /// <typeparam name="TEntry">The type of the LDAP entry, which is dependent
    /// on the underlying library.</typeparam>
    /// <typeparam name="TUser">The type of the user to create the claims for.
    /// </typeparam>
    /// <typeparam name="TGroup">The type of the group to create the claims for.
    /// </typeparam>
    public abstract class ClaimsBuilder<TEntry, TUser, TGroup>
            : IClaimsBuilder<TEntry, TUser, TGroup> {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        public ClaimsBuilder() {
            this._groupClaims = ClaimAttribute.GetMap<TGroup>();
            this._groupGroups = GroupMembershipsAttribute
                .GetGroupMemberships<TGroup>();
            this._userClaims = ClaimAttribute.GetMap<TUser>();
            this._userGroups = GroupMembershipsAttribute
                .GetGroupMemberships<TUser>();
        }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public virtual bool SupportsDirectBuild => false;
        #endregion

        #region Public methods
        /// <inheritdoc />
        public virtual IEnumerable<Claim> GetClaims(
                TEntry user,
                IEnumerable<TEntry> groups,
                ClaimFilter? filter = null)
            => Enumerable.Empty<Claim>();

        /// <inheritdoc />
        public IEnumerable<Claim> GetClaims(TGroup group,
                ClaimFilter? filter = null) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));

            // Add claims derived from properties of the group.
            foreach (var p in this._groupClaims) {
                var v = p.Key.GetValue(group) as string;

                if (v != null) {
                    foreach (var c in p.Value) {
                        if (filter?.Invoke(c.Name, v) != false) {
                            yield return new(c.Name, v);
                        }
                    }
                }
            }

            // Add the claims derived from parent groups.
            foreach (var g in this.GetGroups(group)) {
                foreach (var c in this.GetClaims(g)) {
                    yield return c;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<Claim> GetClaims(TUser user,
                ClaimFilter? filter = null) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            // Add claims derived from properties of the user.
            foreach (var p in this._userClaims) {
                var v = p.Key.GetValue(user) as string;

                if (v != null) {
                    foreach (var c in p.Value) {
                        if (filter?.Invoke(c.Name, v) != false) {
                            yield return new(c.Name, v);
                        }
                    }
                }
            }

            // Add the claims derived from groups the user is member of.
            foreach (var g in this.GetGroups(user)) {
                foreach (var c in this.GetClaims(g)) {
                    yield return c;
                }
            }
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Gets the groups that <paramref name="group"/> is member of.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        protected IEnumerable<TGroup> GetGroups(TGroup group)
            => this._groupGroups?.GetValue(group) as IEnumerable<TGroup>
            ?? Enumerable.Empty<TGroup>();

        /// <summary>
        /// Gets the groups that <paramref name="user"/> is member of.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected IEnumerable<TGroup> GetGroups(TUser user)
            => this._userGroups?.GetValue(user) as IEnumerable<TGroup>
            ?? Enumerable.Empty<TGroup>();
        #endregion

        #region Private fields
        private readonly ClaimMap _groupClaims;
        private readonly PropertyInfo? _groupGroups;
        private readonly ClaimMap _userClaims;
        private readonly PropertyInfo? _userGroups;
        #endregion
    }
}
