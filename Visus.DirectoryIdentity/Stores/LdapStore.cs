// <copyright file="LdapStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication;
using Visus.DirectoryIdentity.Properties;
using Visus.Ldap.Claims;
using Visus.Ldap.Extensions;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryIdentity.Stores {

    /// <summary>
    /// The most basic read-only LDAP-backed user and role (group) store
    /// we can build.
    /// </summary>
    public class LdapStore<TUser, TRole> : IQueryableUserStore<TUser>,
            IQueryableRoleStore<TRole>,
            IRoleClaimStore<TRole>,
            IUserClaimStore<TUser>
            where TUser : class, new()
            where TRole : class, new() {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="searchService"></param>
        /// <param name="userMap"></param>
        /// <param name="roleMap"></param>
        /// <param name="claimsBuilder"></param>
        /// <param name="userClaims"></param>
        /// <param name="roleClaims"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException">If any of the parameters
        /// is <c>null</c>.</exception>
        public LdapStore(ILdapSearchService<TUser, TRole> searchService,
                ILdapAttributeMap<TUser> userMap,
                ILdapAttributeMap<TRole> roleMap,
                IClaimsBuilder<TUser, TRole> claimsBuilder,
                IUserClaimsMap userClaims,
                IGroupClaimsMap roleClaims,
                ILogger<LdapStore<TUser, TRole>> logger) {
            ArgumentNullException.ThrowIfNull(userClaims, nameof(userClaims));
            ArgumentNullException.ThrowIfNull(roleClaims, nameof(roleClaims));

            this._claimsBuilder = claimsBuilder
                ?? throw new ArgumentNullException(nameof(claimsBuilder));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._roleMap = roleMap
                ?? throw new ArgumentNullException(nameof(roleMap));
            this._searchService = searchService
                ?? throw new ArgumentNullException(nameof(searchService));
            this._userMap = userMap
                ?? throw new ArgumentNullException(nameof(userMap));

            // TODO: sanity checks of the maps.

            // Build a map from claim types to attribute names to obtain users
            // based on their claims. Note that we assume that the claims are
            // unique within a user, ie that only one property is mapped to
            // each claim.
            foreach (var map in userClaims) {
                foreach (var c in map.Value) {
                    this._userClaimAttributes.Add(c.Name, map.Key.Name);
                }
            }

            foreach (var map in roleClaims) {
                foreach (var c in map.Value) {
                    this._roleClaimAttributes.Add(c.Name, map.Key.Name);
                }
            }
        }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public IQueryable<TUser> Users
            => this._searchService.GetUsers().AsQueryable();

        /// <inheritdoc />
        public IQueryable<TRole> Roles
            => this._searchService.GetGroups().AsQueryable();
        #endregion

        #region Public methods
        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="claim"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task AddClaimAsync(
                TRole role,
                Claim claim,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="claims"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task AddClaimsAsync(
                TUser user,
                IEnumerable<Claim> claims,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> CreateAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> CreateAsync(
                TRole role,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(role, nameof(role));
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> DeleteAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> DeleteAsync(
                TRole role,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(role, nameof(role));
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <inheritdoc />
        public virtual void Dispose() {
            this._searchService?.Dispose();
        }

        /// <inheritdoc />
        Task<TRole?> IRoleStore<TRole>.FindByIdAsync(
                string roleID,
                CancellationToken cancellationToken)
            => this._searchService.GetGroupByIdentityAsync(roleID);

        /// <inheritdoc />
        Task<TUser?> IUserStore<TUser>.FindByIdAsync(
                string userID,
                CancellationToken cancellationToken)
            => this._searchService.GetUserByIdentityAsync(userID);

        /// <inheritdoc />
        Task<TRole?> IRoleStore<TRole>.FindByNameAsync(
                string normalisedRoleName,
                CancellationToken cancellationToken)
            => this._searchService.GetGroupByNameAsync(
                normalisedRoleName);

        /// <inheritdoc />
        Task<TUser?> IUserStore<TUser>.FindByNameAsync(
                string normalisedUserName,
                CancellationToken cancellationToken)
            => this._searchService.GetUserByAccountNameAsync(
                normalisedUserName);

        /// <inheritdoc />
        public Task<IList<Claim>> GetClaimsAsync(
                TRole role,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(role, nameof(role));
            var claims = this._claimsBuilder.GetClaims(role);
            return Task.FromResult((IList<Claim>) claims.ToList());
        }

        /// <inheritdoc />
        public Task<IList<Claim>> GetClaimsAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            var claims = this._claimsBuilder.GetClaims(user);
            return Task.FromResult((IList<Claim>) claims.ToList());
        }

        /// <inheritdoc />
        public virtual Task<string?> GetNormalizedRoleNameAsync(
                TRole role,
                CancellationToken cancellationToken)
            => this.GetRoleNameAsync(role, cancellationToken);

        /// <inheritdoc />
        public virtual Task<string?> GetNormalizedUserNameAsync(
                TUser user,
                CancellationToken cancellationToken)
            => this.GetUserNameAsync(user, cancellationToken);

        /// <inheritdoc />
        public virtual Task<string> GetRoleIdAsync(
                TRole role,
                CancellationToken cancellationToken)
            => Task.FromResult(this._roleMap.IdentityProperty
                ?.GetValue(role)
                ?.ToString()
                ?? string.Empty);

        /// <inheritdoc />
        public virtual Task<string?> GetRoleNameAsync(
                TRole role,
                CancellationToken cancellationToken)
            => Task.FromResult(this._roleMap.AccountNameProperty
                ?.GetValue(role) as string);

        /// <inheritdoc />
        public virtual Task<string> GetUserIdAsync(
                TUser user,
                CancellationToken cancellationToken)
            => Task.FromResult(this._userMap.IdentityProperty
                ?.GetValue(user)
                ?.ToString()
                ?? string.Empty);

        /// <inheritdoc />
        public virtual Task<string?> GetUserNameAsync(
                TUser user,
                CancellationToken cancellationToken)
            => Task.FromResult(this._userMap.IdentityProperty
                ?.GetValue(user) as string);

        /// <inheritdoc />
        public async Task<IList<TUser>> GetUsersForClaimAsync(
                Claim claim,
                CancellationToken cancellationToken) {
            if (this._userClaimAttributes.TryGetValue(claim.Type, out var ua)) {
                var c = claim.Value.EscapeLdapFilterExpression();
                var filter = $"({ua}={c})";
                this._logger.LogDebug("Obtaining users having an attribute "
                    + "{Attribute} with value {Value} ({EscapedValue}).",
                    ua, claim.Value, c);
                return (await this._searchService
                    .GetUsersAsync(filter, cancellationToken)
                    .ConfigureAwait(false))
                    .ToList();
            }

            if (this._roleClaimAttributes.TryGetValue(claim.Type, out var ra)) {
                var c = claim.Value.EscapeLdapFilterExpression();
                var filter = $"({ua}={c})";
                this._logger.LogDebug("Obtaining groups having an attribute "
                    + "{Attribute} with value {Value} ({EscapedValue}).",
                    ua, claim.Value, c);
                var groups = await this._searchService
                    .GetUsersAsync(filter, cancellationToken)
                    .ConfigureAwait(false);

                // TODO: The search service needs to be able to recursively search group members in order for this to work.
            }

            return Array.Empty<TUser>();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="claim"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task RemoveClaimAsync(
                TRole role,
                Claim claim,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="claims"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task RemoveClaimsAsync(
                TUser user,
                IEnumerable<Claim> claims,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="claim"></param>
        /// <param name="newClaim"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task ReplaceClaimAsync(
                TUser user,
                Claim claim,
                Claim newClaim,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="normalizedName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetNormalizedRoleNameAsync(
                TRole role,
                string? normalizedName,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(role, nameof(role));
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="normalisedName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetNormalizedUserNameAsync(
                TUser user,
                string? normalisedName,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="roleName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetRoleNameAsync(
                TRole role,
                string? roleName,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(role, nameof(role));
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetUserNameAsync(
                TUser user,
                string? userName,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> UpdateAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying users or groups in the directory.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> UpdateAsync(
                TRole role,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(role, nameof(role));
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }
        #endregion

        #region Protecte properties
        /// <summary>
        /// Gets the logger for the store.
        /// </summary>
        protected ILogger Logger => this._logger;

        /// <summary>
        /// Gets the mapping of role/group attributes.
        /// </summary>
        protected ILdapAttributeMap<TRole> RoleMap => this._roleMap;

        /// <summary>
        /// Gets the LDAP searching service.
        /// </summary>
        protected ILdapSearchService<TUser, TRole> SearchService
            => this._searchService;

        /// <summary>
        /// Gets the mapping of the user attributes.
        /// </summary>
        protected ILdapAttributeMap<TUser> UserMap => this._userMap;
        #endregion

        #region Private fields
        private readonly IClaimsBuilder<TUser, TRole> _claimsBuilder;
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _roleClaimAttributes = new();
        private readonly ILdapAttributeMap<TRole> _roleMap;
        private readonly ILdapSearchService<TUser, TRole> _searchService;
        private readonly Dictionary<string, string> _userClaimAttributes = new();
        private readonly ILdapAttributeMap<TUser> _userMap;
        #endregion
    }
}
