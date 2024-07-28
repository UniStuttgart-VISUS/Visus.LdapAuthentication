// <copyright file="ClaimsBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// The default implementation of
    /// <see cref="IClaimsBuilder{TUser, TGroup}"/> which is based on
    /// attribute annotations of the user and group objects.
    /// </summary>
    /// <remarks>
    /// <para>This builder can be used for any user and group type that uses
    /// <see cref="ClaimAttribute"/>s to mark the properties of the user
    /// or group as a claim. Note that for adding group-based claims to a
    /// user automatically, the property holding the groups in the user
    /// must be marked with <see cref="GroupMembershipsAttribute"/> and
    /// return an <see cref="IEnumerable{T}"/> of
    /// <typeparamref name="TGroup"/>.</para>
    /// </remarks>
    /// <typeparam name="TUser">The type of the user to create the claims for.
    /// </typeparam>
    /// <typeparam name="TGroup">The type of the group to create the claims for.
    /// </typeparam>
    public sealed class ClaimsBuilder<TUser, TGroup>
            : IClaimsBuilder<TUser, TGroup> {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        public ClaimsBuilder(IClaimsMap userClaims,
                ILdapAttributeMap<TUser> userMap,
                IClaimsMap groupClaims,
                ILdapAttributeMap<TGroup> groupMap,
                IOptions<LdapOptionsBase> options) {
            this._groupClaims = groupClaims
                ?? throw new ArgumentNullException(nameof(groupClaims));
            this._groupGroups = GroupMembershipsAttribute
                .GetGroupMemberships<TGroup>();
            this._groupMap = groupMap
                ?? throw new ArgumentNullException(nameof(groupMap));
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
            this._userClaims = userClaims
                ?? throw new ArgumentNullException(nameof(userClaims));
            this._userGroups = GroupMembershipsAttribute
                .GetGroupMemberships<TUser>();
            this._userMap = userMap
                ?? throw new ArgumentNullException(nameof(userMap));
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public IEnumerable<Claim> GetClaims(TGroup group,
                ClaimFilter? filter = null) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));

            // Add claims derived from properties of the group.
            foreach (var p in this._groupMap) {
                var value = p.Key.GetValue(group) as string;

                if (value != null) {
                    var claims = this._groupClaims[p.Value];

                    foreach (var c in claims) {
                        if (filter?.Invoke(c.Name, value) != false) {
                            yield return new(c.Name, value);
                        }
                    }
                }
            }

            // Optionally add the primary group claim.
            {
                var c = this._options.PrimaryGroupIdentityClaim;

                if (!string.IsNullOrEmpty(c) && this.IsPrimaryGroup(group)) {
                    var value = this.GetIdentity(group);

                    if (value != null) {
                        if (filter?.Invoke(c, value) != false) {
                            yield return new(c, value);
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
            foreach (var p in this._userMap) {
                var value = p.Key.GetValue(user) as string;

                if (value != null) {
                    var claims = this._userClaims[p.Value];

                    foreach (var c in claims) {
                        if (filter?.Invoke(c.Name, value) != false) {
                            yield return new(c.Name, value);
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

        #region Private methods
        /// <summary>
        /// Gets the groups that <paramref name="group"/> is member of.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TGroup> GetGroups(TGroup group)
            => this._groupGroups?.GetValue(group) as IEnumerable<TGroup>
            ?? Enumerable.Empty<TGroup>();

        /// <summary>
        /// Gets the groups that <paramref name="user"/> is member of.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TGroup> GetGroups(TUser user)
            => this._userGroups?.GetValue(user) as IEnumerable<TGroup>
            ?? Enumerable.Empty<TGroup>();

        /// <summary>
        /// Gets the identity of the given <paramref name="group"/>.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private string? GetIdentity(TGroup group)
            => (this._groupMap.IdentityProperty != null)
            && (this._groupMap.IdentityProperty.GetValue(group) is string s)
            ? s : null;

        /// <summary>
        /// Answer whether <paramref name="group"/> is the primary group.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsPrimaryGroup(TGroup group)
            => (this._groupMap.IsPrimaryGroupProperty != null)
            && (this._groupMap.IsPrimaryGroupProperty.GetValue(group) is bool b)
            && b;
        #endregion

        #region Private fields
        private readonly IClaimsMap _groupClaims;
        private readonly ILdapAttributeMap<TGroup> _groupMap;
        private readonly PropertyInfo? _groupGroups;
        private readonly LdapOptionsBase _options;
        private readonly IClaimsMap _userClaims;
        private readonly PropertyInfo? _userGroups;
        private readonly ILdapAttributeMap<TUser> _userMap;
        #endregion
    }
}
